namespace TooYoung.Domain.Services
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open System
open TooYoung
open TooYoung.Domain.Authorization
open TooYoung.Domain.Authorization.UserGroup
open TooYoung.Domain.Repositories
open TooYoung.Domain.User
open Utils


type AuthorizationService(repo: IUserGroupRepository) =

    let checkForAdd name fn =
        repo.GetGroupByName name
        |> Async.bind
            ( function
            | Some _ -> Validation "Group already exists" |> Error |> Async.fromValue
            | None -> fn ()
            )
    /// Create a new group for new user
    member this.CreateGroupForUserAsync (user: User) =
        let group = UserGroup(Guid.NewGuid())
        ( fun _ ->
            group.Name <- user.UserName.ToString()
            group.Users <- [ user.Id ]
            repo.AddGroupAsync(group)
        )
        |> checkForAdd (user.Id.ToString())

    /// Get group by name
    member this.GetGroupByName name =
        repo.GetGroupByName name
    member this.EnsureGroupByName name =
        this.GetGroupByName name
        |> AsyncResult.ofSome
            (fun _ ->
                raise (InvalidState (name |> sprintf "User group: %s should exists")))

    /// Create a new empty group
    member this.CreateNewGroup name (permissions: Permission list) =
        ( fun _ ->
            let group = UserGroup(Guid.NewGuid())
            group.Name <- name
            group.AddPermissions permissions
            repo.AddGroupAsync group
        )
        |> checkForAdd name

    member this.DeleteGroupAsync groupId =
        repo.DeleteGroupAsync groupId

    member this.AddUserToAsync (group: UserGroup) userId =
        group.Users <- userId:: group.Users |> List.distinct
        repo.UpdateGroupAsync group

    member this.UpdateGroupAsync group =
        repo.UpdateGroupAsync group

    member this.GetAccessDefinitionsForUserAsync userId accessType =
        repo.GetPermissionForUser userId accessType