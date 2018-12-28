import './Files.less';

import { observer } from 'mobx-react';
import { Breadcrumb } from 'office-ui-fabric-react/lib/Breadcrumb';
// import { DefaultButton, PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import { CommandBar } from 'office-ui-fabric-react/lib/CommandBar';
import { DetailsList, DetailsListLayoutMode, SelectionMode } from 'office-ui-fabric-react/lib/DetailsList';
// import Dialog, { DialogFooter, DialogType } from 'office-ui-fabric-react/lib/Dialog';
import { Panel, PanelType } from 'office-ui-fabric-react/lib/Panel';
import React, { Component } from 'react';
import { SharingPanel } from '../SharingPanel/SharingPanel';
import { FilesStore } from './Files.store';

// tslint:disable-next-line:no-empty-interface
interface IFilesProps {
}

type FilesProps = IFilesProps;

@observer
export class Files extends Component<FilesProps> {
    private store = new FilesStore();
    public render() {
        return (
            <div className='files cmd-bar-page'>
                <div className='cmd-bar'>
                    <CommandBar
                        items={this.store.commandBarItems}
                        farItems={this.store.farItems}
                    />
                </div>
                <div className='title'>
                    <Breadcrumb
                        items={this.store.pathNavItems}
                    />
                </div>
                <div className='file-list' data-is-scrollable='true'>
                    <DetailsList
                        items={this.store.fileListItems}
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
                    <Panel
                        type={PanelType.medium}
                        isOpen={this.store.showSharingPanel}>
                        {this.store.sharingEntry === null
                            ? <h1>请选择一个文件</h1>
                            : <SharingPanel entry={this.store.sharingEntry} />}
                    </Panel>
                    <input className='input-file' ref={this.store.inputFileRef} type='file' />

                    {/* <Dialog
                        minWidth={400}
                        hidden={!this.store.showUploadFlie.value}
                        onDismiss={() => this.store.showUploadFlie.set(false)}
                        >
                        <div>
                        </div>
                        <DialogFooter>
                            <PrimaryButton onClick={() =>
                                this.store.showUploadFlie.set(false)} text='确定' />
                            <DefaultButton onClick={() =>
                                this.store.showUploadFlie.set(false)} text='取消' />
                        </DialogFooter>
                    </Dialog> */}
                </div>
            </div>
        );
    }
}
