import { observer } from 'mobx-react';
import React, { Component } from 'react';
import { SharingRule } from '../SharingRule/SharingRule';
import { ISharingEntry } from '../CommonTypes';
import './SharingPanel.less';
import { ActionButton } from 'office-ui-fabric-react/lib/Button';

interface ISharingPanelProps {
    entry: ISharingEntry;
}

type SharingPanelProps = ISharingPanelProps;


@observer
export class SharingPanel extends Component<SharingPanelProps> {
    public render() {
        const { entry } = this.props;
        return (
            <div className="sharing-panel">
                <h1 className="ms-font-xxl ms-fontWeight-regular">“{this.props.entry.fileName}” 的分享链接</h1>
                <div className="rules">
                    <div className="rules-header">
                        <span className="rules-name ms-font-l ms-fontWeight-regular">私密共享</span>
                        <ActionButton
                            iconProps={{
                                iconName: "Add"
                            }}>添加链接</ActionButton>
                    </div>
                    {entry.tokenRules.map(rule =>
                        <SharingRule key={rule.id} rule={rule} />
                    )}
                </div>
                <div className="rules">
                    <div className="rules-header">
                        <span className="rules-name ms-font-l ms-fontWeight-regular">公开链接</span>
                        <ActionButton
                            iconProps={{
                                iconName: "Add"
                            }}>添加链接</ActionButton>
                    </div>
                    {entry.refererRules.map(rule =>
                        <SharingRule
                            key={rule.id}
                            rule={rule} />)}
                </div>
            </div>
        );
    }
}
