import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { AccountService } from '../account/account.service';

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.scss']
})
export class CheckoutComponent implements OnInit {
  checkoutForm: FormGroup;

  get addressForm(): AbstractControl {
    return this.checkoutForm.get('addressForm');
  }

  get deliveryForm(): AbstractControl {
    return this.checkoutForm.get('deliveryForm');
  }

  get paymentForm(): AbstractControl {
    return this.checkoutForm.get('paymentForm');
  }

  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
  ) { }

  ngOnInit(): void {
    this.createCheckoutForm();
    this.getAddressFormValues();
  }

  createCheckoutForm() {
    this.checkoutForm = this.fb.group({
      addressForm: this.fb.group({
        firstName: [null, Validators.required],
        lastName: [null, Validators.required],
        street: [null, Validators.required],
        city: [null, Validators.required],
        state: [null, Validators.required],
        zipcode: [null, Validators.required],
      }),
      deliveryForm: this.fb.group({
        deliveryMethod: [null, Validators.required],
      }),
      paymentForm: this.fb.group({
        nameOnCard: [null, Validators.required],
      })
    });
  }

  getAddressFormValues() {
    this.accountService.getUserAddress()
      .subscribe(address => {
        if (address) {
          this.addressForm.patchValue(address);
        }
      }, error => {
        console.log(error);
      });
  }

}
