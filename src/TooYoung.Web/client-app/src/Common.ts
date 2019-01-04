import _ from 'lodash';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';
import styled from 'styled-components';

import { EventHandler, ICommandBarItems} from './CommonTypes';
import { ISharingEntry } from './models/sharing';

export const dateFormat = 'PPpp';

export const countSharing = (entry: ISharingEntry | null) => {
    if (entry) {
        return entry.refererRules.length + entry.tokenRules.length;
    }
    return 0;
};

export const nullableHandler = <T>(handler: EventHandler<T> | null | undefined) => (event: T) => {
    if (handler) {
        handler(event);
    }
};

export const convertCmdItems = (obj: ICommandBarItems) => {
    return _.sortBy(_.valuesIn(obj).filter(x => x !== null) as ICommandBarItemProps[], ['key']);
};

export const Input = styled.div`
margin: 0 0 20px 0;`;
