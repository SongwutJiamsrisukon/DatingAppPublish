import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { PaginatedResult } from '../_models/pagination';
import { map } from 'rxjs/operators';
import { isUndefined } from 'util';
import { Message } from '../_models/message';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // return type Observable<User[]>
  getUsers(userParams?: any): Observable<PaginatedResult<User[]>> {
    const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();
    let params = new HttpParams();

    params = isUndefined(userParams.pageNumber) ? params : params.append('pageNumber', userParams.pageNumber);
    params = isUndefined(userParams.pageSize) ? params : params.append('pageSize', userParams.pageSize);

    params = isUndefined(userParams.minAge) ? params : params.append('minAge', userParams.minAge);
    params = isUndefined(userParams.maxAge) ? params : params.append('maxAge', userParams.maxAge);
    params = isUndefined(userParams.gender) ? params : params.append('gender', userParams.gender);
    params = isUndefined(userParams.orderBy) ? params : params.append('orderBy', userParams.orderBy);

    params = isUndefined(userParams.typeOfLike) ? params : params.append('typeOfLike', userParams.typeOfLike);

    // if observe = response(not body anymore) we need to use rxjs .pipe
    return this.http.get<User[]>(this.baseUrl + 'users', { observe: 'response', params })
    .pipe(
      map(r => {
        paginatedResult.result = r.body;
        if (r.headers.get('Pagination') != null) {
          paginatedResult.pagination = JSON.parse(r.headers.get('Pagination')); // convert string to Json object
        }
        return paginatedResult;
      })
    );
  }

  getUser(id: number): Observable<User> {
    return this.http.get<User>(this.baseUrl + 'users/' + id);
  }

  updateUser(id: number, user: User) {
    return this.http.put(this.baseUrl + 'users/' + id, user);
  }

  setMainPhoto(userId: number, id: number) {
    return this.http.post(this.baseUrl + 'users/' + userId + '/photos/' + id + '/setMain', {});
  }

  deletePhoto(userId: number, id: number) {
    return this.http.delete(this.baseUrl + 'users/' + userId + '/photos/' + id);
  }

  sendLike(id: number, recipientId: number) {
    return this.http.post(this.baseUrl + 'users/' + id + '/like/' + recipientId, {});
  }

  removeLike(id: number, recipientId: number) {
    return this.http.post(this.baseUrl + 'users/' + id + '/remove_like/' + recipientId, {});
  }

  getMessages(id: number, messageParams?: any) {
    const paginatedResult: PaginatedResult<Message[]> = new PaginatedResult<Message[]>();
    let params = new HttpParams();

    params = isUndefined(messageParams.pageNumber) ? params : params.append('pageNumber', messageParams.pageNumber);
    params = isUndefined(messageParams.pageSize) ? params : params.append('pageSize', messageParams.pageSize);

    params = isUndefined(messageParams.messageContainer) ? params : params.append('messageContainer', messageParams.messageContainer);

    return this.http.get<Message[]>(this.baseUrl + 'users/' + id + '/messages', { observe: 'response', params })
    .pipe(
      map(r => {
        paginatedResult.result = r.body;
        if (r.headers.get('Pagination') != null) {
          paginatedResult.pagination = JSON.parse(r.headers.get('Pagination')); // converts JSON data into JavaScript Object.
        }
        return paginatedResult;
      })
    );
  }

  getMessageThread(id: number, recipientId: number) {
    return this.http.get<Message[]>(this.baseUrl + 'users/' + id + '/messages/thread/' + recipientId);
  }

  sendMessage(id: number, message: Message) {
    return this.http.post(this.baseUrl + 'users/' + id + '/messages', message);
  }

  deleteMessage(messageId: number, userId: number) {
    return this.http.post(this.baseUrl + 'users/' + userId + '/messages/' + messageId, {});
  }

  markAsRead(userId: number, messageId: number) {
    this.http.post(this.baseUrl + 'users/' + userId + '/messages/' + messageId + '/read', {}).subscribe();
  }
}
