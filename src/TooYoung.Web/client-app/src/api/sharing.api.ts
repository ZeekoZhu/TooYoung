import axios from 'axios';
import { defer } from 'rxjs';
import { map } from 'rxjs/operators';

import { ISharingEntry } from '../models/sharing';
import { apiV1, falseOnFailed, readData, successMsg } from './base.api';

export const SharingAPI = {
    getAllEntries: () => {
        return defer(() => axios.get<ISharingEntry[]>(apiV1('/sharing/')))
            .pipe(
                readData,
                falseOnFailed
            );
    },
    addRefererRule: (host: string, fileInfoId: string) => {
        return defer(() => axios.post<ISharingEntry>(apiV1('/sharing/referer'), {
            allowedHost: host, fileInfoId
        }))
            .pipe(
                successMsg('添加成功'),
                readData,
                falseOnFailed,
            );
    },
    addTokenRule: (password: string, fileInfoId: string, expiredAt: Date) => {
        return defer(() => axios.post<ISharingEntry>(apiV1('/sharing/token'), {
            password, expiredAt, fileInfoId
        }))
            .pipe(
                successMsg('添加成功'),
                readData,
                falseOnFailed
            );
    },
    deleteRule: (rule: 'token' | 'referer', fileInfoId: string, ruleId: string) => {
        return defer(() => axios.delete(apiV1(`/sharing/${fileInfoId}/${rule}/${ruleId}`)))
            .pipe(
                successMsg('操作成功'),
                map(() => true),
                falseOnFailed
            );
    }
};
