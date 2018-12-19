import { action, observable } from 'mobx';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';

export interface ICommandBarItems {
    [key: string]: ICommandBarItemProps | null;
}

export type DocumentIcon = 'FabricFolder' | 'Page';

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

export type AsyncData<T> = 'pending' | T;

export const isPending = <T>(data: AsyncData<T>): data is 'pending' => {
    return data === 'pending';
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
