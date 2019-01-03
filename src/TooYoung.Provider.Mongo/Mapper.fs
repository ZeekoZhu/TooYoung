module TooYoung.Provider.Mongo.Mapper
open TooYoung.Provider.Mongo.Enities
open WebCommon.Impure

module FileDirectory =
    open TooYoung.Domain.FileDirectory
    open TooYoung.Domain.Resource

    let toEntity (model: FileDirectory): FileDirectoryEntity =
        FileDirectoryEntity
             ( Id = model.Id,
               IsRoot = model.IsRoot,
               Name = model.Name,
               OwnerId = model.OwnerId,
               ParentId = model.ParentId,
               DirectoryChildren = toCsList model.DirectoryChildren,
               FileChildren = (model.FileChildren |> List.map (fun x -> x.Id) |> toCsList)
             )

    let toModel (entity: FileDirectoryEntity): FileDirectory =
        FileDirectory
            ( entity.Id, entity.OwnerId, entity.IsRoot,
              Name = entity.Name, DirectoryChildren = ofCsList entity.DirectoryChildren,
              ParentId = entity.ParentId,
              FileChildren =
                ( entity.FileChildren
                  |> Seq.map (fun x -> FileInfo(x, entity.OwnerId, ""))
                  |> ofCsList
                )
            )

module AccessDefinition =
    open TooYoung.Domain.Authorization.UserGroup
    
    let toEntity (model: AccessDefinition):AccessDefinitionEntity =
        { Target = model.Target
          Constraint =
            match model.Constraint with
            | All -> "*"
            | Instance id -> id
          Restrict = model.Restrict
          AccessOperation =
            match model.AccessOperation with
            | Any -> "*"
            | AccessOperation.Action act -> act
        }

    let toModel (entity: AccessDefinitionEntity): AccessDefinition =
        { Target = entity.Target
          Constraint =
            match entity.Constraint with
            | "*" -> AccessConstraint.All
            | target -> AccessConstraint.Instance target
          Restrict = entity.Restrict
          AccessOperation =
            match entity.AccessOperation with
            | "*" -> AccessOperation.Any
            | op -> AccessOperation.Action op
        }

module UserGroup =
    open System
    open TooYoung.Domain.Authorization.UserGroup
    
    let toEntity (model: UserGroup) =
        UserGroupEntity
            ( Id = model.Id,
              Users = toCsList model.Users,
              Name = model.Name,
              Definitions =
                ( model.AccessDefinitions
                  |> List.map (AccessDefinition.toEntity)
                  |> toCsList
                )
            )
    
    let toModel (entity: UserGroupEntity): UserGroup =
        UserGroup
            ( entity.Id,
              Users = ofCsList entity.Users,
              Name = entity.Name,
              AccessDefinitions =
                ( entity.Definitions
                  |> Seq.map (AccessDefinition.toModel)
                  |> ofCsList
                )
            )

module TokenRule =
    open System
    open TooYoung.Domain.Sharing
    let toEntity (model: TokenRule) =
        TokenRuleEntity
            ( Id = model.Id,
              Token = model.Token,
              Password = model.Password,
              ExpiredAt = (model.ExpiredAt |> Option.toNullable)
            )

    let toModel (entity: TokenRuleEntity): TokenRule =
        { Id = entity.Id
          Token = entity.Token
          ExpiredAt = entity.ExpiredAt |> Option.ofNullable
          Password = entity.Password
        }

module RefererRule =
    open System
    open TooYoung.Domain.Sharing
    let toEntity (model: RefererRule) =
        RefererRuleEntity
            ( Id = model.Id,
              AllowedHost = model.AllowedHost
            )
    let toModel (entity: RefererRuleEntity): RefererRule =
        { Id = entity.Id
          AllowedHost = entity.AllowedHost
        }

module SharingEntry =
    open System
    open TooYoung.Domain.Sharing
    
    let toEntity (model: SharingEntry) =
        SharingEntryEntity
            ( Id = model.Id,
              Type = model.Type,
              ResourceId = model.ResourceId,
              OwnerId = model.OwnerId,
              TokenRules =
                ( model.TokenRules
                  |> mapToCsList TokenRule.toEntity
                ),
              RefererRules =
                ( model.RefererRules
                  |> mapToCsList (RefererRule.toEntity)
                )
            )
    
    let toModel (entity: SharingEntryEntity) =
        SharingEntry
            ( entity.Id, entity.OwnerId, entity.ResourceId,
              TokenRules = (entity.TokenRules |> mapFromCsList TokenRule.toModel),
              RefererRules = (entity.RefererRules |> mapFromCsList RefererRule.toModel)
            )