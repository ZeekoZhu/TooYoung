namespace TooYoung.Domain.Services
open System
open TooYoung
open TooYoung.Domain
open TooYoung.Domain.Repositories
open TooYoung.Domain.Sharing
open TooYoung.Domain.Resource
open Asyncx
open FunxAlias

type SharingService (repo: ISharingRepository, fileSvc: FileService) =
    let startWork = Repository.startWork repo
    let unitwork = Repository.unitWork repo

    /// 为指定文件添加新的分享入口
    let addSharingEntryFor (resource: FileInfo) =
        let newEntry = SharingEntry(Guid.NewGuid().ToString(), resource.OwnerId, resource.Id)
        repo.AddEntry newEntry

    /// 在对分享入口进行操作前，先获取对应的实体
    let getEntryBeforeOperating resourceId userId =
        fileSvc.GetById resourceId
        =>> (fun resource ->
            repo.GetEntryAsync resourceId
            |> Async.map (function
                | Some e when e.OwnerId = userId -> Some e
                | _ -> None
            )
            |> Async.bind (function
                | Some entry -> entry |> Ok |> Async.fromValue
                | None -> addSharingEntryFor resource
            )
        )

    /// 使用凭证获取分享出来的资源 
    member this.GetResourceAsync (claim: AccessClaim) (resourceId) (userId) =
        // 先查找该资源有没有被分享出来
        repo.GetEntryAsync resourceId
        |> Async.fromOption (Error "Resource is private")
        // 检查是否有权限，所有者可以直接通过检查
        >=> (fun entry ->
            entry.OwnerId = userId || Sharing.canAccess entry claim
        )
        =>> (function
            | true -> fileSvc.GetById resourceId
            | false -> "Access denied" |> Error |> Async.fromValue
        )

    /// 获取指定资源的所有分享入口
    member this.GetEntryByResource (resourceId) (userId) =
        repo.GetEntryAsync resourceId
        |> Async.fromOption (Error "Resource is private")
        >=> (fun entry -> (entry.OwnerId = userId, entry))
        |> AsyncResult.bind (function
            | (true, entry) -> Ok entry
            | (false, _) -> Error ""
            )

    /// 获取用户的所有分享入口
    member this.GetAllEntries (userId) =
        repo.GetAllEntries userId

    /// 添加一个 referer 规则
    member this.AddRefererRule resourceId userId referer =
        getEntryBeforeOperating resourceId userId
        =>> flip repo.AddRefererRule referer

    /// 添加一个 token 规则
    member this.AddTokenRule resourceId userId token =
        getEntryBeforeOperating resourceId userId
        =>> flip repo.AddTokenRule token

    /// 删除一条 token 规则
    member this.RemoveTokenRule resourceId userId tokenId =
        getEntryBeforeOperating resourceId userId
        =>> flip repo.RemoveTokenRule tokenId

    /// 删除一条 referer 规则
    member this.RemoveRefererRule resourceId userId refererId =
        getEntryBeforeOperating resourceId userId
        =>> flip repo.RemoveRefererRule refererId