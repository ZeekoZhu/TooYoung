namespace TooYoung.Provider.Mongo.Repositories
open System
open System.Linq
open MongoDB.Driver
open MongoDB.Driver.Linq
open TooYoung.Domain.Repositories
open FsToolkit.ErrorHandling

open System.Collections.Generic
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
        groups.AsQueryable()
              .Where(fun g -> g.Users.Any(fun u -> u = userId))
              .SelectMany(fun g -> g.Definitions :> IEnumerable<AccessDefinitionEntity>)
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
            addGroupAsync (Mapper.UserGroup.toEntity group)
            |> Async.map (fun _ -> group |> Ok)
        
        member this.DeleteGroupAsync groupId =
            deleteGroupAsync groupId
            |> Async.map Ok

        member this.GetPermissionForUser userId accessType =
            getPermissionForUser userId accessType
            |> Async.map Ok
        
        member this.UpdateGroupAsync group =
            updateGroupAsync group
            |> Async.map Ok
        
        member this.GetGroupByName name =
            getGroupByName name