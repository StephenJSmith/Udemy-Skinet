import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ReplaySubject, of } from 'rxjs';
import { IUser } from '../shared/models/user';
import { map } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<IUser>(1);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router,
  ) { }

  loadCurrentUser(token: string) {
    if (token === null) {
      this.currentUserSource.next(null);

      return of(null);
    }

    const authValue = `Bearer ${token}`;
    const url = `${this.baseUrl}account`;
    let headers  = new HttpHeaders();
    headers = headers.set('Authorization', authValue);

    return this.http.get(url, {headers})
      .pipe(
        map((user: IUser) => {
          localStorage.setItem('token', user.token);
          this.currentUserSource.next(user);
        })
      );
  }

  login(values: any) {
    const url = `${this.baseUrl}account/login`;

    return this.http.post(url, values)
      .pipe(
        map((user: IUser) => {
          if (user) {
            localStorage.setItem('token', user.token);
            this.currentUserSource.next(user);
          }
        })
      );
  }

  register(values: any) {
    const url = `${this.baseUrl}account/register`;

    return this.http.post(url, values)
      .pipe(
        map((user: IUser) => {
          if (user) {
            localStorage.setItem('token', user.token);
            this.currentUserSource.next(user);
          }
        })
      );
  }

  logout() {
    localStorage.removeItem('token');
    this.currentUserSource.next(null);
    this.router.navigateByUrl('/');
  }

  checkEmailExists(email: string) {
    const url = `${this.baseUrl}account/emailexists?email=${email}`;

    return this.http.get(url);
  }
}
