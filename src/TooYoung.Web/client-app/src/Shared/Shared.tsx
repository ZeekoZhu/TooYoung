import { inject, observer } from 'mobx-react';
import { DefaultButton, PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import { CommandBar } from 'office-ui-fabric-react/lib/CommandBar';
import { DetailsList, DetailsListLayoutMode, SelectionMode } from 'office-ui-fabric-react/lib/DetailsList';
import Dialog, { DialogFooter } from 'office-ui-fabric-react/lib/Dialog';
import { Panel, PanelType } from 'office-ui-fabric-react/lib/Panel';
import React, { Component } from 'react';

import { selectAppStore, WithAppStore } from '../Context';
import { SharingPanel } from '../SharingPanel/SharingPanel';
import { SharedStore } from './Shared.store';

// tslint:disable-next-line:no-empty-interface
interface ISharedProps {
}

type SharedProps = ISharedProps & WithAppStore;

@inject(selectAppStore)
@observer
export class Shared extends Component<SharedProps> {
    store!: SharedStore;
    constructor(props: SharedProps) {
        super(props);
        this.store = new SharedStore(props.appStore!.sharing);
    }
    public render() {
        return (
            <div className='shared cmd-bar-page'>
                <div className='cmd-bar'>
                    <CommandBar
                        items={this.store.commandBarItems}
                        farItems={this.store.farItems}
                    />
                </div>
                <div className='title'>
                    <h2 className='ms-font-xxl ms-fontWeight-semilight'>我的共享</h2>
                </div>
                <div className='file-list' data-is-scrollable='true'>
                    <DetailsList
                        items={this.store.fileListItems.value}
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
                <Panel
                    type={PanelType.medium}
                    onDismissed={() => this.store.showSharingPanel.set(false)}
                    isOpen={this.store.showSharingPanel.value}>
                    {this.store.selectedItem === null
                        ? <h1>请选择一个文件</h1>
                        : <SharingPanel file={this.store.selectedItem.file} />}
                </Panel>
                <Dialog
                    onDismiss={this.closeCancelShareDialog}
                    hidden={!this.store.showCancelShare.value}
                    dialogContentProps={{
                        title: '取消分享',
                        subText: '是否取消分享？'
                    }}
                >
                    <DialogFooter>
                        <PrimaryButton
                            text='确定'
                            onClick={() => {
                                const selected = this.store.selectedItem;
                                if (selected) {
                                    this.store.sharingStore.deleteEntry(selected.file.id);
                                }
                                this.closeCancelShareDialog();
                            }}
                        />
                        <DefaultButton
                            text='取消'
                            onClick={this.closeCancelShareDialog}
                        />
                    </DialogFooter>
                </Dialog>
            </div>
        );
    }
    private closeCancelShareDialog = (): void => {
        this.store.showCancelShare.set(false);
    }
}
