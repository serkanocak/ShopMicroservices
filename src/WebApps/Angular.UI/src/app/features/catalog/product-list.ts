import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CatalogService } from '../../services/catalog.service';
import { Product } from '../../core/models/product.model';
import { ProductCard } from '../../shared/components/product-card/product-card';

@Component({
  selector: 'app-product-list',
  imports: [ProductCard],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss'
})
export class ProductList implements OnInit {
  private readonly catalogService = inject(CatalogService);

  protected readonly productList = signal<Product[]>([]);
  protected readonly categoryList = signal<string[]>([]);
  protected readonly selectedCategory = signal<string | null>(null);
  protected readonly isLoading = signal(true);

  // Computed signal to filter products reactively
  protected readonly filteredProducts = computed(() => {
    const cat = this.selectedCategory();
    const list = this.productList();
    if (!cat) return list;
    return list.filter(p => p.category.includes(cat));
  });

  ngOnInit(): void {
    this.catalogService.getProducts(1, 50).subscribe({
      next: (res) => {
        this.productList.set(res.products);
        
        // Extract distinct categories
        const categories = res.products.reduce((acc: string[], prod) => {
          prod.category.forEach(c => {
            if (!acc.includes(c)) acc.push(c);
          });
          return acc;
        }, []);
        
        this.categoryList.set(categories);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  selectCategory(category: string | null): void {
    this.selectedCategory.set(category);
  }
}
