import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AsyncValidatorFn } from '@angular/forms';
import { AccountService } from '../account.service';
import { Router } from '@angular/router';
import { timer, of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  errors: string[];

  constructor(
    private fb: FormBuilder,
    private accontService: AccountService,
    private router: Router,
  ) { }

  ngOnInit(): void {
    this.createRegisterForm();
  }

  createRegisterForm() {
    this.registerForm = this.fb.group({
      displayName: [null, [Validators.required]],
      email: [null,
        [Validators.required,
        Validators.pattern('^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$')],
        [this.validateEmailNotTaken()]],
      password: [null, [Validators.required]]
    });
  }

  onSubmit() {
    this.accontService.register(this.registerForm.value)
      .subscribe(response => {
        this.router.navigateByUrl('/shop');
      }, error => {
        console.log(error);
        this.errors = error.errors;
      });
  }

  validateEmailNotTaken(): AsyncValidatorFn {
    return control => {
      return timer(500).pipe(
        switchMap(() => {
          if (!control.value) {
            return of(null);
          }

          return this.accontService.checkEmailExists(control.value)
            .pipe(
              map(res => {
                return res ? {emailExists: true} : null;
              })
            );
        })
      );
    };
  }
}
