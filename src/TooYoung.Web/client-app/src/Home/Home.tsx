import './Home.less';

import React, { Component } from 'react';
import { Redirect, Route, Switch } from 'react-router';

import { Files } from '../Files/Files';
import { Header } from '../Header/Header';
import { Shared } from '../Shared/Shared';
import { SideNav } from '../SideNav/SideNav';
import { UserManage } from '../UserManage/UserManage';

export class Home extends Component {
    public render() {
        return (
            <div className='home'>
                <Header></Header>
                <div className='main-content'>
                    <div className='side ms-borderColor-neutralLight'>
                        <SideNav></SideNav>
                    </div>
                    <div className='content'>
                        <Switch>
                            <Route path='/admin' component={UserManage} />
                            <Route path='/files' component={Files} />
                            <Route path='/shared' component={Shared} />
                            <Route path='/'>
                                <Redirect to='/files'></Redirect>
                            </Route>
                        </Switch>
                    </div>
                </div>
            </div>
        );
    }
}
