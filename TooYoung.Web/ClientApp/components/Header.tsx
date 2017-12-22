import * as React from 'react';
export interface IHeaderProps { }

export default class Header extends React.Component<IHeaderProps,
    any> {
    render() {
        return <div className='header'></div>;
    }
}
