namespace TooYoung.Domain.Repositories

open System
open TooYoung.Domain.Authorization
open UserGroup

type IUserGroupRepository = 
    inherit IRepository
    abstract GetGroupsByUserId: userId: Guid -> Async<UserGroup list>
    abstract AddGroupAsync: group: UserGroup -> Async<Result<UserGroup, AppError>>
    abstract DeleteGroupAsync: groupId: Guid -> Async<Result<unit, string>>
    abstract GetPermissionForUser: userId:Guid -> accessType: AccessTargetType -> Async<Result<AccessDefinition list, string>>
    abstract UpdateGroupAsync: group: UserGroup -> Async<Result<UserGroup, AppError>>
    abstract GetGroupByName: name: string -> Async<UserGroup option>