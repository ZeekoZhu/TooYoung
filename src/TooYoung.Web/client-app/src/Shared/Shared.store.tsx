import _ from 'lodash';
import { action, computed, observable } from 'mobx';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';
import { IColumn, Selection } from 'office-ui-fabric-react/lib/DetailsList';
import { Icon } from 'office-ui-fabric-react/lib/Icon';
import React from 'react';

import { ICommandBarItems, ISharingDocument, WrappedProp } from '../CommonTypes';
import { SharedStatus } from '../SharedStatus/SharedStatus';
import { SharingStore } from '../stores/Sharing.store';

export class SharedStore {
    @observable public cmdBarButtons: ICommandBarItems = {
    };

    @observable private cmdDownloadBtn = {
        name: '下载',
        key: '1-download',
        iconProps: {
            iconName: 'Download'
        }
    };
    @observable private cmdUnlinkBtn = {
        name: '取消分享',
        key: '2-unlink',
        iconProps: {
            iconName: 'RemoveLink'
        },
        onClick: () => {
            this.showCancelShare.set(true);
        }
    };

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

    // todo: wrong api call
    public selection = new Selection({
        onSelectionChanged: () => {
            if (this.selection.count !== 0) {
                const item = this.selection.getItems()[0] as ISharingDocument;
                this.setSelectedItem(item);
            } else {
                this.setSelectedItem(null);
            }
        }
    });
    @computed public get commandBarItems(): ICommandBarItemProps[] {
        return _.sortBy(_.valuesIn(this.cmdBarButtons).filter(x => x !== null) as ICommandBarItemProps[], ['name']);
    }

    @observable public selectedItem: ISharingDocument | null = null;
    showSharingPanel = new WrappedProp(false);
    public showCancelShare = new WrappedProp(false);
    @action.bound
    public setSelectedItem(item: ISharingDocument | null) {
        console.log(item);
        this.selectedItem = item;
        if (this.selectedItem === null) {
            this.cmdBarButtons['2-download'] = null;
            this.cmdBarButtons['3-unlink'] = null;
        } else {
            this.cmdBarButtons['2-download'] = this.cmdDownloadBtn;
            this.cmdBarButtons['3-unlink'] = this.cmdUnlinkBtn;
        }
    }

    fileListItems = this.sharingStore.sharingDocs;

    public columns: IColumn[] = [
        {
            key: 'column1',
            name: '类型',
            iconName: 'Page',
            isIconOnly: true,
            fieldName: 'name',
            minWidth: 32,
            maxWidth: 32,
            onRender: (item: ISharingDocument) => {
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
            isSorted: true,
            isSortedDescending: false,
            sortAscendingAriaLabel: 'Sorted A to Z',
            sortDescendingAriaLabel: 'Sorted Z to A',
            data: 'string',
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
            onRender: (item: ISharingDocument) => {
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
            onRender: (item: ISharingDocument) => {
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
            onRender: (item: ISharingDocument) => {
                return (
                    <SharedStatus
                        onClick={() => this.showSharingPanel.set(true)}
                        fileId={item.file.id}
                    />
                );
            },
            isPadded: true
        },
    ];
    constructor(public sharingStore: SharingStore) {
    }
}
