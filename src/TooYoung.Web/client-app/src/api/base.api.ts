import { AxiosResponse } from 'axios';
import { toast } from 'react-toastify';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';

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

export const errorMsg = <T>(source: Observable<AxiosResponse<T>>) => source.pipe(
    catchError((resp) => {
        toast.error(resp && resp.data && resp.data.error || '操作失败');
        throw resp;
    })
);

export const successMsg = (msg: string) => <T>(source: Observable<AxiosResponse<T>>) => source.pipe(
    tap(resp => {
        if (resp.status >= 200 && resp.status < 300) {
            toast.success(msg);
        }
    })
);
