import { AxiosResponse } from 'axios';

export const apiV1 = (url: any) => `/api/v1${url}`;

export const valueOnFailed = <TV>(value: TV) => <T>(axiosResp: AxiosResponse<T>) => {
    return axiosResp.status < 300 && axiosResp.status >= 200 ? axiosResp.data : value;
};

export const falseOnFailed = <T>(axiosResp: AxiosResponse<T>) => {
    return valueOnFailed<false>(false)(axiosResp);
};
