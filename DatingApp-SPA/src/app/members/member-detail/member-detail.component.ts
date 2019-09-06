import { Component, OnInit, ViewChild } from '@angular/core';
import { User } from 'src/app/_models/user';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryAnimation } from 'ngx-gallery';
import { TabsetComponent } from 'ngx-bootstrap';
import { AuthService } from 'src/app/_services/auth.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  user: User;

  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];

  @ViewChild('memberTabs', {static: true}) memberTabs: TabsetComponent;

  constructor(private authService: AuthService, private userService: UserService,
     private alertify: AlertifyService, private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.user = data['user'];
    });

    this.route.queryParams.subscribe(params => {
      const selectedTab = params['tab'];
      this.memberTabs.tabs[selectedTab > 0 ? selectedTab : 0].active = true;
    });

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4, // show 4 image under main image
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false // you can;t click image to show preview mode
      }
    ];

    this.galleryImages = this.getImage();

  } // end ngOnInit

  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }

  getImage() {
    const imageUrls = [];
    for (let i = 0; i < this.user.photos.length; i++) {
      imageUrls.push(
        {
          small: this.user.photos[i].url,
          medium: this.user.photos[i].url,
          big: this.user.photos[i].url,
          description: this.user.photos[i].description
        }
      );
    }
    return imageUrls;
  }

  sendLike(id: number) {
    this.userService.sendLike(this.authService.decodeToken.nameid, id).subscribe(() => {
      this.alertify.success('You have liked: ' + this.user.knownAs);
    }, e => {
      this.alertify.error(e);
    });
  }
}
