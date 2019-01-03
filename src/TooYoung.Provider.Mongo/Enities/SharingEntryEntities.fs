namespace TooYoung.Provider.Mongo.Enities

open TooYoung.Domain.Sharing
open System
open WebCommon.Impure

[<AllowNullLiteral>]
type TokenRuleEntity() =
    member val Id = String.Empty with get, set
    member val ExpiredAt = Nullable<DateTime>() with get, set
    member val Password = String.Empty with get, set
    member val Token = String.Empty with get, set

[<AllowNullLiteral>]
type RefererRuleEntity() =
    member val AllowedHost = String.Empty with get, set
    member val Id = String.Empty with get, set


[<AllowNullLiteral>]
type SharingEntryEntity() =
    member val Id = String.Empty with get, set
    member val Type = SharingType.File with get, set
    member val ResourceId = String.Empty with get, set
    member val OwnerId = String.Empty with get, set
    member val TokenRules = CsList<TokenRuleEntity>() with get, set
    member val RefererRules = CsList<RefererRuleEntity>() with get, set

