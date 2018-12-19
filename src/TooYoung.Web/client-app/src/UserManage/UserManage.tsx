import { observer } from 'mobx-react';
import { CommandBar } from 'office-ui-fabric-react/lib/CommandBar';
import { DetailsList, DetailsListLayoutMode, SelectionMode } from 'office-ui-fabric-react/lib/DetailsList';
import React, { Component } from 'react';

import { UserManageStore } from './UserManage.store';

// tslint:disable-next-line:no-empty-interface
interface IUserManageProps {
}

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
            <div className='files cmd-bar-page'>
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
            </div>
        );
    }
}
