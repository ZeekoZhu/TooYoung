import { inject, observer } from 'mobx-react';
import React, { Component } from 'react';
import { BrowserRouter as Router, Redirect, Route, Switch } from 'react-router-dom';

import { selectAppStore } from './Context';
import { Home } from './Home/Home';
import { Login } from './Login/Login';
import { AppStore } from './stores/App.store';

export interface IPrivateRouteProp {
    comp: typeof Component,
    appStore?: AppStore,
    path: string
}

@inject(selectAppStore)
@observer
export class PrivateRoute extends Component<IPrivateRouteProp> {
    public render() {
        const { appStore, comp: Comp, ...rest } = this.props;
        const guard = (props: any) => {
            console.log('2333', appStore!.auth.isUserSignedIn);
            return appStore!.auth.isUserSignedIn === true
                ? (<Comp {...props} />)
                : <Redirect to={{
                    pathname: '/login',
                    state: { from: props.location }
                }} />;
        };
        return (
            <Route {...rest}
                render={guard}
            />);
    }
}

export const AppRouter = () => (
    <Router>
        <Switch>
            <Route path="/login" component={Login} />
            <PrivateRoute path="/" comp={Home} />
        </Switch>
    </Router>
);