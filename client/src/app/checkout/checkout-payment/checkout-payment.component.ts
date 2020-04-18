import {
  Component,
  Input,
  AfterViewInit,
  ViewChild,
  ElementRef,
  OnDestroy,
} from '@angular/core';
import { FormGroup, AbstractControl } from '@angular/forms';
import { BasketService } from 'src/app/basket/basket.service';
import { CheckoutService } from '../checkout.service';
import { ToastrService } from 'ngx-toastr';
import { IBasket } from 'src/app/shared/models/basket';
import { IOrderToCreate, IOrder } from 'src/app/shared/models/order';
import { Router, NavigationExtras } from '@angular/router';

declare var Stripe;

@Component({
  selector: 'app-checkout-payment',
  templateUrl: './checkout-payment.component.html',
  styleUrls: ['./checkout-payment.component.scss'],
})
export class CheckoutPaymentComponent implements AfterViewInit, OnDestroy {
  @Input() checkoutForm: FormGroup;
  @ViewChild('cardNumber', { static: true }) cardNumberElement: ElementRef;
  @ViewChild('cardExpiry', { static: true }) cardExpiryElement: ElementRef;
  @ViewChild('cardCvc', { static: true }) cardCvcElement: ElementRef;
  stripe: any;
  cardNumber: any;
  cardExpiry: any;
  cardCvc: any;
  cardErrors: any;
  cardHandler = this.onChange.bind(this);
  loading = false;
  cardNumberValid = false;
  cardExpiryValid = false;
  cardCvcValid = false;

  get paymentForm(): AbstractControl {
    return this.checkoutForm.get('paymentForm');
  }

  get nameOnCard(): AbstractControl {
    return this.paymentForm.get('nameOnCard');
  }

  get deliveryForm(): AbstractControl {
    return this.checkoutForm.get('deliveryForm');
  }

  get deliveryMethod(): AbstractControl {
    return this.deliveryForm.get('deliveryMethod');
  }

  get addressForm(): AbstractControl {
    return this.checkoutForm.get('addressForm');
  }

  get canSubmitOrder(): boolean {
    return !this.loading
      && this.paymentForm.valid
      && this.cardNumberValid
      && this.cardExpiryValid
      && this.cardNumberValid;
  }

  constructor(
    private basketService: BasketService,
    private checkoutService: CheckoutService,
    private toastr: ToastrService,
    private router: Router
  ) {}

  ngAfterViewInit(): void {
    this.stripe = Stripe('pk_test_98KmQCTsvWeqJ1UHkoVqV95Y00wcPXACSs');
    const elements = this.stripe.elements();

    this.cardNumber = elements.create('cardNumber');
    this.cardNumber.mount(this.cardNumberElement.nativeElement);
    this.cardNumber.addEventListener('change', this.cardHandler);

    this.cardExpiry = elements.create('cardExpiry');
    this.cardExpiry.mount(this.cardExpiryElement.nativeElement);
    this.cardExpiry.addEventListener('change', this.cardHandler);

    this.cardCvc = elements.create('cardCvc');
    this.cardCvc.mount(this.cardCvcElement.nativeElement);
    this.cardCvc.addEventListener('change', this.cardHandler);
  }

  ngOnDestroy(): void {
    this.cardNumber.destroy();
    this.cardExpiry.destroy();
    this.cardCvc.destroy();
  }

  onChange(event) {
    this.cardErrors = null;

    if (event.error) {
      this.cardErrors = event.error.message;
    }

    switch (event.elementType) {
      case 'cardNumber':
        this.cardNumberValid = event.complete;
        break;

      case 'cardExpiry':
        this.cardExpiryValid = event.complete;
        break;

      case 'cardCvc':
        this.cardCvcValid = event.complete;
        break;

      default:
        break;
    }
  }

  async submitOrder() {
    this.loading = true;
    const basket = this.basketService.getCurrentBasketValue();

    try {
      const createdOrder = await this.creatOrder(basket);
      const paymentResult = await this.confirmPaymentWithStripe(basket);

      if (paymentResult.paymentIntent) {
        this.basketService.deleteBasket(basket);
        const navigationExtras: NavigationExtras = { state: createdOrder };
        this.router.navigate(['checkout/success'], navigationExtras);
      } else {
        this.toastr.error(paymentResult.error.message);
      }
      this.loading = false;
      } catch (error) {
      console.log(error);
      this.loading = false;
    }
  }

  private async confirmPaymentWithStripe(basket: IBasket) {
    return this.stripe.confirmCardPayment(basket.clientSecret, {
      payment_method: {
        card: this.cardNumber,
        billing_details: {
          name: this.nameOnCard.value,
        },
      },
    });
  }

  private async creatOrder(basket: IBasket) {
    const orderToCreate = this.getOrderToCreate(basket);

    return this.checkoutService.createOrder(orderToCreate).toPromise();
  }

  private getOrderToCreate(basket: IBasket): IOrderToCreate {
    const order: IOrderToCreate = {
      basketId: basket.id,
      deliveryMethodId: +this.deliveryMethod.value,
      shipToAddress: this.addressForm.value,
    };

    return order;
  }
}
