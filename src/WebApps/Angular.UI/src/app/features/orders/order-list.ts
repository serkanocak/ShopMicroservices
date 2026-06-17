import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrderingService } from '../../services/ordering.service';
import { Order } from '../../core/models/order.model';

@Component({
  selector: 'app-order-list',
  imports: [RouterLink],
  templateUrl: './order-list.html',
  styleUrl: './order-list.scss'
})
export class OrderList implements OnInit {
  private readonly orderingService = inject(OrderingService);

  protected readonly orders = signal<Order[]>([]);
  protected readonly isLoading = signal(true);

  ngOnInit(): void {
    const customerId = '58c49479-ec65-4de2-86e7-033c546291aa';
    
    this.orderingService.getOrdersByCustomer(customerId).subscribe({
      next: (res) => {
        this.orders.set(res.orders);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  getStatusName(status: number): string {
    switch (status) {
      case 1: return 'Draft';
      case 2: return 'Pending';
      case 3: return 'Completed';
      case 4: return 'Cancelled';
      default: return 'Unknown';
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 1: return 'status-draft';
      case 2: return 'status-pending';
      case 3: return 'status-completed';
      case 4: return 'status-cancelled';
      default: return '';
    }
  }

  getOrderTotal(order: Order): number {
    return order.orderItems.reduce((acc, item) => acc + (item.price * item.quantity), 0);
  }
}
