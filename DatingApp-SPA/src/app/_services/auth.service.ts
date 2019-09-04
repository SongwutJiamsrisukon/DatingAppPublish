import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import {map} from 'rxjs/operators';

import {JwtHelperService} from '@auth0/angular-jwt';
import { environment } from 'src/environments/environment';
import { LocalUserData } from '../_models/localUserData';
import { BehaviorSubject } from 'rxjs';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  baseUrl = environment.apiUrl + 'auth/';
  jwtHelper = new JwtHelperService();
  decodeToken: any;
  localUserData: LocalUserData;
  photoUrl = new BehaviorSubject<string>('../../assets/user.png');
  currentPhotoUrl = this.photoUrl.asObservable(); // currentPhotoUrl use as observable, so we can subscribe this

  constructor(private http: HttpClient) { }

  // when user login call this
  changeMemberPhoto(photoUrl: string) {
    this.photoUrl.next(photoUrl); // next is use to update this.photoUrl.asObservable()
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token); // If token is not expired and it is jwt token, return true
  }

  logout() {
    localStorage.removeItem('token');
    this.decodeToken = null;
    localStorage.removeItem('localUserData');
    this.localUserData = null;
  }

  /** model is UserForLoginDto */
  login(model: any) {
    return this.http.post(this.baseUrl + 'login', model)
    .pipe(
      map((response: any) => {
        const responseObject = response;
        if (responseObject) {
          localStorage.setItem('token', responseObject.token);
          localStorage.setItem('localUserData', JSON.stringify(responseObject.localUserData)); // convert object to string
          this.decodeToken = this.jwtHelper.decodeToken(responseObject.token);
          this.localUserData = responseObject.localUserData;
          this.changeMemberPhoto(this.localUserData.photoUrl);
        }
      })
    );
  }

  register(user: User) {
    return this.http.post(this.baseUrl + 'register', user);
  }

  roleMatch(allowRoles): boolean {
    let isMatch = false;
    const userRoles = this.decodeToken.role as Array<string>;

    allowRoles.forEach(element => {
      if (userRoles.includes(element)) { // had some include it return true
        isMatch = true;
        return; // out for each
      }
    });
    return isMatch;
  }

}
