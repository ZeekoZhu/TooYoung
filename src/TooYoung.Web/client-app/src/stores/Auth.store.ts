import { computed, observable } from 'mobx';

import UserAPI from '../api/user.api';
import { WrappedProp } from '../CommonTypes';
import { IProfile } from '../models/user';

export class AuthStore {
    @observable public currentProfile = new WrappedProp<IProfile | null>(null);
    @observable public errorMsg = new WrappedProp<string>('');
    @computed get isUserSignedIn() {
        return this.currentProfile.value !== null;
    }
    @computed get isAdmin() {
        return this.currentProfile.value !== null
            && this.currentProfile.value.isAdmin;
    }
    @computed get userName() {
        return this.currentProfile.value !== null ? this.currentProfile.value.user.displayName : '尚未登录';
    }

    public checkSession() {
        UserAPI.getProfile().subscribe(
            resp => {
                if (resp !== false) {
                    this.currentProfile.set(resp);
                    this.errorMsg.set('');
                } else {
                    this.currentProfile.set(null);
                    this.errorMsg.set('登录失效，请重新登录');
                }
            }
        );
    }

    public signIn(userName: string, password: string) {
        UserAPI.signin(userName, password).subscribe(
            resp => {
                if (resp === false) {
                    this.errorMsg.set('登录失败，请检查用户名或密码是否正确');
                } else {
                    this.checkSession();
                }
            }
        );
    }

    public signOut() {
        UserAPI.signOut().subscribe(resp => {
            if (resp) {
                this.checkSession();
            }
        });
    }
}
