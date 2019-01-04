export interface IUser {
    id: string;
    userName: string;
    displayName: string;
    email: string;
}

export interface IAccessDefinition {
    target: string;
    contraint: string;
    restrict: boolean;
    accessOperation: string;
}

export interface IProfile {
    user: IUser;
    permissions: IAccessDefinition[];
    isAdmin: boolean;
}
