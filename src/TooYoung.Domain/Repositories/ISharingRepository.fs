namespace TooYoung.Domain.Repositories
open TooYoung.Domain.Sharing
open System

type ISharingRepository =
    inherit IRepository
    abstract GetEntryAsync: resource: string -> Async<SharingEntry option>
    abstract GetAllEntries: user: string -> Async<SharingEntry list> 
    abstract AddEntry: entry: SharingEntry -> Async<Result<SharingEntry, string>>
    abstract AddRefererRule: entry: SharingEntry -> referer: RefererRule -> Async<Result<SharingEntry, string>>
    abstract AddTokenRule: entry: SharingEntry -> token: TokenRule -> Async<Result<SharingEntry, string>>
    abstract RemoveTokenRule: entry: SharingEntry -> tokenId: string -> Async<Result<unit,string>>
    abstract RemoveRefererRule: entry: SharingEntry -> refererId: string -> Async<Result<unit,string>>
