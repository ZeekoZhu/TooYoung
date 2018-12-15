import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';

export interface ICommandBarItems {
    [key: string]: ICommandBarItemProps | null;
}

export type DocumentIcon = 'FabricFolder' | 'Page';
