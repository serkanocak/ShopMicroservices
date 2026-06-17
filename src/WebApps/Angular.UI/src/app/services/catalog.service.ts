import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { GetProductsResponse, GetProductByIdResponse, GetProductByCategoryResponse } from '../core/models/product.model';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiGatewayUrl}/catalog-service/products`;

  getProducts(pageNumber: number = 1, pageSize: number = 10): Observable<GetProductsResponse> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<GetProductsResponse>(this.baseUrl, { params });
  }

  getProduct(id: string): Observable<GetProductByIdResponse> {
    return this.http.get<GetProductByIdResponse>(`${this.baseUrl}/${id}`);
  }

  getProductsByCategory(category: string): Observable<GetProductByCategoryResponse> {
    return this.http.get<GetProductByCategoryResponse>(`${this.baseUrl}/category/${category}`);
  }
}
