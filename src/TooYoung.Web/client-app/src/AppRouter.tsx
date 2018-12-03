import { observer, inject } from 'mobx-react';
import React, { Component } from 'react';
import { IAppModel } from './App';
import {
    BrowserRouter as Router,
    Route,
    Link,
    Redirect,
    withRouter,
    RouteComponentProps
} from 'react-router-dom';
import { AuthService } from './Services/AuthService';

const selectAppModel = (appStore: any) => (appStore as IAppModel);

export class Index extends Component {
    render() {
        return (
            <h1>Index</h1>
        );
    }
}

// const Login = () => <h1>Login</h1>;
interface ILoginProps {
    appModel?: IAppModel;
}

type LoginProps = RouteComponentProps & ILoginProps;

@inject(selectAppModel)
@observer
export class Login extends Component<LoginProps> {
    private authSvc!: AuthService;
    constructor(props: LoginProps) {
        super(props);
        this.authSvc = props.appModel!.services!.authSvc;
    }
    signIn() {
        this.authSvc.signIn();
    }
    signOut() {
        this.authSvc.signOut();
    }
    render() {
        const { appModel } = this.props;
        console.log(appModel!.isSignedIn);
        return (<>
            {
                appModel!.isSignedIn === false
                    ? <button onClick={() => this.signIn()}>Sign In</button>
                    : <button onClick={() => this.signOut()}>Sign Out</button>
            }
        </>);
    }
}

export interface IPrivateRouteProp {
    comp: typeof Component,
    appModel?: IAppModel,
    path: string
}

@inject(appStore => ({
    appModel: appStore as IAppModel
}))
@observer
export class PrivateRoute extends Component<IPrivateRouteProp> {
    render() {
        const { appModel, comp: Comp, ...rest } = this.props;
        return (
            <Route {...rest}
                render={props => appModel!.isSignedIn
                    ? (<Comp {...props} />)
                    : <Redirect to={{
                        pathname: '/login',
                        state: { from: props.location }
                    }} />}
            />);
    }
}

export const AppRouter = () => (
    <Router>
        <>
            <Route path="/login" component={Login} />
            <PrivateRoute path="/" comp={Index} />
        </>
    </Router>
);
