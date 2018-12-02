namespace TooYoung.Provider.Mongo.Enities

open System
open WebCommon.Impure

type AccessDefinitionEntity =
    { Target: string
      Constraint: string
      Restrict: bool
      AccessOperation: string
    }

[<AllowNullLiteral>]
type UserGroupEntity() =
    member val Id = Guid.Empty with get, set
    member val Users = CsList<Guid>() with get, set
    member val Name = String.Empty with get, set
    member val Definitions = CsList<AccessDefinitionEntity>() with get, set