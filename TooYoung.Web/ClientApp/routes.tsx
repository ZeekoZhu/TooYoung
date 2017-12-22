import * as React from 'react';
import { Route } from 'react-router-dom';
import Home from './components/Home';

export const routes = <div>
    <Route exact path='/' component={Home} />
</div>;
