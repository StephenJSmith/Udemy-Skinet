export interface IPagination {
  pageIndex: number;
  pageSize: number;
  count: number;
  data: Datum[];
}

interface Datum {
  id: number;
  name: string;
  description: string;
  price: number;
  pictureUrl: string;
  productType: string;
  productBrand: string;
}
