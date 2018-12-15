import { observer } from 'mobx-react';
import { CommandBar } from 'office-ui-fabric-react/lib/CommandBar';
import React, { Component } from 'react';
import { SharedStore } from './Shared.store';
import { DetailsList, SelectionMode, DetailsListLayoutMode } from 'office-ui-fabric-react/lib/DetailsList';

interface ISharedProps {
}

type SharedProps = ISharedProps;


@observer
export class Shared extends Component<SharedProps> {
    private store = new SharedStore();
    public render() {
        return (
            <div className="shared cmd-bar-page">
                <div className="cmd-bar">
                    <CommandBar
                        items={this.store.commandBarItems}
                        farItems={this.store.farItems}
                    />
                </div>
                <div className="title">
                    <h2 className="ms-font-xxl ms-fontWeight-semilight">我的共享</h2>
                </div>
                <div className="file-list" data-is-scrollable="true">
                    <DetailsList
                        items={this.store.fileListItems}
                        columns={this.store.columns}
                        selectionMode={SelectionMode.single}
                        setKey="set"
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
