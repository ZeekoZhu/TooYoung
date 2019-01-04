import { format as formatDate } from 'date-fns';
import filesize from 'filesize';
import _ from 'lodash';
import { action, observable } from 'mobx';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';

import { dateFormat } from './Common';
import { IFileDirectory } from './models/dir';
import { IFileInfo } from './models/file';
import { ISharingEntry } from './models/sharing';

export interface ICommandBarItems {
    [key: string]: ICommandBarItemProps | null;
}

export type DocumentIcon = 'FabricFolder' | 'Page';

export interface IDocument {
    name: string;
    id: string;
    value: string;
    iconName: DocumentIcon;
    fileType: string;
    dateModified: string;
    dateModifiedValue: Date;
    fileSize: string;
    fileSizeRaw: number;
    origin: IFileInfo | IFileDirectory;
}

export interface ISharingDocument {
    name: string;
    value: string;
    fileType: string;
    iconName: DocumentIcon;
    dateModified: string;
    dateModifiedValue: Date;
    fileSize: string;
    fileSizeRaw: number;
    sharingEntry: null | ISharingEntry;
    file: IFileInfo;
}

export const entriesToDocs = (entries: ISharingEntry[], files: IFileInfo[]): ISharingDocument[] => {
    const sortedEntries = _.sortBy(entries, ['resourceId']);
    const sortedFiles = _.sortBy(files, ['id']);
    const zipped = _.zip(sortedEntries, sortedFiles);
    return zipped.map(([entry, file]): ISharingDocument => {
        return {
            name: file!.name,
            dateModifiedValue: new Date(file!.dateModified),
            dateModified: formatDate(file!.dateModified, dateFormat),
            fileSize: filesize(file!.fileSize),
            fileSizeRaw: file!.fileSize,
            iconName: 'Page',
            fileType: file!.metadatas.mime,
            sharingEntry: entry!,
            value: file!.id,
            file: file!
        };
    });
};

export const fileInfoToDoc = (file: IFileInfo): IDocument => {
    const date = new Date(file.dateModified);
    const fileType = file.metadatas.mime || 'file';
    return {
        dateModifiedValue: date,
        dateModified: formatDate(date, dateFormat),
        fileSize: filesize(file.fileSize),
        fileSizeRaw: file.fileSize,
        fileType,
        iconName: 'Page',
        id: file.id,
        name: file.name,
        value: file.id,
        origin: file
    };
};

export const dirInfoToDoc = (dir: IFileDirectory): IDocument => {
    return {
        dateModified: '-',
        dateModifiedValue: new Date(),
        fileSize: '-',
        fileSizeRaw: 0,
        fileType: 'folder',
        iconName: 'FabricFolder',
        id: dir.id,
        name: dir.name,
        value: dir.id,
        origin: dir
    };
};

export const Pending = Symbol('pending');

export type AsyncData<T> = 'pending' | T | symbol;

export const isPending = <T>(data: AsyncData<T>): data is 'pending' => {
    return data === 'pending' || data === Pending;
};

export const isValue = <T>(data: AsyncData<T>): data is T => {
    return data !== Pending && data !== 'pending';
};

export type EventHandler<T> = (event: T) => void;

export class WrappedProp<T> {
    @observable public value!: T;
    @action.bound
    public set(val: T) {
        this.value = val;
    }
    constructor(val: T) {
        this.set(val);
    }
}

export type FormValidator<T> = (value: T) => string;

export const validators = <T>(...fns: Array<FormValidator<T>>) => (value: T) => {
    let result = '';
    for (const fn of fns) {
        result = fn(value);
        if (result !== '') {
            return result;
        }
    }
    return result;
};

export const Validators = {
    notEmpty: (value: string) => !value || value === '' ? '此项不能为空' : ''
};
