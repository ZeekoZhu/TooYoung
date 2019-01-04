import axios from 'axios';
import { defer } from 'rxjs';
import { map } from 'rxjs/operators';

import { IProfile } from '../models/user';
import { apiV1, falseOnFailed, readData } from './base.api';

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
    }
};

export default UserAPI;
