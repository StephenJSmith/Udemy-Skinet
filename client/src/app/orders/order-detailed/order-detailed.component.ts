import { Component, OnInit } from '@angular/core';
import { IOrder } from 'src/app/shared/models/order';
import { ActivatedRoute } from '@angular/router';
import { BreadcrumbService } from 'xng-breadcrumb';
import { OrdersService } from '../orders.service';

@Component({
  selector: 'app-order-detailed',
  templateUrl: './order-detailed.component.html',
  styleUrls: ['./order-detailed.component.scss']
})
export class OrderDetailedComponent implements OnInit {
  order: IOrder;

  constructor(
    private route: ActivatedRoute,
    private breadcrumbService: BreadcrumbService,
    private ordersService: OrdersService,
  ) {
    this.breadcrumbService.set('@OrderDetailed', '');
   }

  ngOnInit(): void {
    const id = +this.route.snapshot.paramMap.get('id');
    this.getOrderDetailed(id);
  }

  getOrderDetailed(id: number) {
    this.ordersService.getOrderDetailed(id)
      .subscribe((order: IOrder) => {
        this.order = order;
        const value = `Order# ${order.id} - ${order.status}`;
        this.breadcrumbService.set('@OrderDetailed', value);
      }, error => {
        console.log(error);
      });
  }
}
