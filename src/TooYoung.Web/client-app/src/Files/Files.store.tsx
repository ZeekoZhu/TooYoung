import { format as formatDate } from 'date-fns';
import filesize from 'filesize';
import _ from 'lodash';
import { action, computed, observable, runInAction } from 'mobx';
import { IBreadcrumbItem } from 'office-ui-fabric-react/lib/Breadcrumb';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';
import { IColumn, Selection } from 'office-ui-fabric-react/lib/DetailsList';
import { Icon } from 'office-ui-fabric-react/lib/Icon';
import React from 'react';

import { dateFormat } from '../Common';
import { DocumentIcon, ICommandBarItems, ISharingEntry } from '../CommonTypes';
import { SharedStatus } from '../SharedStatus/SharedStatus';

export interface IDocument {
    name: string;
    id: string;
    value: string;
    iconName: DocumentIcon;
    fileType: string;
    dateModified: string;
    dateModifiedValue: number;
    fileSize: string;
    fileSizeRaw: number;
    sharedLinks: number;
}


export class FilesStore {
    @observable private cmdBarButtons: ICommandBarItems = {
        upload: {
            name: '上传',
            key: '1-upload',
            iconProps: {
                iconName: 'Upload'
            },
        }
    };
    @observable private cmdDownloadBtn: ICommandBarItemProps = {
        name: '下载',
        key: '2-download',
        iconProps: {
            iconName: 'Download'
        },
    };
    @observable private cmdDeleteBtn: ICommandBarItemProps = {
        name: '删除',
        key: '3-delete',
        iconProps: {
            iconName: 'Delete'
        },
    };
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

    @observable public pathNavItems: IBreadcrumbItem[] = [
        {
            text: '/',
            key: 'uuid-for-/'
        },
        {
            text: 'foo',
            key: 'uuid-for-foo'
        }
    ];

    @observable public seletedItem: IDocument | null = null;

    // todo: multiple select
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

    @observable public fileListItems: IDocument[] = [
        {
            id: 'bar111111',
            name: 'bar111111',
            dateModified: formatDate(new Date(), dateFormat),
            dateModifiedValue: +new Date(),
            fileSize: '-',
            fileSizeRaw: 0,
            iconName: 'FabricFolder',
            fileType: 'folder',
            value: 'uuid-for-bar',
            sharedLinks: 0
        },
        {
            name: 'bar222222',
            id: 'bar222222',
            dateModified: formatDate(new Date(), dateFormat),
            dateModifiedValue: +new Date(),
            fileSize: '-',
            fileSizeRaw: 0,
            iconName: 'FabricFolder',
            fileType: 'folder',
            value: 'uuid-for-bar2',
            sharedLinks: 0
        },
        {
            name: 'bar333333',
            id: 'bar333333',
            dateModified: formatDate(new Date(), dateFormat),
            dateModifiedValue: +new Date(),
            fileSize: '-',
            fileSizeRaw: 0,
            iconName: 'FabricFolder',
            fileType: 'folder',
            value: 'uuid-for-bar3',
            sharedLinks: 0
        },
        {
            id: 'lorem file.txt',
            name: 'lorem file.txt',
            dateModified: formatDate(new Date(), dateFormat),
            dateModifiedValue: +new Date(),
            fileSize: filesize(123432435234),
            fileSizeRaw: 123432435234,
            iconName: 'Page',
            fileType: 'txt',
            value: 'uuid-for-txt',
            sharedLinks: 0
        },
        {
            name: 'test.jpg',
            id: 'test.jpg',
            dateModified: formatDate(new Date(), dateFormat),
            dateModifiedValue: +new Date(),
            fileSize: filesize(2222),
            fileSizeRaw: 2222,
            iconName: 'Page',
            fileType: 'jpg',
            value: 'uuid-for-tes',
            sharedLinks: 1
        },
        {
            id: 'data.zip',
            name: 'data.zip',
            dateModified: formatDate(new Date(), dateFormat),
            dateModifiedValue: +new Date(),
            fileSize: filesize(231231),
            fileSizeRaw: 231231,
            iconName: 'Page',
            fileType: 'zip',
            value: 'uuid-for-zip',
            sharedLinks: 2
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
                    <SharedStatus
                        onClick={() => {
                            this.setSharingEntry(item.id);
                        }}
                        links={item.sharedLinks} />
                );
            },
            isPadded: true
        },
    ];

    @action.bound
    public setSelectedItem(item: IDocument | null) {
        console.log(item);
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

    private getSharingEntry = async (fileId: string): Promise<ISharingEntry> => {
        return Promise.resolve({
            fileName: fileId,
            id: '12312312',
            refererRules: [
                {
                    id: 'ewerqwre',
                    allowedHost: 'www.cnblogs.com',
                    resourceId: 'asasfasf'
                }
            ],
            tokenRules: [
                {
                    expiredAt: null,
                    id: '12312312',
                    password: 'password',
                    token: 'asdfasdfasdfasdferwr23redfa',
                    resourceId: 'sadfasfas'
                }
            ]
        });
    }

    @action.bound
    public setSharingEntry = async (fileId: string) => {
        const entry = await this.getSharingEntry(fileId);
        runInAction(() => {
            this.sharingEntry = entry;
        })
    }


}


