import fileSize from 'filesize';
import { action, computed, observable, runInAction } from 'mobx';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';
import { IColumn, Selection } from 'office-ui-fabric-react/lib/DetailsList';
import { Link as FabricLink } from 'office-ui-fabric-react/lib/Link';
import React from 'react';
import { tap } from 'rxjs/operators';

import UserAPI from '../api/user.api';
import { convertCmdItems } from '../Common';
import { ICommandBarItems, Validators, WrappedProp } from '../CommonTypes';

export interface IUserInfo {
    id: string;
    userName: string;
    displayName: string;
    email: string;
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
            const selected = this.selectedItem.value;
            if (selected) {
                this.lockUser(selected.id, false);
            }
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

    @observable public userListItems: IUserInfo[] = [];
    public columns: IColumn[] = [
        {
            key: 'column1',
            name: '用户名',
            fieldName: 'userName',
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
                            onClick={() => {
                                this.selectedItem.set(item);
                                this.showResetPwd.set(true);
                            }}
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
    public resetPwdForm = {
        pwd: new WrappedProp('')
    };
    public resetPwdFormValidators = {
        pwd: computed(() => Validators.notEmpty(this.resetPwdForm.pwd.value))
    };

    public addUserForm = {
        userName: new WrappedProp(''),
        displayName: new WrappedProp(''),
        password: new WrappedProp(''),
        email: new WrappedProp(''),
    };

    public addUserFormValidators = {
        userName: computed(() => Validators.notEmpty(this.addUserForm.userName.value)),
        password: computed(() => Validators.notEmpty(this.addUserForm.password.value)),
        email: computed(() => Validators.notEmpty(this.addUserForm.email.value)),
        displayName: computed(() => Validators.notEmpty(this.addUserForm.displayName.value)),
    };

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

    loadUsers() {
        this.selection.setAllSelected(false);
        UserAPI.getAllUsers().pipe(
            tap(data => data.forEach(u => u.sizeUsed = fileSize(u.sizeUsedValue))),
        )
            .subscribe(resp => {
                runInAction('[User Manage] Load users', () => {
                    this.userListItems = resp;
                });
            });
    }

    addUser() {
        const form = this.addUserForm;
        UserAPI.addUser(form.userName.value, form.password.value, form.email.value, form.displayName.value)
            .subscribe(resp => {
                if (resp !== false) {
                    this.loadUsers();
                }
                this.showAddUser.set(false);
            });
    }

    lockUser(userId: string, lock: boolean) {
        UserAPI.setLockStatus(userId, lock).subscribe(
            resp => {
                if (resp !== false) {
                    this.loadUsers();
                }
            }
        );
    }

    deleteUser(userId: string) {
        UserAPI.deleteUser(userId).subscribe(
            resp => {
                if (resp !== false) {
                    this.loadUsers();
                }
            }
        );
    }

    resetPwd() {
        const form = this.resetPwdForm;
        const user = this.selectedItem.value;
        const newPwd = form.pwd.value;
        if (user) {
            UserAPI.updateProfile({
                userName: user.userName,
                displayName: user.displayName,
                email: user.email,
                password: newPwd
            }, user.id).subscribe(
                resp => {
                    this.selectedItem.set(null);
                }
            );
        }
    }

}
