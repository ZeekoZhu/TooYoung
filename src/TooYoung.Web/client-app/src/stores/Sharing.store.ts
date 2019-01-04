import { computed } from 'mobx';

import { FilesAPI } from '../api/files.api';
import { SharingAPI } from '../api/sharing.api';
import { entriesToDocs, ISharingDocument, WrappedProp } from '../CommonTypes';
import { ISharingEntry } from '../models/sharing';

export class SharingStore {
    entries = new WrappedProp<ISharingEntry[]>([]);
    sharingDocs = new WrappedProp<ISharingDocument[]>([]);

    entriesForFile(fileInfoId: string) {
        return computed(() => {
            return this.entries.value.filter(x => x.resourceId === fileInfoId)[0];
        });
    }

    loadEntries() {
        SharingAPI.getAllEntries().subscribe(
            resp => {
                if (resp !== false) {
                    this.entries.set(resp);
                    const fileIds = resp.map(x => x.resourceId);
                    FilesAPI.queryFiles(fileIds).subscribe(fileInfos => {
                        if (fileInfos.length === resp.length) {
                            this.sharingDocs.set(entriesToDocs(resp, fileInfos));
                        }
                    });
                }
            }
        );
    }

    addTokenRule(password: string, expiredAt: Date, fileId: string) {
        SharingAPI.addTokenRule(password, fileId, expiredAt).subscribe(resp => {
            if (resp !== false) {
                this.loadEntries();
            }
        });
    }
    addRefererRule(host: string, fileId: string) {
        SharingAPI.addRefererRule(host, fileId).subscribe(resp => {
            if (resp !== false) {
                this.loadEntries();
            }
        });
    }
    deleteRule(type: 'token' | 'referer', ruleId: string, fileId: string) {
        SharingAPI.deleteRule(type, fileId, ruleId).subscribe(resp => {
            if (resp !== false) {
                this.loadEntries();
            }
        });
    }

    deleteEntry(fileId: string) {
        SharingAPI.deleteEntry(fileId).subscribe(resp => {
            if (resp !== false) {
                this.loadEntries();
            }
        });
    }
}
