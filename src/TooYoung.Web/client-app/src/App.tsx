import React, { Component } from 'react';
import logo from './logo.svg';
import './App.css';
import { Provider, observer } from 'mobx-react';
import { AuthService } from './Services/AuthService';
import { AppRouter } from './AppRouter';
import DevTools from 'mobx-react-devtools';
import { observable } from 'mobx';

export interface IAppModel {
    isSignedIn: boolean;
    services?: {
        authSvc: AuthService
    }
}

const defaultModel: IAppModel = {
    isSignedIn: false
}


class App extends Component {
    @observable model = defaultModel;

    constructor(props: IAppModel) {
        super(props);
        this.model.services = {
            authSvc: new AuthService(this.model)
        }
    }
    render() {
        return (
            <Provider appModel={this.model}>
                <>
                    <AppRouter />
                    <DevTools />
                </>
            </Provider>
        );
    }
}

export default App;
