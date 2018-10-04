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
open Asyncx
open TooYoung.Domain.Sharing
open Utils

type SharingRepository (db: IMongoDatabase) =
    let entries = db.GetCollection<SharingEntry>("SharingEntry")

    let getEntry resource =
        task {
            let! result = entries.Find(fun x -> x.ResourceId = resource).FirstOrDefaultAsync()
            return Option.ofObj result
        } |> Async.AwaitTask

    let getAllEntries userId =
        task {
            let! result = entries.Find(fun x -> x.OwnerId = userId).ToListAsync()
            return List.ofSeq result
        } |> Async.AwaitTask

    let addEntry entry =
        entries.InsertOneAsync(entry)
        |> Async.AwaitTask
        <!> (fun _ -> Ok entry)

    let addRefererRule (entry: SharingEntry) referer =
        let update = Builders<SharingEntry>.Update.Push
                        ((fun x -> x.RefererRules :> IEnumerable<RefererRule>), referer)
        entries.FindOneAndUpdateAsync<SharingEntry, SharingEntry>
            ((fun x -> x.Id = entry.Id), update)
        |> Async.AwaitTask
        <!> (fun _ ->
            entry.RefererRules <- referer :: entry.RefererRules
            Ok entry
        )

    let addTokenRule (entry: SharingEntry) token =
        let update = Builders<SharingEntry>.Update.Push
                        ((fun x -> x.TokenRules :> IEnumerable<TokenRule>), token)
        entries.FindOneAndUpdateAsync<SharingEntry, SharingEntry>
            ((fun x -> x.Id = entry.Id), update)
        |> Async.AwaitTask
        <!> (fun _ ->
            entry.TokenRules <- token :: entry.TokenRules
            Ok entry
        )

    let removeTokenRule (entry: SharingEntry) tokenId =
        let update = Builders<SharingEntry>.Update.PullFilter<TokenRule>
                        ((fun x -> x.TokenRules :> IEnumerable<TokenRule>)
                        ,(fun x -> x.Id = tokenId)
                        )
        entries.FindOneAndUpdateAsync<SharingEntry, SharingEntry>
            ((fun x -> x.Id = entry.Id), update)
        |> Async.AwaitTask
        <!> (fun _ -> Ok ())

    let removeRefererRule (entry: SharingEntry) refererId =
        let update = Builders<SharingEntry>.Update.PullFilter
                        ((fun x -> x.RefererRules :> IEnumerable<RefererRule>)
                        ,(fun (x: RefererRule) -> x.Id = refererId)
                        )
        entries.FindOneAndUpdateAsync<SharingEntry, SharingEntry>
            ((fun x -> x.Id = entry.Id), update)
        |> Async.AwaitTask
        <!> (fun _ -> Ok ())

    interface ISharingRepository with
        member this.BeginTransaction() =
            new MongoUnitOfWork(db.Client) :> UnitOfWork

        member this.GetEntryAsync resource =
            try getEntry resource
            with _ -> None |> Async.fromValue

        member this.GetAllEntries user =
            try getAllEntries user
            with _ -> [] |> Async.fromValue

        member this.AddEntry entry =
            onError None addEntry entry

        member this.AddRefererRule entry referer =
            onError None (addRefererRule entry) referer

        member this.AddTokenRule entry token =
            onError None (addTokenRule entry) token

        member this.RemoveTokenRule entry tokenId =
            onError None (removeTokenRule entry) tokenId

        member this.RemoveRefererRule entry refererId =
            onError None (removeRefererRule entry) refererId