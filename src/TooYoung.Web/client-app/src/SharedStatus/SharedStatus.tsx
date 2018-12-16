import { Link as FabricLink } from 'office-ui-fabric-react/lib/Link';
import React from 'react';
import { nullableHandler } from '../Common';
import { EventHandler } from '../CommonTypes';

export const SharedStatus = (props: { links: number, onClick?: EventHandler<void> }) => {
    const { links, onClick } = props;
    if (links === 0) {
        return (
            <FabricLink>开始共享</FabricLink>
        );
    } else {
        return (
            <FabricLink
                onClick={() => nullableHandler(onClick)()}>
                管理 {links} 个共享
            </FabricLink>
        );
    }
}
