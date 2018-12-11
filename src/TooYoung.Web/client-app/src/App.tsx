import './App.less';

import { observable } from 'mobx';
import { Provider } from 'mobx-react';
import DevTools from 'mobx-react-devtools';
import React, { Component } from 'react';

import { AppRouter } from './AppRouter';
import { IAppContext } from './Context';
import { AppStore } from './stores/App.store';




// Context


const appContext: IAppContext = observable({
    appStore: new AppStore()
});

class App extends Component {
    public componentDidMount() {
        appContext.appStore.auth.checkSession();
    }
    public render() {
        return (
            <Provider {...appContext}>
                <>
                    <AppRouter />
                </>
            </Provider>
        );
    }
}

export default App;
