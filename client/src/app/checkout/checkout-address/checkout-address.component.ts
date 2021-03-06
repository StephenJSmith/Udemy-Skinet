import { Component, OnInit, Input } from '@angular/core';
import { FormGroup, AbstractControl } from '@angular/forms';
import { AccountService } from 'src/app/account/account.service';
import { ToastrService } from 'ngx-toastr';
import { IAddress } from 'src/app/shared/models/address';

@Component({
  selector: 'app-checkout-address',
  templateUrl: './checkout-address.component.html',
  styleUrls: ['./checkout-address.component.scss']
})
export class CheckoutAddressComponent implements OnInit {
  @Input() checkoutForm: FormGroup;

  get addressForm(): AbstractControl {
    return this.checkoutForm.get('addressForm');
  }

  get canSaveDefaultAddress(): boolean {
    const value = this.addressForm.valid
      && this.addressForm.dirty;

    return value;
  }

  constructor(
    private accountService: AccountService,
    private toastr: ToastrService,
  ) { }

  ngOnInit(): void {
  }

  saveUserAddress() {
    this.accountService.updateUserAddress(this.addressForm.value)
      .subscribe((address: IAddress) => {
        this.toastr.success('Address saved');
        this.addressForm.reset(address);
      }, error => {
        console.log(error);
      });
  }


}
