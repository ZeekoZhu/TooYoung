import './UserManage.less';

import { observer } from 'mobx-react';
import { DefaultButton, PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import { CommandBar } from 'office-ui-fabric-react/lib/CommandBar';
import { DetailsList, DetailsListLayoutMode, SelectionMode } from 'office-ui-fabric-react/lib/DetailsList';
import Dialog, { DialogFooter, DialogType } from 'office-ui-fabric-react/lib/Dialog';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import React, { Component } from 'react';
import styled from 'styled-components';

import { UserManageStore } from './UserManage.store';

// tslint:disable-next-line:no-empty-interface
interface IUserManageProps {
}

const Input = styled.div`
margin: 0 0 20px 0;`;

type UserManageProps = IUserManageProps;

@observer
export class UserManage extends Component<UserManageProps> {
    private store!: UserManageStore;
    constructor(props: UserManageProps) {
        super(props);
        this.store = new UserManageStore();
    }
    public render() {
        return (
            <div className='admin cmd-bar-page'>
                <div className='cmd-bar'>
                    <CommandBar
                        items={this.store.commandBarItems}
                    />
                </div>
                <div className='title'>
                    <h2 className='ms-font-xxl ms-fontWeight-semilight'>用户管理</h2>
                </div>
                <div className='file-list' data-is-scrollable='true'>
                    <DetailsList
                        items={this.store.userListItems}
                        columns={this.store.columns}
                        selectionMode={SelectionMode.single}
                        setKey='set'
                        layoutMode={DetailsListLayoutMode.justified}
                        isHeaderVisible={true}
                        selection={this.store.selection}
                        selectionPreservedOnEmptyClick={true}
                        // onItemInvoked={this._onItemInvoked}
                        enterModalSelectionOnTouch={true}
                    />
                </div>
                <Dialog
                    minWidth={400}
                    dialogContentProps={{
                        type: DialogType.largeHeader,
                        title: '添加新用户'
                    }}
                    hidden={!this.store.showAddUser.value}
                    onDismiss={this.closeAddUserDialog}
                >
                    <div className='add-user'>
                        <Input>
                            <TextField placeholder='用户名' />
                        </Input>
                        <Input>
                            <TextField placeholder='显示名称' />
                        </Input>
                        <Input>
                            <TextField placeholder='邮箱地址' />
                        </Input>
                        <Input>
                            <TextField type='password' placeholder='密码' />
                        </Input>
                    </div>
                    <DialogFooter>
                        <PrimaryButton
                            text='添加'
                            onClick={this.closeAddUserDialog}
                        />
                        <DefaultButton
                            text='取消'
                            onClick={this.closeAddUserDialog}
                        />
                    </DialogFooter>
                </Dialog>
                <Dialog
                    onDismiss={this.closeBlockUserDialog}
                    hidden={!this.store.showBlockUser.value}
                    dialogContentProps={{
                        title: '停用账户',
                        subText: '禁止该用户登录，并禁用该用户分享的所有文件'
                    }}
                >
                    <DialogFooter>
                        <PrimaryButton
                            text='禁用'
                            onClick={this.closeBlockUserDialog}
                        />
                        <DefaultButton
                            text='取消'
                            onClick={this.closeBlockUserDialog}
                        />
                    </DialogFooter>
                </Dialog>
                <Dialog
                    onDismissed={this.closeResetPwdDialog}
                    hidden={!this.store.showResetPwd.value}
                    dialogContentProps={{
                        title: '重置密码',
                        type: DialogType.largeHeader
                    }}
                >
                    <Input>
                        <TextField placeholder='新密码'></TextField>
                    </Input>
                    <DialogFooter>
                        <PrimaryButton
                            text='重置'
                            onClick={this.closeResetPwdDialog}
                        />
                        <DefaultButton
                            text='取消'
                            onClick={this.closeResetPwdDialog}
                        />
                    </DialogFooter>
                </Dialog>
                <Dialog
                    onDismiss={this.closeDeleteUserDialog}
                    hidden={!this.store.showDeleteUser.value}
                    dialogContentProps={{
                        title: '删除账户',
                        subText: '将此用户删除'
                    }}
                >
                    <DialogFooter>
                        <PrimaryButton
                            text='确定'
                            onClick={this.closeDeleteUserDialog}
                        />
                        <DefaultButton
                            text='取消'
                            onClick={this.closeDeleteUserDialog}
                        />
                    </DialogFooter>
                </Dialog>
            </div>
        );
    }
    private closeAddUserDialog = (): void => {
        this.store.showAddUser.set(false);
    }
    private closeBlockUserDialog = (): void => {
        this.store.showBlockUser.set(false);
    }
    private closeResetPwdDialog = (): void => {
        this.store.showResetPwd.set(false);
    }
    private closeDeleteUserDialog = (): void => {
        this.store.showDeleteUser.set(false);
    }
}
