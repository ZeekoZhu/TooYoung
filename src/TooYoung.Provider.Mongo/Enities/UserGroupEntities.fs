namespace TooYoung.Provider.Mongo.Enities

open System

type AccessDefinitionEntity =
    { Target: string
      Constraint: string
      Restrict: bool
      AccessOperation: string
    }

[<AllowNullLiteral>]
type UserGroupEntity() =
    member val Id = Guid.Empty with get, set
    member val Users = List<Guid>.Empty with get, set
    member val Name = String.Empty with get, set
    member val Definitions = List<AccessDefinitionEntity>.Empty with get, set