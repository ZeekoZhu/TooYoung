namespace TooYoung.Domain.Repositories
open TooYoung.Domain.Sharing
open System

type ISharingRepository =
    inherit IRepository
    abstract GetEntryAsync: resource: string -> Async<SharingEntry option>
    abstract GetAllEntries: user: string -> Async<SharingEntry list> 
    abstract AddEntry: entry: SharingEntry -> Async<Result<SharingEntry, AppError>>
    abstract AddRefererRule: entry: SharingEntry -> referer: RefererRule -> Async<Result<SharingEntry, AppError>>
    abstract AddTokenRule: entry: SharingEntry -> token: TokenRule -> Async<Result<SharingEntry, AppError>>
    abstract RemoveTokenRule: entry: SharingEntry -> tokenId: string -> Async<Result<unit,AppError>>
    abstract RemoveRefererRule: entry: SharingEntry -> refererId: string -> Async<Result<unit,AppError>>
    abstract DeleteEntry: entry: SharingEntry -> Async<Result<unit, AppError>>
