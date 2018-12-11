import { inject, observer } from 'mobx-react';
import { ActionButton, DefaultButton, PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import React from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import { selectAppStore, WithAppStore } from '../Context';
import { AuthStore } from '../stores/Auth.store';
import './Login.less'

// tslint:disable-next-line:no-empty-interface
interface ILoginProps {
}
type LoginProps = RouteComponentProps & ILoginProps & WithAppStore;
@inject(selectAppStore)
@observer
class LoginComp extends React.Component<LoginProps> {
    private authSvc!: AuthStore;
    constructor(props: LoginProps) {
        super(props);
        this.authSvc = props.appStore!.auth;
    }
    public signIn = () => {
        this.authSvc.signIn();
    }
    public signOut = () => {
        this.authSvc.signOut();
    }
    public welcome = (fragment: JSX.Element) => {
        return (
            <div className="full-screen login">
                <div className="background">
                    <div className="center">
                        {fragment}
                    </div>
                </div>
            </div>
        );
    }
    public welcomeLogin = () => {
        return this.welcome(
            <div className="login-form">
                <div className="fields">
                    <TextField
                        className="text-field"
                        placeholder="用户名" />
                    <TextField
                        className="text-field"
                        placeholder="密码" />
                </div>
                <div className="buttons">
                    <PrimaryButton
                        onClick={this.signIn}
                        className="btn" text="登录" />
                    <DefaultButton
                        className="btn"
                        text="注册" />
                </div>
            </div>
        );
    }
    public welcomeSignOut = () => {
        return this.welcome(
            <div className="sign-out">
                <span className="ms-font-xl ms-fontWeight-regular">{this.authSvc.userName}, </span>
                <ActionButton
                    iconProps={{ iconName: 'SignOut' }}
                    onClick={this.signOut}>登出</ActionButton>
            </div>
        );
    }
    public render() {
        switch (this.authSvc.isUserSignedIn) {
            case 'loading':
                return (<p>Loading...</p>);
            case true:
                return this.welcomeSignOut();
            case false:
                return this.welcomeLogin();
        }
    }
}

export const Login = withRouter(LoginComp);
