import axios from 'axios';
import { apiV1 } from './base.api';

const UserAPI = {
    signin: (username: string, password: string) => {
        axios.post(apiV1('/account/login'), {
            userName: 'zeeko',
            password: 'test'
        });
    }
};

export default UserAPI;
