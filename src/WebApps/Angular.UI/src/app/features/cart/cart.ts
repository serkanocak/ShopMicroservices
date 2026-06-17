import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BasketService } from '../../services/basket.service';

@Component({
  selector: 'app-cart',
  imports: [RouterLink],
  templateUrl: './cart.html',
  styleUrl: './cart.scss'
})
export class Cart implements OnInit {
  readonly basketService = inject(BasketService);

  ngOnInit(): void {
    this.basketService.loadBasket('swn').subscribe();
  }

  removeItem(productId: string): void {
    this.basketService.removeFromCart(productId).subscribe();
  }

  updateQuantity(productId: string, currentQty: number, change: number): void {
    const newQty = currentQty + change;
    if (newQty <= 0) {
      this.removeItem(productId);
      return;
    }

    const currentCart = this.basketService.basketSignal();
    if (!currentCart) return;

    const items = currentCart.items.map(item => {
      if (item.productId === productId) {
        return { ...item, quantity: newQty };
      }
      return item;
    });

    this.basketService.storeBasket({ ...currentCart, items }).subscribe();
  }
}
