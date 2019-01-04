import axios from 'axios';
import { defer } from 'rxjs';
import { map } from 'rxjs/operators';

import { IFileDirectory } from '../models/dir';
import { IFileInfo } from '../models/file';
import { apiV1, falseOnFailed, readData, valueOnFailed } from './base.api';

export const FilesAPI = {
    getRootDir: () => {
        return defer(() => axios.get<IFileDirectory>(apiV1('/dir/root')))
            .pipe(
                readData,
                falseOnFailed
            );
    },
    createDir: (parentId: string, name: string) => {
        return defer(() => axios.post<IFileDirectory>(apiV1('/dir/'), {
            name, parentId
        }))
            .pipe(
                readData,
                falseOnFailed
            );
    },
    deleteDir: (dirId: string) => {
        return defer(() => axios.delete(apiV1(`/dir/${dirId}`)))
            .pipe(
                map(() => true),
                falseOnFailed
            );
    },
    getDirById: (dirId: string) => {
        return defer(() => axios.get<IFileDirectory>(apiV1(`/dir/${dirId}`)))
            .pipe(
                readData,
                falseOnFailed
            );
    },
    getPath: (dirId: string) => {
        return defer(() => axios.get<IFileDirectory[]>(apiV1(`/dir/${dirId}/path`)))
            .pipe(
                readData,
                valueOnFailed<IFileDirectory[]>([])
            );
    },
    queryDirs: (dirIds: string[]) => {
        return defer(() => axios.post<IFileDirectory[]>(apiV1('/dir/query'), dirIds))
            .pipe(
                readData,
                valueOnFailed<IFileDirectory[]>([])
            );
    },
    addFile: (fileName: string, dirId: string) => {
        return defer(() => axios.post<IFileInfo>(apiV1('/files/'), {
            fileName, dirId
        }))
            .pipe(
                readData,
                falseOnFailed
            );
    },
    uploadFile: (fileId: string, file: File) => {
        return axios.request({
            url: apiV1(`/files/${fileId}/content`),
            method: 'POST',
            data: file,
            onUploadProgress: (event) => {
                console.log(event);
            }
        });
    },
    deleteFile: (fileId: string) => {
        return defer(() => axios.delete(apiV1(`/files/${fileId}`)))
            .pipe(
                map(_ => true),
                falseOnFailed
            );
    },
    testAccess: (fileId: string, fileName: string) => {
        return defer(() => axios.get<IFileInfo>(apiV1(`/files/ping/${fileId}/${fileName}`)))
            .pipe(
                readData,
                falseOnFailed
            );
    },
    queryFiles: (fileIds: string[]) => {
        return defer(() => axios.post<IFileInfo[]>(apiV1('/files/query'), fileIds))
            .pipe(
                readData,
                valueOnFailed<IFileInfo[]>([])
            );
    }
};
