export interface IFileInfo {
    id: string;
    ownerId: string;
    name: string;
    fileSize: number;
    binaryId: string;
    metadatas: {
        [key: string]: string;
    };
}
