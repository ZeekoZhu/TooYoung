import { computed, observable } from 'mobx';

import UserAPI from '../api/user.api';
import { AsyncData, isPending, Pending, WrappedProp } from '../CommonTypes';
import { IProfile } from '../models/user';

export class AuthStore {
    @observable public userName: string = 'Foo Bar';
    @observable public isAdmin: AsyncData<boolean> = 'pending';
    @observable public currentProfile = new WrappedProp<AsyncData<IProfile | null>>('pending');
    @observable public errorMsg = new WrappedProp<AsyncData<string | null>>(Pending);
    @computed get isUserSignedIn() {
        return isPending(this.currentProfile.value) === false && this.currentProfile.value !== null;
    }

    public async checkSession(): Promise<boolean> {
        const profile = await UserAPI.getProfile();
        if (profile === false) {
            this.currentProfile.set(null);
            this.errorMsg.set('登录失效，请重新登录');
            return false;
        } else {
            this.currentProfile.set(profile);
            this.errorMsg.set(null);
            return true;
        }
    }

    public async signIn(userName: string, password: string) {
        const result = await UserAPI.signin(userName, password);
        if (result === false) {
            this.errorMsg.set('登录失败，请检查用户名或密码是否正确');
        } else {
            this.checkSession();
        }
        this.isAdmin = true;
    }

    public async signOut() {
        const result = await UserAPI.signOut();
        if (result) {
            this.checkSession();
        }
        this.isAdmin = false;
    }
}
