import _ from 'lodash';
import { action, computed, observable, runInAction } from 'mobx';
import { IBreadcrumbItem } from 'office-ui-fabric-react/lib/Breadcrumb';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';
import { IColumn, Selection } from 'office-ui-fabric-react/lib/DetailsList';
import { Icon } from 'office-ui-fabric-react/lib/Icon';
import React from 'react';
import { Link } from 'react-router-dom';
import { fromEvent, of } from 'rxjs';
import { map, switchMap, take } from 'rxjs/operators';

import { FilesAPI } from '../api/files.api';
import { dirInfoToDoc, fileInfoToDoc, ICommandBarItems, IDocument, Validators, WrappedProp } from '../CommonTypes';
import { IFileDirectory } from '../models/dir';
import { ISharingEntry } from '../models/sharing';
import { SharedStatus } from '../SharedStatus/SharedStatus';

export class FilesStore {
    @observable public inputFileRef = React.createRef<HTMLInputElement>();
    @observable private cmdBarButtons: ICommandBarItems = {
        upload: {
            name: '上传',
            key: '1-upload',
            iconProps: {
                iconName: 'Upload'
            },
            onClick: () => {
                this.inputFileRef.current!.click();
                const sub = fromEvent(this.inputFileRef.current!, 'change')
                    .pipe(
                        take(1)
                    )
                    .subscribe(() => {
                        const files = this.inputFileRef.current!.files;
                        console.log(files);
                        if (files && files.length > 0) {
                            const file = files.item(0)!;
                            const fileName = file.name;
                            FilesAPI.addFile(fileName, this.currentDir!.id)
                                .pipe(
                                    switchMap(info => {
                                        if (info !== false) {
                                            return FilesAPI.uploadFile(info.id, file);
                                        } else {
                                            return of();
                                        }
                                    })
                                )
                                .subscribe(() => {
                                    this.loadCurrentDir(this.currentDir!.id);
                                });
                        }
                        sub.unsubscribe();
                    });
            }
        },
        createDir: {
            name: '创建文件夹',
            key: '1-createDir',
            iconProps: {
                iconName: 'FabricNewFolder'
            },
            onClick: () => {
                this.showCreateDir.set(true);
            }
        }
    };
    @observable private cmdDownloadBtn: ICommandBarItemProps = {
        name: '下载',
        key: '2-download',
        iconProps: {
            iconName: 'Download'
        },
        onClick: () => {
            const selected = this.seletedItem;
            console.log(selected);
            if (selected && selected.iconName === 'Page') {
                window.open(`/api/v1/files/${selected.id}/${selected.name}`, '_blank');
            }
        }
    };
    @observable private cmdDeleteBtn: ICommandBarItemProps = {
        name: '删除',
        key: '3-delete',
        iconProps: {
            iconName: 'Delete'
        },
        onClick: () => {
            this.showDeleteFile.set(true);
        }
    };
    dirInfo = new WrappedProp<IFileDirectory[]>([]);
    @computed get currentDir() {
        return _.last(this.dirInfo.value);
    }
    dirChildren = new WrappedProp<IFileDirectory[]>([]);
    @computed get fileListItems() {
        const current = _.last(this.dirInfo.value);
        if (current) {
            const files = current.fileChildren.map(x => fileInfoToDoc(x));
            const dirs = (this.dirChildren.value).map(x => dirInfoToDoc(x));
            const parent = this.dirInfo.value[this.dirInfo.value.length - 2];
            if (parent) {
                const p = _.cloneDeep(parent);
                p.name = '..';
                dirs.unshift(dirInfoToDoc(p));
            }
            return [...dirs, ...files];
        }
        return [];
    }
    @computed public get commandBarItems(): ICommandBarItemProps[] {
        return _.sortBy(_.valuesIn(this.cmdBarButtons).filter(x => x !== null) as ICommandBarItemProps[], ['name']);
    }

    @observable public farItems: ICommandBarItemProps[] = [
        {
            name: '详情',
            key: 'info',
            iconProps: {
                iconName: 'Info'
            },
            iconOnly: true
        }
    ];

    @computed get pathNavItems(): IBreadcrumbItem[] {
        return this.dirInfo.value.map(x => {
            return {
                text: x.isRoot ? '/' : x.name,
                key: x.id
            } as IBreadcrumbItem;
        });
    }

    @observable public seletedItem: IDocument | null = null;

    // todo: multiple select
    public selection = new Selection({
        onSelectionChanged: () => {
            if (this.selection.count !== 0) {
                const item = this.selection.getSelection()[0] as IDocument;
                this.setSelectedItem(item);
            } else {
                this.setSelectedItem(null);
            }
        }
    });

    public columns: IColumn[] = [
        {
            key: 'column1',
            name: '类型',
            iconName: 'Page',
            isIconOnly: true,
            fieldName: 'name',
            minWidth: 32,
            maxWidth: 32,
            onRender: (item: IDocument) => {
                return (
                    <div className='icon'>
                        <Icon iconName={item.iconName} />
                    </div>
                );
            }
        },
        {
            key: 'column2',
            name: '名称',
            fieldName: 'name',
            minWidth: 210,
            maxWidth: 350,
            isRowHeader: true,
            isResizable: true,
            isSortedDescending: false,
            sortAscendingAriaLabel: 'Sorted A to Z',
            sortDescendingAriaLabel: 'Sorted Z to A',
            data: 'string',
            onRender: (item: IDocument) => {
                if (item.iconName === 'FabricFolder') {
                    return (
                        <Link to={`/files/${item.id}`}>{item.name}</Link>
                    );
                } else {
                    return (<>{item.name}</>);
                }
            },
            isPadded: true
        },
        {
            key: 'column3',
            name: '修改日期',
            fieldName: 'dateModifiedValue',
            minWidth: 100,
            maxWidth: 150,
            isResizable: true,
            data: 'number',
            onRender: (item: IDocument) => {
                return (
                    <span>
                        {item.dateModified}
                    </span>
                );
            },
            isPadded: true
        },
        {
            key: 'column4',
            name: '大小',
            fieldName: 'fileSizeRaw',
            minWidth: 70,
            maxWidth: 90,
            isResizable: true,
            isCollapsable: true,
            data: 'number',
            onRender: (item: IDocument) => {
                return <span>{item.fileSize}</span>;
            }
        },
        {
            key: 'column5',
            name: '分享',
            fieldName: 'sharedLinks',
            minWidth: 70,
            maxWidth: 90,
            isResizable: true,
            isCollapsable: true,
            data: 'string',
            onRender: (item: IDocument) => {
                if (item.iconName === 'FabricFolder') {
                    return '-';
                } else {
                    return (
                        <SharedStatus
                            onClick={(entry) => {
                                this.setSharingEntry(entry);
                            }}
                            fileId={item.id} />
                    );
                }
            },
            isPadded: true
        },
    ];

    @action.bound
    public setSelectedItem(item: IDocument | null) {
        this.seletedItem = item;
        if (this.seletedItem === null) {
            this.cmdBarButtons['2-download'] = null;
            this.cmdBarButtons['3-delete'] = null;
        } else {
            this.cmdBarButtons['2-download'] = this.cmdDownloadBtn;
            this.cmdBarButtons['3-delete'] = this.cmdDeleteBtn;
        }
    }

    @observable public sharingEntry: null | ISharingEntry = null;
    @computed public get showSharingPanel() {
        return this.sharingEntry !== null;
    }
    // 需要添加弹框
    public showDeleteFile = new WrappedProp(false);
    public showCreateDir = new WrappedProp(false);
    public newDirName = new WrappedProp('');
    @computed
    public get newDirNameValidators() { return Validators.notEmpty(this.newDirName.value); }
    @action.bound
    public setSharingEntry = (entry: ISharingEntry | null) => {
        if (entry !== null) {
            runInAction(() => {
                this.sharingEntry = entry;
            });
        }
    }

    public clearSelection() {
        this.selection.toggleAllSelected();
    }

    public getRootDir() {
        return FilesAPI.getRootDir().pipe(
            map(x => x ? x.id : x)
        );
    }

    public deleteItem(item: IDocument) {
        if (item.iconName === 'FabricFolder') {
            FilesAPI.deleteDir(item.id).subscribe(
                resp => {
                    if (resp !== false) {
                        this.loadCurrentDir(this.currentDir!.id);
                    }
                }
            );
        } else {
            FilesAPI.deleteFile(item.id).subscribe(
                resp => {
                    if (resp !== false) {
                        this.loadCurrentDir(this.currentDir!.id);
                    }
                }
            );
        }
    }

    public loadCurrentDir(dirId: string) {
        FilesAPI.getPath(dirId).subscribe(resp => {
            const current = _.last(resp);
            if (current && current.directoryChildren.length > 0) {
                FilesAPI.queryDirs(current.directoryChildren).subscribe(queryResult => {
                    this.dirChildren.set(queryResult);
                    this.dirInfo.set(resp);
                });
            } else {
                this.dirInfo.set(resp);
                this.dirChildren.set([]);
            }
        });
    }

    public createDir() {
        const dirName = this.newDirName.value;
        const currentDir = this.currentDir!.id;
        if (this.newDirNameValidators === '') {
            FilesAPI.createDir(currentDir, dirName).subscribe(resp => {
                if (resp !== false) {
                    this.loadCurrentDir(currentDir);
                }
            });
        }
    }
}
