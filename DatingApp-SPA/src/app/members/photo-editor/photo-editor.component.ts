import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {

  @Input() photos: Photo[];
  baseUrl = environment.apiUrl;
  uploader: FileUploader;
  hasBaseDropZoneOver = false;

  currentMainPhoto: Photo;

  constructor(private authService: AuthService, private userService: UserService, private alertify: AlertifyService) { }

  ngOnInit() {
    this.initializeUploader();
  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  initializeUploader() {
    this.uploader = new FileUploader ({
      url: this.baseUrl + 'users/' + this.authService.decodeToken.nameid + '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024 // 10mb
    });

    this.uploader.onAfterAddingFile = (file) => {file.withCredentials = false; };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const resPhoto: Photo = JSON.parse(response); // we need to convert reponse(string) to Photo(object), use JSON
        const photo = {
          id: resPhoto.id,
          url: resPhoto.url,
          dateAdded: resPhoto.dateAdded,
          description: resPhoto.description,
          isMain: resPhoto.isMain,
          isApproved: resPhoto.isApproved
        };
        this.photos.push(photo);

        if (photo.isMain) {
          this.authService.changeMemberPhoto(photo.url);
          this.authService.localUserData.photoUrl = photo.url;
          localStorage.setItem('localUserData', JSON.stringify(this.authService.localUserData));
        }
      }
    };
  } // end initializeUploader()

  setMainPhoto(photo: Photo) {
    this.userService.setMainPhoto(this.authService.decodeToken.nameid, photo.id).subscribe( () => {

      if (this.photos.filter(p => p.isMain)[0]) {
      this.currentMainPhoto = this.photos.filter(p => p.isMain)[0]; // return copy of photo array, we need one so we use [0]
      this.currentMainPhoto.isMain = false;
      }


      photo.isMain = true;
      this.authService.changeMemberPhoto(photo.url); // change all subscribe that photo

      // set localStorage
      this.authService.localUserData.photoUrl = photo.url;
      localStorage.setItem('localUserData', JSON.stringify(this.authService.localUserData));
    }, e => {
      this.alertify.error(e);
    });
  }

  deletePhoto(id: number) {
    this.alertify.confirm('Are you sure you want to delete this photo?', () => {
      this.userService.deletePhoto(this.authService.decodeToken.nameid, id).subscribe(() => {
        this.photos.splice(this.photos.findIndex(p => p.id === id), 1); // ลบจาก this.photos.findIndex(p => p.id === id) ไป 1 รูป
        this.alertify.success('Photo has been deleted');
      }, e => {
        this.alertify.error('Failed to delete photo');
      });
    });
  }
}
