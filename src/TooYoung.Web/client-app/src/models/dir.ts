import { IFileInfo } from './file';

export interface IFileDirectory {
    id: string;
    ownerId: string;
    isRoot: boolean;
    name: string;
    parentId: string;
    directoryChildren: string[];
    fileChildren: IFileInfo[];
}
