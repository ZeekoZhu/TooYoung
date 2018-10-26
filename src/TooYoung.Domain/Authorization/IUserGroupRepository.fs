namespace TooYoung.Domain.Repositories

open System
open TooYoung.Domain.Authorization
open UserGroup

type IUserGroupRepository =
    inherit IRepository
    abstract GetGroupsByUserId: userId: Guid -> Async<UserGroup list>
    abstract AddGroupAsync: group: UserGroup -> Async<Result<UserGroup, string>>
    abstract DeleteGroupAsync: groupId: Guid -> Async<Result<unit, string>>
    abstract GetPermissionForUser: userId:Guid -> accessType: AccessTargetType list -> Async<Result<AccessDefinition list, string>>
    abstract UpdateGroupAsync: group: UserGroup -> Async<Result<UserGroup, string>>
    abstract GetGroupByName: name: string -> Async<UserGroup option>