import './index.less';
import 'babel-polyfill';

import { initializeIcons } from '@uifabric/icons';
import { configure } from 'mobx';
import * as React from 'react';
import * as ReactDOM from 'react-dom';

import App from './App';
import * as serviceWorker from './serviceWorker';

configure({
    enforceActions: 'observed'
});

initializeIcons();

ReactDOM.render(<App />, document.getElementById('root'));

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: http://bit.ly/CRA-PWA
serviceWorker.unregister();
