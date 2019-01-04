import { action, observable } from 'mobx';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';

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
    dateModifiedValue: number;
    fileSize: string;
    fileSizeRaw: number;
    sharedLinks: number;
}

export interface ITokenRule {
    token: string;
    expiredAt: number | null;
    password: string;
    id: string;
    resourceId: string;
}

export interface IRefererRule {
    id: string;
    allowedHost: string;
    resourceId: string;
}

export type ISharingRule = IRefererRule | ITokenRule;

export interface ISharingEntry {
    id: string;
    fileName: string;
    tokenRules: ITokenRule[];
    refererRules: IRefererRule[];
}

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
