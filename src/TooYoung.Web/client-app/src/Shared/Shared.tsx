import React, { Component } from 'react';
import { observer, inject } from 'mobx-react';

interface ISharedProps {
}

type SharedProps = ISharedProps;


@observer
export class Shared extends Component<SharedProps> {
    public render() {
        return (
            <p>Shared component</p>
        );
    }
}
