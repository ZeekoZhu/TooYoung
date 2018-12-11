import React, { Component } from 'react';
import { observer, inject } from 'mobx-react';
import { selectAppStore, WithAppStore } from '../Context';
import { AuthStore } from '../stores/Auth.store';

interface IHeaderProps {
}

type HeaderProps = IHeaderProps & WithAppStore;

@inject(selectAppStore)
@observer
export class Header extends Component<HeaderProps> {
    private auth!: AuthStore;
    constructor(props: HeaderProps) {
        super(props);
        this.auth = props.appStore!.auth;
    }
    public render() {
        return (
            <div className="header">
                <div className="left-align">
                    <span className="brand">YourDrive</span>
                </div>
                <div className="center-align"></div>
                <div className="right-align">
                    <span>你好，</span><span>{this.auth.userName}</span>
                </div>
            </div>
        );
    }
}
