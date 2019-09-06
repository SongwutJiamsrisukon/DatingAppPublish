import { Component, OnInit } from '@angular/core';
import { AuthService } from './_services/auth.service';
import {JwtHelperService} from '@auth0/angular-jwt';
import { LocalUserData } from './_models/localUserData';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  jwtHelper = new JwtHelperService();

  constructor(private authService: AuthService) {}

  ngOnInit() {
    const token = localStorage.getItem('token');
    const localUserData: LocalUserData = JSON.parse(localStorage.getItem('localUserData'));
    // token is expired or localUserData == null
    if (this.jwtHelper.isTokenExpired(token) || !localUserData) {
      this.authService.logout();
    }
    this.authService.decodeToken = this.jwtHelper.decodeToken(token);
    this.authService.localUserData = localUserData;

    if (localUserData) {
      this.authService.changeMemberPhoto(localUserData.photoUrl);
    }

  }

}
