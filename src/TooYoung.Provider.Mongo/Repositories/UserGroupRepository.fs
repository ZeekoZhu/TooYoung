namespace TooYoung.Provider.Mongo.Repositories
open System
open MongoDB.Driver
open TooYoung.Domain.Repositories
open System.Linq;
open FsToolkit.ErrorHandling

open MongoDB.Bson
open TooYoung.Domain.Authorization.UserGroup
open TooYoung.Provider.Mongo
open Utils
open BsonHelper
open TooYoung
open TooYoung.Domain
open TooYoung.Provider.Mongo.Enities



type UserGroupRepository(db: IMongoDatabase) =
    let groups = db.GetCollection<UserGroupEntity>("UserGroup")

    let getGroupsByUserId userId =
        groups.Find((fun x -> x.Users.Contains(userId)))
                .ToListAsync()
        |> Async.AwaitTask
        |> Async.map (List.ofSeq >> List.map (Mapper.UserGroup.toModel))

    let addGroupAsync group =
        groups.InsertOneAsync group
        |> Async.AwaitTask

    let deleteGroupAsync groupId =
        groups.DeleteOneAsync (fun x -> x.Id = groupId)
        |> Async.AwaitTask
        |> Async.map (fun _ -> ())

    let getPermissionForUser (userId: Guid) (targetType: string) =
        let pipeline =
            [ bson [ "$match", bson [ "Users", bson ["$elemMatch", bVal userId] ] ]
              bson [ "$unwind", bVal "$Definitions" ]
              bson [ "$match", bson [ "Target", bVal targetType ] ]
            ]
        groups.Aggregate()
            .Match(fun g -> g.Users.Any(fun u -> u = userId))
            .Unwind<UserGroupEntity, AccessDefinitionEntity>((fun x -> x.Definitions :> obj))
            .Match(fun def -> def.Target = targetType)
            .ToListAsync()
        |> Async.AwaitTask
        |> Async.map ( List.ofSeq >> List.map (Mapper.AccessDefinition.toModel))

    let updateGroupAsync (group: UserGroup) =
        groups.ReplaceOneAsync((fun x -> x.Id = group.Id), Mapper.UserGroup.toEntity group)
        |> Async.AwaitTask
        |> Async.map (fun _ -> group)

    let getGroupByName name =
        groups.Find(fun x -> x.Name = name).FirstOrDefaultAsync()
        |> Async.AwaitTask
        |> Async.map (Option.ofObj >> Option.map Mapper.UserGroup.toModel)

    interface IUserGroupRepository with
        member this.BeginTransaction () =
            new MongoUnitOfWork(db.Client) :> UnitOfWork
        member this.GetGroupsByUserId (userId: Guid) =
            getGroupsByUserId userId

        member this.AddGroupAsync group =
            try addGroupAsync (Mapper.UserGroup.toEntity group)
                |> Async.map (fun _ -> group |> Ok)
            with e ->
                e.Message |> Error |> Async.fromValue
        
        member this.DeleteGroupAsync groupId =
            try deleteGroupAsync groupId
                |> Async.map Ok
            with e ->
                e.Message |> Error |> Async.fromValue

        member this.GetPermissionForUser userId accessType =
            try getPermissionForUser userId accessType
                |> Async.map Ok
            with e -> e.Message |> Error |> Async.fromValue
        
        member this.UpdateGroupAsync group =
            try updateGroupAsync group
                |> Async.map Ok
            with e -> e.Message |> Error |> Async.fromValue
        
        member this.GetGroupByName name =
            getGroupByName name