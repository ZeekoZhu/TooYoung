import { inject, observer } from 'mobx-react';
import { Link as FabricLink } from 'office-ui-fabric-react/lib/Link';
import React, { Component } from 'react';

import { nullableHandler } from '../Common';
import { EventHandler } from '../CommonTypes';
import { selectAppStore, WithAppStore } from '../Context';
import { ISharingEntry } from '../models/sharing';
import { SharingStore } from '../stores/Sharing.store';

interface ISharedStatusProps {
    fileId: string;
    onClick?: EventHandler<ISharingEntry | null>;
}

type SharedStatusProps = WithAppStore & ISharedStatusProps;

@inject(selectAppStore)
@observer
export class SharedStatus extends Component<SharedStatusProps> {
    sharingStore!: SharingStore;
    constructor(props: SharedStatusProps) {
        super(props);
        this.sharingStore = props.appStore!.sharing;
    }
    public render() {
        const entry = this.sharingStore.entriesForFile(this.props.fileId).get();
        const links = entry && entry.refererRules.length + entry.tokenRules.length || 0;
        if (links === 0) {
            return (
                <FabricLink>开始共享</FabricLink>
            );
        } else {
            return (
                <FabricLink
                    onClick={() => nullableHandler(this.props.onClick)(entry || null)}>
                    管理 {links} 个共享
             </FabricLink>
            );
        }
    }
}
