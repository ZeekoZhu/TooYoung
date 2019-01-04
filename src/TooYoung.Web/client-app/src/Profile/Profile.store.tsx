import { computed } from 'mobx';

import UserAPI from '../api/user.api';
import { WrappedProp } from '../CommonTypes';
import { IUpdateProfileModel } from '../models/user';
import { AuthStore } from '../stores/Auth.store';

export class ProfileStore {
    constructor(public authStore: AuthStore) { }
    @computed
    get profile() {
        return this.authStore.currentProfile.value;
    }

    userName = new WrappedProp<string>('');
    displayName = new WrappedProp<string>('');
    email = new WrappedProp<string>('');
    password = new WrappedProp<string>('');
    comfirmPwd = new WrappedProp('');
    loading = new WrappedProp(false);

    @computed
    get pwdErrorMsg() {
        return this.comfirmPwd.value === this.password.value ? '' : '两次输入不一致';
    }
    setForm() {
        if (this.profile !== null) {
            const user = this.profile.user;
            this.userName.set(user.userName);
            this.displayName.set(user.displayName);
            this.email.set(user.email);
        }
    }

    submitForm() {
        const model: IUpdateProfileModel = {
            userName: this.userName.value,
            displayName: this.displayName.value,
            email: this.email.value
        };
        const updateProfile = () => {
            this.loading.set(true);
            UserAPI.updateProfile(model, this.profile!.user.id)
                .subscribe(resp => {
                    if (resp !== false) {
                        this.authStore.checkSession();
                        setTimeout(() => {
                            this.loading.set(false);
                        }, 500);
                    }
                });
        };
        if (this.password.value !== '' || this.comfirmPwd.value !== '') {
            if (this.pwdErrorMsg === '') {
                model.password = this.password.value;
                updateProfile();
            }
        } else {
            updateProfile();
        }
    }
}
