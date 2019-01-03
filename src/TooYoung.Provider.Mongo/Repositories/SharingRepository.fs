namespace TooYoung.Domain.Repositories
open System
open System.Collections.Generic
open System.IO
open System.Linq
open MongoDB.Driver
open MongoDB.Driver.GridFS
open FSharp.Control.Tasks.V2
open TooYoung
open TooYoung.Domain.Repositories
open TooYoung.Provider.Mongo
open FsToolkit.ErrorHandling
open TooYoung.Domain.Sharing
open TooYoung.Provider.Mongo.Enities
open Utils
open TooYoung.Provider.Mongo.Mapper
open TooYoung.Provider.Mongo.Mapper

type SharingRepository (db: IMongoDatabase) =
    let entries = db.GetCollection<SharingEntryEntity>("SharingEntry")

    let getEntry resource =
        task {
            let! result = entries.Find(fun x -> x.ResourceId = resource).FirstOrDefaultAsync()
            return result |> Option.ofObj |> Option.map SharingEntry.toModel
        } |> Async.AwaitTask

    let getAllEntries userId =
        task {
            let! result = entries.Find(fun x -> x.OwnerId = userId).ToListAsync()
            return result |> List.ofSeq |> List.map SharingEntry.toModel
        } |> Async.AwaitTask

    let addEntry entry =
        entries.InsertOneAsync(entry |> SharingEntry.toEntity)
        |> Async.AwaitTask
        |> Async.map (fun _ -> Ok entry)

    let addRefererRule (entry: SharingEntry) referer =
        let update = Builders<SharingEntryEntity>.Update.Push
                        ( (fun x -> x.RefererRules :> IEnumerable<RefererRuleEntity>),
                          referer |> RefererRule.toEntity
                        )
        entries.FindOneAndUpdateAsync<SharingEntryEntity, SharingEntryEntity>
            ((fun x -> x.Id = entry.Id), update)
        |> Async.AwaitTask
        |> Async.map (fun _ ->
            entry.RefererRules <- referer :: entry.RefererRules
            Ok entry
        )

    let addTokenRule (entry: SharingEntry) token =
        let update = Builders<SharingEntryEntity>.Update.Push
                        ((fun x -> x.TokenRules :> IEnumerable<TokenRuleEntity>), token |> TokenRule.toEntity)
        entries.FindOneAndUpdateAsync<SharingEntryEntity, SharingEntryEntity>
            ((fun x -> x.Id = entry.Id), update)
        |> Async.AwaitTask
        |> Async.map (fun _ ->
            entry.TokenRules <- token :: entry.TokenRules
            Ok entry
        )

    let removeTokenRule (entry: SharingEntry) tokenId =
        let update = Builders<SharingEntryEntity>.Update.PullFilter<TokenRuleEntity>
                        ((fun x -> x.TokenRules :> IEnumerable<TokenRuleEntity>)
                        ,(fun (x: TokenRuleEntity) -> x.Id = tokenId)
                        )
        entries.FindOneAndUpdateAsync<SharingEntryEntity, SharingEntryEntity>
            ((fun x -> x.Id = entry.Id), update)
        |> Async.AwaitTask
        |> Async.map (fun _ -> Ok ())

    let removeRefererRule (entry: SharingEntry) refererId =
        let update = Builders<SharingEntryEntity>.Update.PullFilter
                        ((fun x -> x.RefererRules :> IEnumerable<RefererRuleEntity>)
                        ,(fun (x: RefererRuleEntity) -> x.Id = refererId)
                        )
        entries.FindOneAndUpdateAsync<SharingEntryEntity, SharingEntryEntity>
            ((fun x -> x.Id = entry.Id), update)
        |> Async.AwaitTask
        |> Async.map (fun _ -> Ok ())

    interface ISharingRepository with
        member this.BeginTransaction() =
            new MongoUnitOfWork(db.Client) :> UnitOfWork

        member this.GetEntryAsync resource =
            getEntry resource

        member this.GetAllEntries user =
            getAllEntries user

        member this.AddEntry entry =
             addEntry entry

        member this.AddRefererRule entry referer =
             (addRefererRule entry) referer

        member this.AddTokenRule entry token =
             (addTokenRule entry) token

        member this.RemoveTokenRule entry tokenId =
             (removeTokenRule entry) tokenId

        member this.RemoveRefererRule entry refererId =
             (removeRefererRule entry) refererId