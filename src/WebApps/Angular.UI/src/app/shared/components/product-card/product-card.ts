import { Component, input, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Product } from '../../../core/models/product.model';
import { BasketService } from '../../../services/basket.service';

@Component({
  selector: 'app-product-card',
  imports: [RouterLink],
  templateUrl: './product-card.html',
  styleUrl: './product-card.scss'
})
export class ProductCard {
  readonly product = input.required<Product>();
  private readonly basketService = inject(BasketService);

  protected readonly isAdding = signal(false);

  addToCart(event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    
    this.isAdding.set(true);
    const item = this.product();
    
    this.basketService.addToCart(item.id, item.name, item.price, 1, 'Black').subscribe({
      next: () => {
        setTimeout(() => this.isAdding.set(false), 800);
      },
      error: () => {
        this.isAdding.set(false);
      }
    });
  }
}
