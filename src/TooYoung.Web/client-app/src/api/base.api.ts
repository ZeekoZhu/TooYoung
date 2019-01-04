import { AxiosResponse } from 'axios';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export const apiV1 = (url: any) => `/api/v1${url}`;

export const valueOnFailed = <TV>(value: TV) => <T>(source: Observable<T>) => {
    return source.pipe(
        catchError(() => {
            return of(value);
        })
    );
};

export const falseOnFailed = valueOnFailed(false) as <T>(source: Observable<T>) => Observable<false | T>;

export const readData = <T>(source: Observable<AxiosResponse<T>>) => source.pipe(map(resp => resp.data));
