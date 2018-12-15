import _ from 'lodash';
import { computed, observable } from 'mobx';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';
import { Selection } from 'office-ui-fabric-react/lib/DetailsList';
import { ICommandBarItems } from '../CommonTypes';


interface ISharingEntry {
}


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

}
