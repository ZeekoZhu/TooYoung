import './Files.less';

import { observer } from 'mobx-react';
import { Breadcrumb } from 'office-ui-fabric-react/lib/Breadcrumb';
import { DefaultButton, PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import { CommandBar } from 'office-ui-fabric-react/lib/CommandBar';
import { DetailsList, DetailsListLayoutMode, SelectionMode } from 'office-ui-fabric-react/lib/DetailsList';
import Dialog, { DialogFooter } from 'office-ui-fabric-react/lib/Dialog';
import { Panel, PanelType } from 'office-ui-fabric-react/lib/Panel';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

import { Input } from '../Common';
import { SharingPanel } from '../SharingPanel/SharingPanel';
import { FilesStore } from './Files.store';

// tslint:disable-next-line:no-empty-interface
interface IFilesProps {
}

type FilesProps = IFilesProps & RouteComponentProps<{ dirId?: string }>;

@observer
class FilesComp extends Component<FilesProps> {
    private store = new FilesStore();

    loadDirInfo() {
        const match = this.props.match;
        const dirId = match.params.dirId;
        if (dirId && dirId !== '') {
            this.store.loadCurrentDir(dirId);
        } else {
            this.store.getRootDir().subscribe(resp => {
                if (resp !== false) {
                    this.props.history.push('/files/' + resp);
                }
            });
        }
    }

    componentDidMount() {
        this.loadDirInfo();
    }
    componentDidUpdate(prevProps: FilesProps) {
        if (prevProps.match.params.dirId !== this.props.match.params.dirId) {
            this.loadDirInfo();
        }
    }

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
                    <Dialog
                        onDismissed={() => {
                            return 0;
                        }}
                        hidden={!this.store.showCreateDir.value}
                        dialogContentProps={{
                            title: '创建文件夹'
                        }}
                    >
                        <Input>
                            <TextField
                                required={true}
                                value={this.store.newDirName.value}
                                onChange={(_, value) => this.store.newDirName.set(value || '')}
                                errorMessage={this.store.newDirNameValidators}
                                placeholder='名称'></TextField>
                        </Input>
                        <DialogFooter>
                            <PrimaryButton
                                text='确定'
                                onClick={() => {
                                    this.store.createDir();
                                    this.closeNewDirDialog();
                                }}
                            />
                            <DefaultButton
                                text='取消'
                                onClick={this.closeNewDirDialog}
                            />
                        </DialogFooter>
                    </Dialog>
                    <Dialog
                        onDismiss={this.clonseDeleteFileDialog}
                        hidden={!this.store.showDeleteFile.value}
                        dialogContentProps={{
                            title: '删除',
                            subText: '确定删除此项？'
                        }}
                    >
                        <DialogFooter>
                            <PrimaryButton
                                text='确定'
                                onClick={() => {
                                    const selected = this.store.seletedItem;
                                    if (selected) {
                                        this.store.deleteItem(selected);
                                        this.clonseDeleteFileDialog();
                                    }
                                }}
                            />
                            <DefaultButton
                                text='取消'
                                onClick={this.clonseDeleteFileDialog}
                            />
                        </DialogFooter>
                    </Dialog>
                </div>
            </div>
        );
    }
    private clonseDeleteFileDialog = (): void => {
        this.store.showDeleteFile.set(false);
        this.store.clearSelection();
    }
    private closeNewDirDialog = (): void => {
        this.store.newDirName.set('');
        this.store.showCreateDir.set(false);
    }
}

export const Files = withRouter(FilesComp);
