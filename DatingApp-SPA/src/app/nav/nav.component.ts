import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

 model: any = {};
 photoUrl: string;

  constructor(public authService: AuthService, private alertify: AlertifyService, private router: Router) { }

  ngOnInit() {
    this.authService.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl);
  }

  loggedIn() {
   return this.authService.loggedIn();
  }

  logout() {
    this.authService.logout();
    this.alertify.message('logged out');
    this.router.navigate(['home']);
  }

  login() {
    /**use subscribe to get return value observable from authService.login() */
    this.authService.login(this.model).subscribe(
      next => {
        this.alertify.success('Logged in successfully');
      }, error => {
        this.alertify.error(error);
      }, () => { // after complete do rounting, you can use inside next but this is learning coures we must to test much as possible
        this.router.navigate(['/members']);
      }
    );
  }

  // don't use anywhere be cause we use HasRoleDirectives Instead of this
  checkRole() {
    const canUseRoles: Array<string> = ['Admin', 'Moderator'];
    return this.authService.roleMatch(canUseRoles);
  }
}
