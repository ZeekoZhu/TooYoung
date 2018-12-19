import _ from 'lodash';
import { ICommandBarItemProps } from 'office-ui-fabric-react/lib/CommandBar';

import { EventHandler, ICommandBarItems, ISharingEntry, ISharingRule, ITokenRule } from './CommonTypes';

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

export const isTokenRule = (rule: ISharingRule): rule is ITokenRule => {
    if (rule.hasOwnProperty('token')) {
        return true;
    }
    return false;
};

export const convertCmdItems = (obj: ICommandBarItems) => {
    return _.sortBy(_.valuesIn(obj).filter(x => x !== null) as ICommandBarItemProps[], ['key']);
};
