import { AuthStore } from './Auth.store';
import { SharingStore } from './Sharing.store';

export class AppStore {
    public auth: AuthStore = new AuthStore();
    public sharing: SharingStore = new SharingStore();
}
