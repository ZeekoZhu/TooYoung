import './SharingPanel.less';

import { action, observable } from 'mobx';
import { inject, observer } from 'mobx-react';
import { ActionButton, DefaultButton, PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import { DatePicker } from 'office-ui-fabric-react/lib/DatePicker';
import { Dialog, DialogFooter, DialogType } from 'office-ui-fabric-react/lib/Dialog';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import React, { Component } from 'react';

import { EventHandler, WrappedProp } from '../CommonTypes';
import { selectAppStore, WithAppStore } from '../Context';
import { IFileInfo } from '../models/file';
import { SharingRule } from '../SharingRule/SharingRule';
import { SharingStore } from '../stores/Sharing.store';

interface ISharingPanelProps {
    file: IFileInfo;
}

type SharingPanelProps = WithAppStore & ISharingPanelProps;

@inject(selectAppStore)
@observer
export class SharingPanel extends Component<SharingPanelProps> {
    @observable public showAddTokenRule = false;
    @observable public showAddRefererRule = false;
    sharingStore!: SharingStore;
    rulePassword = new WrappedProp('');
    ruleExpiredAt = new WrappedProp(new Date());
    ruleHost = new WrappedProp('');
    constructor(props: SharingPanelProps) {
        super(props);
        this.sharingStore = props.appStore!.sharing;
    }
    @action.bound
    setAddTokenRuleVisablity(show: boolean) {
        this.showAddTokenRule = show;
    }
    @action.bound
    setAddRefererRuleVisablity(show: boolean) {
        this.showAddRefererRule = show;
    }
    public render() {
        const entry = this.sharingStore.entriesForFile(this.props.file.id).get();
        return (
            <div className='sharing-panel'>
                <h1 className='ms-font-xxl ms-fontWeight-regular'>“{this.props.file.name}” 的分享链接</h1>
                {/* 私密链接 */}
                <div className='rules'>
                    <div className='rules-header'>
                        <span className='rules-name ms-font-l ms-fontWeight-regular'>私密共享</span>
                        <ActionButton
                            iconProps={{
                                iconName: 'Add'
                            }}
                            onClick={() => this.setAddTokenRuleVisablity(true)}
                        >
                            添加链接
                        </ActionButton>
                    </div>
                    {(entry && entry.tokenRules || []).map(rule =>
                        <SharingRule key={rule.id} rule={rule} file={this.props.file} />
                    )}
                </div>
                {/* 公开链接 */}
                <div className='rules'>
                    <div className='rules-header'>
                        <span className='rules-name ms-font-l ms-fontWeight-regular'>公开链接</span>
                        <ActionButton
                            onClick={() => this.setAddRefererRuleVisablity(true)}
                            iconProps={{
                                iconName: 'Add'
                            }}>添加链接</ActionButton>
                    </div>
                    {(entry && entry.refererRules || []).map(rule =>
                        <SharingRule
                            key={rule.id}
                            file={this.props.file}
                            rule={rule} />)}
                </div>
                <AddRuleDialog
                    title='添加私密共享链接'
                    hidden={!this.showAddTokenRule}
                    onDismiss={() => this.setAddTokenRuleVisablity(false)}
                    content={
                        <>
                            <TextField
                                label='其他人需要输入正确的提取码才能下载文件'
                                placeholder='提取码'
                                value={this.rulePassword.value}
                                onChange={(_, value) => this.rulePassword.set(value || '')}
                                iconProps={{
                                    iconName: 'Lock'
                                }}
                            />
                            <DatePicker
                                value={this.ruleExpiredAt.value}
                                onSelectDate={(value) => this.ruleExpiredAt.set(value || new Date())}
                            />
                        </>
                    }
                    onSave={() => {
                        this.setAddTokenRuleVisablity(false);
                        this.sharingStore.addTokenRule(
                            this.rulePassword.value, this.ruleExpiredAt.value, this.props.file.id
                        );
                    }}
                    onCancel={() => {
                        this.setAddTokenRuleVisablity(false);
                    }}
                />
                <AddRuleDialog
                    title='添加公开共享链接'
                    hidden={!this.showAddRefererRule}
                    onDismiss={() => this.setAddRefererRuleVisablity(false)}
                    content={
                        <>
                            <TextField
                                label='限制 HTTP 请求头中的 Referer 字段，防止盗链'
                                placeholder='允许的域名'
                                value={this.ruleHost.value}
                                onChange={(_, value) => this.ruleHost.set(value || '')}
                                iconProps={{
                                    iconName: 'Website'
                                }}
                            />
                        </>
                    }
                    onSave={() => {
                        this.setAddRefererRuleVisablity(false);
                        this.sharingStore.addRefererRule(this.ruleHost.value, this.props.file.id);
                    }}
                    onCancel={() => {
                        this.setAddRefererRuleVisablity(false);
                    }}
                />
            </div>
        );
    }
    public renderDialog = (hidden: boolean) => {
        return <Dialog
            hidden={hidden}
        >
        </Dialog>;
    }
}

const AddRuleDialog =
    (props:
        {
            hidden: boolean,
            onDismiss: EventHandler<void>,
            title: string,
            onSave: EventHandler<void>,
            onCancel: EventHandler<void>,
            content: JSX.Element
        }) => {
        const { hidden, content, onDismiss, title, onSave, onCancel } = props;
        const onSaveClick = () => onSave();
        const onCancelClick = () => onCancel();
        const onDialogDismiss = () => onDismiss();
        return (
            <Dialog
                onDismiss={onDialogDismiss}
                dialogContentProps={{
                    title,
                    type: DialogType.largeHeader,
                }}
                hidden={hidden}>
                {content}
                <DialogFooter>
                    <PrimaryButton text='添加' onClick={onSaveClick} />
                    <DefaultButton onClick={onCancelClick} text='取消' />
                </DialogFooter>
            </Dialog>
        );

    };
