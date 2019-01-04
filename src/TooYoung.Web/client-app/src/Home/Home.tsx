import './Home.less';

import { inject, observer } from 'mobx-react';
import React, { Component } from 'react';
import { Redirect, Route, Switch } from 'react-router';

import { selectAppStore } from '../Context';
import { Files } from '../Files/Files';
import { Header } from '../Header/Header';
import { Profile } from '../Profile/Profile';
import { Shared } from '../Shared/Shared';
import { SideNav } from '../SideNav/SideNav';
import { AppStore } from '../stores/App.store';
import { UserManage } from '../UserManage/UserManage';

@inject(selectAppStore)
@observer
export class Home extends Component<{ appStore?: AppStore }> {

    public componentDidMount() {
        const appStore = this.props.appStore!;
        appStore.auth.checkSession();
        appStore.sharing.loadEntries();
    }
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
                            <Route path='/profile' component={Profile} />
                            <Route path='/files/:dirId?' component={Files} />
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
