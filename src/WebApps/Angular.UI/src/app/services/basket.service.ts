import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { ShoppingCart, ShoppingCartItem, GetBasketResponse, StoreBasketResponse, StoreBasketRequest, DeleteBasketResponse, BasketCheckout, CheckoutBasketRequest, CheckoutBasketResponse } from '../core/models/basket.model';

@Injectable({
  providedIn: 'root'
})
export class BasketService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiGatewayUrl}/basket-service/basket`;

  // Reactive state using Signals
  readonly basketSignal = signal<ShoppingCart | null>(null);

  readonly cartItems = computed(() => this.basketSignal()?.items ?? []);
  readonly cartCount = computed(() => this.cartItems().reduce((acc, item) => acc + item.quantity, 0));
  readonly totalPrice = computed(() => this.cartItems().reduce((acc, item) => acc + (item.price * item.quantity), 0));

  getBasket(userName: string): Observable<GetBasketResponse> {
    return this.http.get<GetBasketResponse>(`${this.baseUrl}/${userName}`);
  }

  storeBasket(cart: ShoppingCart): Observable<StoreBasketResponse> {
    return this.http.post<StoreBasketResponse>(this.baseUrl, { cart } as StoreBasketRequest).pipe(
      tap(() => {
        // Sync local state
        this.basketSignal.set(cart);
      })
    );
  }

  deleteBasket(userName: string): Observable<DeleteBasketResponse> {
    return this.http.delete<DeleteBasketResponse>(`${this.baseUrl}/${userName}`).pipe(
      tap(() => {
        if (this.basketSignal()?.userName === userName) {
          this.basketSignal.set({ userName, items: [] });
        }
      })
    );
  }

  checkoutBasket(basketCheckoutDto: BasketCheckout): Observable<CheckoutBasketResponse> {
    return this.http.post<CheckoutBasketResponse>(`${this.baseUrl}/checkout`, { basketCheckoutDto } as CheckoutBasketRequest).pipe(
      tap((res) => {
        if (res.isSuccess) {
          // Clear basket locally upon checkout
          const userName = basketCheckoutDto.userName;
          this.basketSignal.set({ userName, items: [] });
        }
      })
    );
  }

  loadBasket(userName: string = 'swn'): Observable<ShoppingCart> {
    return this.getBasket(userName).pipe(
      map(res => res.cart),
      tap(cart => {
        this.basketSignal.set(cart);
      }),
      catchError(() => {
        // If 404 or other error, fallback to empty basket
        const emptyCart: ShoppingCart = { userName, items: [] };
        this.basketSignal.set(emptyCart);
        return of(emptyCart);
      })
    );
  }

  addToCart(productId: string, productName: string, price: number, quantity: number = 1, color: string = 'Black'): Observable<StoreBasketResponse> {
    const currentCart = this.basketSignal();
    const userName = currentCart?.userName ?? 'swn';
    const items = currentCart ? [...currentCart.items] : [];

    const existingItemIndex = items.findIndex(item => item.productId === productId);
    if (existingItemIndex > -1) {
      items[existingItemIndex] = {
        ...items[existingItemIndex],
        quantity: items[existingItemIndex].quantity + quantity
      };
    } else {
      items.push({
        productId,
        productName,
        price,
        quantity,
        color
      });
    }

    const updatedCart: ShoppingCart = { userName, items };
    return this.storeBasket(updatedCart);
  }

  removeFromCart(productId: string): Observable<StoreBasketResponse | null> {
    const currentCart = this.basketSignal();
    if (!currentCart) return of(null);

    const items = currentCart.items.filter(item => item.productId !== productId);
    const updatedCart: ShoppingCart = { userName: currentCart.userName, items };
    return this.storeBasket(updatedCart);
  }
}
