import { AppStore } from './stores/App.store';

export interface IAppContext {
    appStore: AppStore;
}

export const selectAppStore = (store: IAppContext) => ({
    appStore: store.appStore as AppStore
});

// tslint:disable-next-line:interface-over-type-literal
export type WithAppStore = {
    appStore?: AppStore
}

