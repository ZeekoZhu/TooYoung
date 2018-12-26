import axios from 'axios';

import { IProfile } from '../models/user';
import { apiV1, falseOnFailed } from './base.api';

const UserAPI = {
    signOut: async () => {
        return await axios.post(apiV1('/account/logout'))
            .then(x => x.status === 200);
    },
    signin: async (username: string, password: string) => {
        const x = await axios.post(apiV1('/account/login'), {
            userName: username,
            password
        });
        return x.status === 200;
    },
    getProfile: async () => {
        const result = await axios.get<IProfile>(apiV1('/account/profile'))
            .then(falseOnFailed);
        return result;
    }
};

export default UserAPI;
