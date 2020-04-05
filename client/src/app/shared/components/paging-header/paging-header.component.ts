import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-paging-header',
  templateUrl: './paging-header.component.html',
  styleUrls: ['./paging-header.component.scss']
})
export class PagingHeaderComponent implements OnInit {
  @Input() pageNumber: number;
  @Input() pageSize: number;
  @Input() totalCount: number;

  constructor() { }

  ngOnInit(): void {
  }

  displayedPageRange(): string {
    const lo = ((this.pageNumber - 1) * this.pageSize) + 1;
    const hi = Math.min(this.totalCount, this.pageNumber * this.pageSize);

    const result = `${lo} - ${hi}`;

    return result;
  }

  isProductsToDisplay(): boolean {
    return this.totalCount > 0;
  }
}
