import * as React from 'react';
import { RouteComponentProps } from 'react-router';

export interface IImagesPanelProps {
}

export default class ImagesPanel extends React.Component<RouteComponentProps<IImagesPanelProps>, any> {
    render() {
        return (
            <div>
                hello, images panel
            </div>
        );
    }
}
