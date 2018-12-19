import fileSize from 'filesize';
import { action, computed, observable } from 'mobx';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';
import { IColumn, Selection } from 'office-ui-fabric-react/lib/DetailsList';
import { Link as FabricLink } from 'office-ui-fabric-react/lib/Link';
import React from 'react';

import { convertCmdItems } from '../Common';
import { ICommandBarItems } from '../CommonTypes';

export interface IUserInfo {
    id: string;
    name: string;
    displayName: string;
    email: string;
    password: string;
    sizeUsedValue: number;
    sizeUsed: string;
}

export class UserManageStore {
    public selection = new Selection({
        onSelectionChanged: () => {
            if (this.selection.count !== 0) {
                const item = this.selection.getItems()[0] as IUserInfo;
                this.setSelected(item);
            }
        }
    });
    @observable private cmdBarButtons: ICommandBarItems = {
        create: {
            name: '新用户',
            key: '1-new-user',
            iconProps: {
                iconName: 'AddFriend'
            }
        }
    };
    @observable private cmdBlockUserBtn: ICommandBarItemProps = {
        name: '停用',
        key: '2-block',
        iconProps: {
            iconName: 'BlockContact'
        }
    };
    @observable private cmdDeleteBtn: ICommandBarItemProps = {
        name: '删除',
        key: '3-delete',
        iconProps: {
            iconName: 'Delete'
        },
    };

    @computed public get commandBarItems() {
        return convertCmdItems(this.cmdBarButtons);
    }

    @observable public userListItems: IUserInfo[] = [
        {
            id: '111111',
            name: 'Fooo',
            displayName: 'Foo Bar',
            email: 'test@example.com',
            password: 'asfasfdasdfaew23feaqa',
            sizeUsedValue: 213222,
            sizeUsed: fileSize(213222)
        },
        {
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
                        <FabricLink>重置密码</FabricLink>
                    </span>
                );
            },
            isPadded: true
        },
    ];
    @observable public selectedItem: IUserInfo | null = null;

    @action.bound
    public setSelected(item: IUserInfo) {
        this.selectedItem = item;
        if (this.selectedItem === null) {
            this.cmdBarButtons['2-block'] = null;
            this.cmdBarButtons['3-delete'] = null;
        } else {
            this.cmdBarButtons['2-block'] = this.cmdBlockUserBtn;
            this.cmdBarButtons['3-delete'] = this.cmdDeleteBtn;
        }
    }

}
