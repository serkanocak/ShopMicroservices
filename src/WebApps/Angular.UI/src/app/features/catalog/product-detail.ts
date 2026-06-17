import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CatalogService } from '../../services/catalog.service';
import { BasketService } from '../../services/basket.service';
import { Product } from '../../core/models/product.model';

@Component({
  selector: 'app-product-detail',
  imports: [RouterLink],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss'
})
export class ProductDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly catalogService = inject(CatalogService);
  private readonly basketService = inject(BasketService);

  protected readonly product = signal<Product | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly isAdding = signal(false);
  protected readonly quantity = signal(1);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/products']);
      return;
    }

    this.catalogService.getProduct(id).subscribe({
      next: (res) => {
        this.product.set(res.product);
        this.isLoading.set(false);
      },
      error: () => {
        this.router.navigate(['/products']);
      }
    });
  }

  incrementQty(): void {
    this.quantity.update(q => q + 1);
  }

  decrementQty(): void {
    this.quantity.update(q => q > 1 ? q - 1 : 1);
  }

  addToCart(): void {
    const prod = this.product();
    if (!prod) return;

    this.isAdding.set(true);
    this.basketService.addToCart(prod.id, prod.name, prod.price, this.quantity(), 'Black').subscribe({
      next: () => {
        this.isAdding.set(false);
        this.router.navigate(['/cart']);
      },
      error: () => {
        this.isAdding.set(false);
      }
    });
  }
}
