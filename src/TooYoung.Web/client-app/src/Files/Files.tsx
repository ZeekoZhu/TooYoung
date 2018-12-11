import './Files.less';

import { observer } from 'mobx-react';
import { Breadcrumb } from 'office-ui-fabric-react/lib/Breadcrumb';
import { CommandBar } from 'office-ui-fabric-react/lib/CommandBar';
import { DetailsList, DetailsListLayoutMode, SelectionMode } from 'office-ui-fabric-react/lib/DetailsList';
import React, { Component } from 'react';

import { FilesStore } from './Files.store';

interface IFilesProps {
}

type FilesProps = IFilesProps;


@observer
export class Files extends Component<FilesProps> {
    private store = new FilesStore();
    public render() {
        return (
            <div className="files">
                <div className="cmd-bar">
                    <CommandBar
                        items={this.store.commandBarItems}
                        farItems={this.store.farItems}
                    />
                </div>
                <div className="path-nav">
                    <Breadcrumb
                        items={this.store.pathNavItems}
                    />
                </div>
                <div className="file-list">
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
