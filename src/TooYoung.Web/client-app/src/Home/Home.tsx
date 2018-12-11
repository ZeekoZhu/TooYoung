import React, { Component } from 'react';
import { Header } from '../Header/Header';
import { SideNav } from '../SideNav/SideNav';
import './Home.less';
import { Switch, Route, Redirect } from 'react-router';
import { Files } from '../Files/Files';
import { Shared } from '../Shared/Shared';

export class Home extends Component {
    public render() {
        return (
            <div className="home">
                <Header></Header>
                <div className="main-content">
                    <div className="side">
                        <SideNav></SideNav>
                    </div>
                    <div className="content">
                        <Switch>
                            <Route path="/files" component={Files} />
                            <Route path="/shared" component={Shared} />
                            <Route path="/">
                                <Redirect to="/files"></Redirect>
                            </Route>
                        </Switch>
                    </div>
                </div>
            </div>
        );
    }
}
