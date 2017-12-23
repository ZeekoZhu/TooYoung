import * as React from 'react';
import { RouteComponentProps } from 'react-router';

export interface IProfileProps {
}

export default class Profile extends React.Component<RouteComponentProps<IProfileProps>, any> {
    render() {
        return (
            <div>
                Hello, Profile Component!
            </div>
        );
    }
}
