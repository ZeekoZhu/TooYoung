export interface IRefererRule {
    allowedHost: string;
    id: string;
}

export interface ITokenRule {
    token: string;
    password: string;
    id: string;
    expiredAt: string;
}

export interface ISharingEntry {
    id: string;
    resourceId: string;
    ownerId: string;
    tokenRules: ITokenRule[];
    refererRules: IRefererRule[];
}
