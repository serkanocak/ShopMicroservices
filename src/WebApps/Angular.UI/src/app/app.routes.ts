import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home').then(m => m.Home)
  },
  {
    path: 'products',
    loadComponent: () => import('./features/catalog/product-list').then(m => m.ProductList)
  },
  {
    path: 'products/:id',
    loadComponent: () => import('./features/catalog/product-detail').then(m => m.ProductDetail)
  },
  {
    path: 'cart',
    loadComponent: () => import('./features/cart/cart').then(m => m.Cart)
  },
  {
    path: 'checkout',
    loadComponent: () => import('./features/checkout/checkout').then(m => m.Checkout)
  },
  {
    path: 'confirmation',
    loadComponent: () => import('./features/confirmation/confirmation').then(m => m.Confirmation)
  },
  {
    path: 'orders',
    loadComponent: () => import('./features/orders/order-list').then(m => m.OrderList)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
