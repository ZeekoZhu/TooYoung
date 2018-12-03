import { IAppModel } from '../App';
import { runInAction, action } from 'mobx';

export class AuthService {
    constructor(private appModel: IAppModel) {
    }

    async isSessionAlive(): Promise<boolean> {
        return !!localStorage.getItem('signin');
    }

    /**
     * Is User signed in
     *
     * @action
     * @returns {Promise<boolean>}
     * @memberof AuthService
     */
    @action('[Auth] Is Signed In')
    async isSignedIn(): Promise<boolean> {
        const result = this.appModel.isSignedIn || await this.isSessionAlive();
        if (result) {
            runInAction(() => {
                this.appModel.isSignedIn = result;
            });
        }
        return result;
    }
    @action('[Auth] Sign In')
    signIn() {
        localStorage.setItem('signin', 'true');
        // fake async
        console.log('set it');
        this.appModel.isSignedIn = true;
    }

    @action('[Auth] Sign Out')
    signOut() {
        localStorage.removeItem('signin');
        this.appModel.isSignedIn = false;
    }
}
