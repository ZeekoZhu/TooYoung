import axios from 'axios';
import { defer } from 'rxjs';
import { map } from 'rxjs/operators';

import { IProfile, IUpdateProfileModel, IUser } from '../models/user';
import { IUserInfo } from '../UserManage/UserManage.store';
import { apiV1, errorMsg, falseOnFailed, readData, successMsg, valueOnFailed } from './base.api';

const UserAPI = {
    signOut: () => {
        return defer(() => axios.post(apiV1('/account/logout')))
            .pipe(
                map(x => x.status === 200)
            );
    },
    signin: (username: string, password: string) => {
        return defer(() => axios.post(apiV1('/account/login'), {
            userName: username,
            password
        })).pipe(
            errorMsg,
            map(resp => resp.status === 200),
            falseOnFailed
        );
    },
    getProfile: () => {
        return defer(() => axios.get<IProfile>(apiV1('/account/profile')))
            .pipe(
                readData,
                falseOnFailed
            );
    },

    updateProfile: (model: IUpdateProfileModel, userId: string) => {
        return defer(() => axios.put<IUser>(apiV1('/account/profile/' + userId), model))
            .pipe(
                successMsg('操作成功'),
                errorMsg,
                readData,
                falseOnFailed
            );
    },

    getAllUsers: () => {
        return defer(() => axios.get<IUserInfo[]>(apiV1('/account/users')))
            .pipe(
                readData,
                valueOnFailed<IUserInfo[]>([])
            );
    },

    addUser: (userName: string, password: string, email: string, displayName: string) => {
        return defer(() => axios.post<IUser>(apiV1('/account/register'), {
            password, userName, email, displayName
        }))
            .pipe(
                readData,
                falseOnFailed
            );
    },

    deleteUser: (userId: string) => {
        return defer(() => axios.request<IUser>(
            {
                method: 'DELETE',
                withCredentials: true,
                url: apiV1(`/account/${userId}`)
            }
        ))
            .pipe(
                readData,
                falseOnFailed
            );
    },

    setLockStatus: (userId: string, lock: boolean) => {
        return defer(() => axios.patch<IUser>(apiV1(`/account/${userId}/lock/${lock}`)))
            .pipe(
                readData,
                falseOnFailed
            );
    }
};

export default UserAPI;
