import './App.less';

import { observable } from 'mobx';
import { Provider } from 'mobx-react';
import React, { Component } from 'react';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

import { AppRouter } from './AppRouter';
import { IAppContext } from './Context';
import { AppStore } from './stores/App.store';

// Context
const appContext: IAppContext = observable({
    appStore: new AppStore()
});

class App extends Component {
    public render() {
        return (
            <Provider {...appContext}>
                <>
                    <AppRouter />
                    <ToastContainer
                        position='top-right'
                        autoClose={5000}
                        hideProgressBar={false}
                        newestOnTop={false}
                        closeOnClick={true}
                        rtl={false}
                        pauseOnHover={false}
                    />
                </>
            </Provider>
        );
    }
}

export default App;
