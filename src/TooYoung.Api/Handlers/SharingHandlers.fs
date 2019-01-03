module TooYoung.Api.Handlers.SharingHandlers

open System
open FSharp.Control.Tasks.V2
open Giraffe
open TooYoung.Api.Handlers
open TooYoung.Api.ServiceAccessor
open TooYoung.Domain.Services
open TooYoung.Domain.Sharing
open TooYoung.WebCommon

let getEntryForFile (fileInfoId: string) :HttpHandler =
    fun next ctx ->
        let shareSvc = getSharingSvc ctx
        let userId = ctx.UserId()
        shareSvc.GetEntryByResource fileInfoId userId
        |> AppResponse.appResult next ctx

let getAllEntries : HttpHandler =
    fun next ctx ->
        let shareSvc = getSharingSvc ctx
        let userId = ctx.UserId()
        task {
            let! result = shareSvc.GetAllEntries userId
            return! json result next ctx
        }

type AddRefererRuleModel =
    { AllowedHost: string
      FileInfoId: string
    }

type AddTokenRuleModel =
    { ExpiredAt: DateTime option
      Password: string
      FileInfoId: string
    }

let addRefererRule (model: AddRefererRuleModel) : HttpHandler =
    fun next ctx ->
        let userId = ctx.UserId()
        let shareSvc = getSharingSvc ctx
        let rule: RefererRule =
            { AllowedHost = model.AllowedHost
              Id = Guid.NewGuid().ToString()
            }
        shareSvc.AddRefererRule model.FileInfoId userId rule
        |> AppResponse.appResult next ctx

let addTokenRule (model: AddTokenRuleModel): HttpHandler =
    fun next ctx ->
        let userId = ctx.UserId()
        let shareSvc = getSharingSvc ctx
        let rule: TokenRule =
            { Token = Guid.NewGuid().ToString()
              ExpiredAt = model.ExpiredAt
              Password = model.Password
              Id = Guid.NewGuid().ToString()
            }
        shareSvc.AddTokenRule model.FileInfoId userId rule
        |> AppResponse.appResult next ctx

let deleteRule removeAction (fileInfoId: string , ruleId: string): HttpHandler =
    fun next ctx ->
        let userId = ctx.UserId()
        let shareSvc = getSharingSvc ctx
        (removeAction shareSvc) fileInfoId userId ruleId
        |> AppResponse.appResult next ctx

let removeRefererRule = 
    deleteRule (fun svc -> svc.RemoveRefererRule)

let removeTokenRule =
    deleteRule (fun svc -> svc.RemoveTokenRule)

let routes: HttpHandler =
    subRouteCi "/sharing"
        ( AuthGuard.requireAcitveUser >=> choose
            [ GET >=> choose
                [ routeCi "/" >=> getAllEntries
                  routeCif "/%s" getEntryForFile
                ]
              POST >=> choose
                [ routeCi "/referer" >=> bindJson<AddRefererRuleModel> addRefererRule
                  routeCi "/token" >=> bindJson<AddTokenRuleModel> addTokenRule
                ]
              DELETE >=> choose
                [ routeCif "/%s/referer/%s" removeRefererRule
                  routeCif "/%s/token/%s" removeTokenRule
                ]
            ]
        )
