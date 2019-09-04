import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse, HTTP_INTERCEPTORS } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            catchError (error => {
                if (error instanceof HttpErrorResponse) { // if found AnyError from response


                    const applicationError = error.headers.get('Application-Error');
                    if (applicationError) {
                        console.error(applicationError); // error red text at interceptor
                        return throwError(applicationError); // errorText from calling method
                    }
                    if (error.status === 401) {
                        return throwError(error.statusText);
                    }
                    if (error.status === 403) {
                        return throwError(error.statusText);
                    }
                    const serverError = error.error; // handle BadRequest, In dotnet 2.2 use error.error.errors instead
                    let modelStateError = ''; // ต่างจาก var ตรงที่ เมื่อประกาศ let variable ไว้ 2 ตำแหน่ง คือในและนอก scope
                                                // เมื่อ ออกนอก scope แล้ว print มันจะเลือก let variable ที่อยู่นอก scope
                                                // let คือ blocked-scope
                    if (serverError.errors && typeof serverError.errors === 'object') {
                        for (const key in serverError) {
                            if (serverError.errors[key]) {
                                modelStateError += serverError.errors[key] + '\n';
                            }
                        }
                    }
                    return throwError(modelStateError || serverError || 'otherError'); // ถ้า modelStateError เป็น null จะไปใช้ serverError
                                                                                        // ถ้า serverError เป็น null จะไปใช้ 'otherError'
                }
            })
        );
    }
}

export const ErrorInterceptorProvider = { // use to add in app.module.ts
    provide: HTTP_INTERCEPTORS,
    useClass: ErrorInterceptor,
    multi: true
}