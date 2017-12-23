import * as React from 'react';
import { Route } from 'react-router-dom';
import Home from './components/Home';
import ImagesPanel from './components/ImagesPanel';
import ProfilePanel from './components/ProfilePanel';

export const routeConfig = [
    {
        path: '/',
        name: '图片管理',
        component: ImagesPanel,
        exact: true,
        hide: true
    },
    {
        name: '图片管理',
        path: '/images',
        component: ImagesPanel
    },
    {
        name: '用户信息',
        path: '/profile',
        component: ProfilePanel
    }
];

const ParseRoute = (route: any) => {
    return <Route path={route.path} component={route.component} exact={route.exact} />;
};

export const routes = <Home>
    {routeConfig.map(ParseRoute)}
</Home>;
