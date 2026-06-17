import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { GetOrdersResponse, GetOrdersByNameResponse, GetOrdersByCustomerResponse } from '../core/models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrderingService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiGatewayUrl}/ordering-service/orders`;

  getOrders(pageIndex: number = 1, pageSize: number = 10): Observable<GetOrdersResponse> {
    const params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<GetOrdersResponse>(this.baseUrl, { params });
  }

  getOrdersByName(orderName: string): Observable<GetOrdersByNameResponse> {
    return this.http.get<GetOrdersByNameResponse>(`${this.baseUrl}/${orderName}`);
  }

  getOrdersByCustomer(customerId: string): Observable<GetOrdersByCustomerResponse> {
    return this.http.get<GetOrdersByCustomerResponse>(`${this.baseUrl}/customer/${customerId}`);
  }
}
