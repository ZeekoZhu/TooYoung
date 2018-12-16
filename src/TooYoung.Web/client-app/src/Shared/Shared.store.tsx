import formatDate from 'date-fns/format';
import filesize from 'filesize';
import _ from 'lodash';
import React from 'react';
import { action, computed, observable } from 'mobx';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';
import { IColumn, Selection } from 'office-ui-fabric-react/lib/DetailsList';
import { Icon } from 'office-ui-fabric-react/lib/Icon';

import { dateFormat, countSharing } from '../Common';
import { ISharingEntry, DocumentIcon, ICommandBarItems } from '../CommonTypes';

import { SharedStatus } from '../SharedStatus/SharedStatus';



export interface IDocument {
    name: string;
    value: string;
    fileType: string;
    iconName: DocumentIcon;
    dateModified: string;
    dateModifiedValue: number;
    fileSize: string;
    fileSizeRaw: number;
    sharingEntry: null | ISharingEntry;
}



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

    public selection = new Selection({
        onSelectionChanged: () => {
            if (this.selection.count !== 0) {
                const item = this.selection.getItems()[0] as IDocument;
                this.setSelectedItem(item);
            } else {
                this.setSelectedItem(null);
            }
        }
    });
    @computed public get commandBarItems(): ICommandBarItemProps[] {
        return _.sortBy(_.valuesIn(this.cmdBarButtons).filter(x => x !== null) as ICommandBarItemProps[], ['name']);
    }

    @observable public selectedItem: IDocument | null = null;

    @action.bound
    public setSelectedItem(item: IDocument | null) {
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

    @observable public fileListItems: IDocument[] = [
        {
            name: 'lorem file.txt',
            dateModified: formatDate(new Date(), dateFormat),
            dateModifiedValue: +new Date(),
            fileSize: filesize(123432435234),
            fileSizeRaw: 123432435234,
            iconName: 'Page',
            fileType: 'txt',
            value: 'uuid-for-txt',
            sharingEntry: {
                fileName: 'lorem file.txt',
                id: '12312312',
                refererRules: [],
                tokenRules: [
                    {
                        expiredAt: null,
                        id: '12312312',
                        password: 'password',
                        token: 'asdfasdfasdfasdferwr23redfa',
                        resourceId: 'resource id uusid'
                    }
                ]
            }
        },
        {
            name: 'test.jpg',
            dateModified: formatDate(new Date(), dateFormat),
            dateModifiedValue: +new Date(),
            fileSize: filesize(2222),
            fileSizeRaw: 2222,
            iconName: 'Page',
            fileType: 'jpg',
            value: 'uuid-for-tes',
            sharingEntry: {
                fileName: 'test.jpg',
                id: '12312312',
                refererRules: [],
                tokenRules: [
                    {
                        expiredAt: null,
                        id: '12312312',
                        password: 'password',
                        token: 'asdfasdfasdfasdferwr23redfa',
                        resourceId: 'resource id uusid'
                    }
                ]
            }
        },
        {
            name: 'data.zip',
            dateModified: formatDate(new Date(), dateFormat),
            dateModifiedValue: +new Date(),
            fileSize: filesize(231231),
            fileSizeRaw: 231231,
            iconName: 'Page',
            fileType: 'zip',
            value: 'uuid-for-zip',
            sharingEntry: {
                fileName: 'test.jpg',
                id: '12312312',
                refererRules: [],
                tokenRules: [
                    {
                        expiredAt: null,
                        id: '12312312',
                        password: 'password',
                        token: 'asdfasdfasdfasdferwr23redfa',
                        resourceId: 'resource id uusid'
                    }
                ]
            }
        },
    ];

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
                    <div className="icon">
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
                return (
                    <SharedStatus links={countSharing(item.sharingEntry)} />
                );
            },
            isPadded: true
        },
    ];


}
