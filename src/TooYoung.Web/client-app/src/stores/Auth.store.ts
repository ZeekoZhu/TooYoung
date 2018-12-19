
import { action, observable, runInAction } from 'mobx';
import { AsyncData } from '../CommonTypes';

export class AuthStore {
    @observable public isUserSignedIn: AsyncData<boolean> = 'pending';
    @observable public userName: string = 'Foo Bar';
    @observable public isAdmin: AsyncData<boolean> = 'pending';

    public isSessionAlive(): Promise<boolean> {
        return Promise.resolve(!!localStorage.getItem('signin'));
    }

    /**
     * Is User signed in
     *
     * @action
     * @returns {Promise<void>}
     * @memberof AuthService
     */
    @action('[Auth] Check Session')
    public async checkSession(): Promise<void> {
        const result = this.isUserSignedIn === true || await this.isSessionAlive();
        runInAction('[Auth] Update session status', () => {
            console.log(result);
            this.isUserSignedIn = result;
        });
    }
    @action('[Auth] Sign In')
    public signIn() {
        localStorage.setItem('signin', 'true');
        // fake async
        console.log('set it');
        this.isUserSignedIn = true;
        this.isAdmin = true;
    }

    @action('[Auth] Sign Out')
    public signOut() {
        localStorage.removeItem('signin');
        this.isUserSignedIn = false;
        this.isAdmin = false;
    }
}
