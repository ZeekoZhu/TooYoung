import { ISharingEntry, EventHandler, ITokenRule, IRefererRule, ISharingRule } from './CommonTypes';
export const dateFormat = 'PPpp';

export const countSharing = (entry: ISharingEntry | null) => {
    if (entry) {
        return entry.refererRules.length + entry.tokenRules.length;
    }
    return 0;
}

export const nullableHandler = <T>(handler: EventHandler<T> | null | undefined) => (event: T) => {
    if (handler) {
        handler(event);
    }
}

export const isTokenRule = (rule: ISharingRule): rule is ITokenRule => {
    if (rule.hasOwnProperty('token')) {
        return true;
    }
    return false;
} 
