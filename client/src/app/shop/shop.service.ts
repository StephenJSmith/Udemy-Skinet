import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { IPagination, Pagination } from '../shared/models/pagination';
import { IBrand } from '../shared/models/brand';
import { IType } from '../shared/models/productType';
import { ShopParams } from '../shared/models/shopParams';
import { IProduct } from '../shared/models/product';

@Injectable({
  providedIn: 'root'
})
export class ShopService {
  baseUrl = environment.apiUrl;
  products: IProduct[] = [];
  brands: IBrand[] = [];
  types: IType[] = [];
  pagination = new Pagination();
  shopParams = new ShopParams();

  constructor(private http: HttpClient) { }

  getProducts(useCache: boolean) {
    if (!useCache) {
      this.products = [];
    }

    if (this.products.length && useCache) {
      const pagesReceived
        = Math.ceil(this.products.length / this.shopParams.pageSize);

      if (this.shopParams.pageNumber <= pagesReceived) {
        const start = (this.shopParams.pageNumber - 1) * this.shopParams.pageSize;
        const end = this.shopParams.pageNumber * this.shopParams.pageSize;
        this.pagination.data = this.products.slice(start, end);

        return of(this.pagination);
      }
    }

    let params = new HttpParams();

    if (this.shopParams.brandId !== 0) {
      params = params.append('brandId', this.shopParams.brandId.toString());
    }

    if (this.shopParams.typeId !== 0) {
      params = params.append('typeId', this.shopParams.typeId.toString());
    }

    if (this.shopParams.search) {
      params = params.append('search', this.shopParams.search);
    }

    params = params.append('sort', this.shopParams.sort);
    params = params.append('pageIndex', this.shopParams.pageNumber.toString());
    params = params.append('pageSize', this.shopParams.pageSize.toString());

    return this.http.get<IPagination>(
      this.baseUrl + 'products',
      {observe: 'response', params}
    ).pipe(
      map(response => {
        this.products = [...this.products, ...response.body.data];
        this.pagination = response.body;

        return this.pagination;
      })
    );
  }

  setShopParams(params: ShopParams) {
    this.shopParams = params;
  }

  getShopParams(): ShopParams {
    return this.shopParams;
  }

  getProduct(id: number) {
    const product = this.products.find(p => p.id === id);
    if (product) {
      return of(product);
    }

    return this.http.get<IProduct>(this.baseUrl + 'products/' + id);
  }

  getBrands() {
    if (this.brands.length > 0) {
      return of(this.brands);
    }

    return this.http.get<IBrand[]>(this.baseUrl + 'products/brands')
      .pipe(
        map(response => {
          this.brands = response;

          return response;
        })
      );
  }

  getTypes() {
    if (this.types.length > 0) {
      return of(this.types);
    }

    return this.http.get<IType[]>(this.baseUrl + 'products/types')
      .pipe(
        map(response => {
          this.types = response;

          return response;
        })
      );
  }
}
