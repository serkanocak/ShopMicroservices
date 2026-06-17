import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CatalogService } from '../../services/catalog.service';
import { Product } from '../../core/models/product.model';
import { ProductCard } from '../../shared/components/product-card/product-card';

@Component({
  selector: 'app-home',
  imports: [RouterLink, ProductCard],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home implements OnInit {
  private readonly catalogService = inject(CatalogService);
  
  protected readonly lastProducts = signal<Product[]>([]);
  protected readonly bestProducts = signal<Product[]>([]);
  protected readonly isLoading = signal(true);
  
  // Slider state
  protected readonly currentSlide = signal(0);
  protected readonly slides = [
    { image: 'images/banner/banner1.png', alt: 'Fresh Tech Deals' },
    { image: 'images/banner/banner2.png', alt: 'Exclusive Collections' },
    { image: 'images/banner/banner3.png', alt: 'Super Sale Event' }
  ];

  ngOnInit(): void {
    // Start slider rotation
    setInterval(() => {
      this.currentSlide.update(idx => (idx + 1) % this.slides.length);
    }, 5000);

    // Fetch products
    this.catalogService.getProducts(1, 10).subscribe({
      next: (res) => {
        const products = res.products;
        this.lastProducts.set(products.slice(0, 4));
        this.bestProducts.set(products.slice(-4));
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  setSlide(index: number): void {
    this.currentSlide.set(index);
  }
}
