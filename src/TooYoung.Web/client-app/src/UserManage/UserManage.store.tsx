import fileSize from 'filesize';
import { action, computed, observable } from 'mobx';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';
import { IColumn, Selection } from 'office-ui-fabric-react/lib/DetailsList';
import { Link as FabricLink } from 'office-ui-fabric-react/lib/Link';
import React from 'react';

import { convertCmdItems } from '../Common';
import { ICommandBarItems, WrappedProp } from '../CommonTypes';

export interface IUserInfo {
    id: string;
    name: string;
    displayName: string;
    email: string;
    password: string;
    sizeUsedValue: number;
    sizeUsed: string;
    locked: boolean;
}

export class UserManageStore {
    public selection = new Selection({
        onSelectionChanged: () => {
            if (this.selection.getSelectedCount() !== 0) {
                const item = this.selection.getSelection()[0] as IUserInfo;
                this.setSelected(item);
            } else {
                this.setSelected(null);
            }
        }
    });
    @observable private cmdBarButtons: ICommandBarItems = {
        create: {
            name: '新用户',
            key: '1-new-user',
            iconProps: {
                iconName: 'AddFriend'
            },
            onClick: () => {
                this.showAddUser.set(true);
            }
        }
    };
    @observable private cmdBlockUserBtn: ICommandBarItemProps = {
        name: '停用',
        key: '2-block',
        iconProps: {
            iconName: 'BlockContact'
        },
        onClick: () => {
            this.showBlockUser.set(true);
        }

    };
    @observable private cmdUnBlockUserBtn: ICommandBarItemProps = {
        name: '启用',
        key: '2-unblock',
        iconProps: {
            iconName: 'Unlock'
        },
        onClick: () => {
            // this.showBlockUser.set(true);
        }
    };

    @observable private cmdDeleteBtn: ICommandBarItemProps = {
        name: '删除',
        key: '3-delete',
        iconProps: {
            iconName: 'Delete'
        },
        onClick: () => {
            this.showDeleteUser.set(true);
        }
    };

    @computed public get commandBarItems() {
        return convertCmdItems(this.cmdBarButtons);
    }

    @observable public userListItems: IUserInfo[] = [
        {
            locked: false,
            id: '111111',
            name: 'Fooo',
            displayName: 'Foo Bar',
            email: 'test@example.com',
            password: 'asfasfdasdfaew23feaqa',
            sizeUsedValue: 213222,
            sizeUsed: fileSize(213222)
        },
        {
            locked: true,
            id: '22222',
            name: 'Barrrr',
            displayName: 'Lorem',
            email: 'test@example.com',
            password: 'asfasfdasdfaew23feaqa',
            sizeUsedValue: 98777,
            sizeUsed: fileSize(98777)
        }
    ];
    public columns: IColumn[] = [
        {
            key: 'column1',
            name: '用户名',
            fieldName: 'name',
            minWidth: 150,
            maxWidth: 250,
            isRowHeader: true,
            isResizable: true,
            isSorted: true,
            data: 'string',
            isPadded: true
        },
        {
            key: 'column5',
            name: '状态',
            fieldName: 'locked',
            minWidth: 50,
            maxWidth: 100,
            isRowHeader: true,
            data: 'boolean',
            isPadded: true,
            onRender: (item: IUserInfo) => {
                if (item.locked) {
                    return '已停用';
                } else {
                    return '正常';
                }
            }
        },
        {
            key: 'column2',
            name: '邮箱地址',
            fieldName: 'email',
            minWidth: 250,
            maxWidth: 350,
            isRowHeader: true,
            isResizable: true,
            isSorted: true,
            data: 'string',
            isPadded: true
        },
        {
            key: 'column3',
            name: '已使用空间大小',
            fieldName: 'sizeUsed',
            minWidth: 100,
            maxWidth: 150,
            isRowHeader: true,
            isResizable: true,
            isSorted: true,
            data: 'string',
            isPadded: true
        },
        {
            key: 'column4',
            name: '管理',
            fieldName: 'name',
            minWidth: 200,
            maxWidth: 350,
            isResizable: true,
            data: 'string',
            onRender: (item: IUserInfo) => {
                return (
                    <span>
                        <FabricLink
                            onClick={() => this.showResetPwd.set(true)}
                        >重置密码</FabricLink>
                    </span>
                );
            },
            isPadded: true
        },
    ];
    public selectedItem: WrappedProp<IUserInfo | null> = new WrappedProp(null);
    public showAddUser = new WrappedProp(false);
    public showBlockUser = new WrappedProp(false);
    public showDeleteUser = new WrappedProp(false);
    public showResetPwd = new WrappedProp(false);

    @action.bound
    public setSelected(item: IUserInfo | null) {
        this.selectedItem.set(item);
        if (this.selectedItem.value === null) {
            this.cmdBarButtons['2-block'] = null;
            this.cmdBarButtons['2-unblock'] = null;
            this.cmdBarButtons['3-delete'] = null;
        } else {
            if (this.selectedItem.value.locked) {
                this.cmdBarButtons['2-unblock'] = this.cmdUnBlockUserBtn;
                this.cmdBarButtons['2-block'] = null;
            } else {
                this.cmdBarButtons['2-block'] = this.cmdBlockUserBtn;
                this.cmdBarButtons['2-unblock'] = null;
            }
            this.cmdBarButtons['3-delete'] = this.cmdDeleteBtn;
        }
    }

}
