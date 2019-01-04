import { AuthStore } from './Auth.store';

export class AppStore {
    public auth: AuthStore = new AuthStore();
}
