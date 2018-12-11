import { format as formatDate } from 'date-fns';
import filesize from 'filesize';
import { observable } from 'mobx';
import { IBreadcrumbItem } from 'office-ui-fabric-react/lib/Breadcrumb';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';
import { IColumn } from 'office-ui-fabric-react/lib/DetailsList';
import { Icon } from 'office-ui-fabric-react/lib/Icon';
import { Selection } from 'office-ui-fabric-react/lib/MarqueeSelection';
import React from 'react';

type DocumentIcon = 'FabricFolder' | 'Page';
const dateFormat = 'MMM D, YYYY';
export interface IDocument {
    name: string;
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
    @observable public commandBarItems: ICommandBarItemProps[] = [
        {
            name: '上传',
            key: 'upload',
            iconProps: {
                iconName: 'Upload'
            },
        }
    ];

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

    // todo: multiple select
    public selection = new Selection();

    @observable public fileListItems: IDocument[] = [
        {
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
            minWidth: 70,
            maxWidth: 90,
            isResizable: true,
            data: 'number',
            onRender: (item: IDocument) => {
                console.log(item.dateModified);
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
            name: '分享',
            fieldName: 'sharedLinks',
            minWidth: 70,
            maxWidth: 90,
            isResizable: true,
            isCollapsable: true,
            data: 'string',
            onRender: (item: IDocument) => {
                return <span>{item.sharedLinks}</span>;
            },
            isPadded: true
        },
        {
            key: 'column5',
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
        }
    ];


}
