import { observer } from 'mobx-react';
import React, { Component } from 'react';
import { SharingRule } from '../SharingRule/SharingRule';
import { ISharingEntry } from '../CommonTypes';


interface ISharingPanelProps {
    entry: ISharingEntry;
}

type SharingPanelProps = ISharingPanelProps;


@observer
export class SharingPanel extends Component<SharingPanelProps> {
    public render() {
        return (
            <div className="sharing-panel">
                <h1 className="ms-font-xxl ms-fontWeight-regular">“{this.props.entry.fileName}” 的分享链接</h1>
                <SharingRule />
            </div>
        );
    }
}
