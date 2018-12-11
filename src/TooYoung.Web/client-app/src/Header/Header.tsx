import React, { Component } from 'react';
import { observer, inject } from 'mobx-react';
import { selectAppStore, WithAppStore } from '../Context';
import { AuthStore } from '../stores/Auth.store';
import './Header.less';

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
            <div className="header ms-bgColor-black ms-fontColor-neutralLighterAlt">
                <div className="left-align">
                    <span className="brand ms-font-xxl ms-fontWeight-regular">YourDrive</span>
                </div>
                <div className="center-align"></div>
                <div className="right-align">
                    <div className="user-info">
                        <span>你好，</span><span>{this.auth.userName}</span></div>
                </div>
            </div>
        );
    }
}
