import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';


@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private authService: AuthService, private rounter: Router, private alertify: AlertifyService) {}

  canActivate(next: ActivatedRouteSnapshot): boolean {
    const roles = next.firstChild.data['roles'] as Array<string>;

    if (roles) {
      const match = this.authService.roleMatch(roles);
      if (match) {
        return true;
      } else {
        this.alertify.error('Yor are not authorized to access this area');
        this.rounter.navigate(['/members']);
      }
    }

    if (this.authService.loggedIn()) {
      return true;
    }
    this.alertify.error('Need to login to use system');
    this.rounter.navigate(['/home']);
    return false;
  }
}
