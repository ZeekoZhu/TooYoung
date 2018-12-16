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

export type EventHandler<T> = (event: T) => void;
