import './Header.less';

import { inject, observer } from 'mobx-react';
import React, { Component } from 'react';
import { Link } from 'react-router-dom';

import { selectAppStore, WithAppStore } from '../Context';
import { AuthStore } from '../stores/Auth.store';

// tslint:disable-next-line:no-empty-interface
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
            <div className='header ms-bgColor-black flex-fixed ms-fontColor-neutralLighterAlt'>
                <div className='left-align'>
                    <span className='brand ms-font-xxl ms-fontWeight-regular'>YourDrive</span>
                </div>
                <div className='center-align'></div>
                <div className='right-align'>
                    <div className='user-info'>
                        <span>你好，</span>
                        <Link to='/profile' >{this.auth.userName}</Link>
                        <span> | </span>
                        <Link to='/login' >退出登录</Link>
                    </div>
                </div>
            </div>
        );
    }
}
