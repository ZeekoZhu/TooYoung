namespace TooYoung.Domain.Authorization

module UserGroup =
    open System.Runtime.CompilerServices
    open System

    type AccessConstraint =
        | All
        | Instance of string

    type AccessOperation =
        | Any
        | Action of string

    type AccessTargetType = string

     /// 当校验权限时，用来传递权限信息
    type Permission =
        { Constraint: AccessConstraint
          Target: AccessTargetType
          AccessOperation: AccessOperation
        }
    
    
    type AccessDefinition =
        { Target: AccessTargetType
          Constraint: AccessConstraint
          Restrict: bool
          AccessOperation: AccessOperation
        }


    let containsPermission (other: Permission) (this: AccessDefinition) =
            if this.Restrict = true || this.Target <> other.Target then false
            else
                match (this.AccessOperation, other.AccessOperation) with
                 | (Any, _) -> true
                 | (Action _, Any) -> false
                 | (Action action, Action otherAction) -> action = otherAction
                &&
                match (this.Constraint, other.Constraint) with
                 | (All, _) -> true
                 | (Instance _, All) -> false
                 | (Instance x, Instance y) -> x = y
    
    let containsDefinition (other: AccessDefinition) (this: AccessDefinition) =
        if this.Target <> other.Target || this.Restrict <> other.Restrict then false
        else
            match (this.AccessOperation, other.AccessOperation) with
             | (Any, _) -> true
             | (Action _, Any) -> false
             | (Action action, Action otherAction) -> action = otherAction
            &&
            match (this.Constraint, other.Constraint) with
             | (AccessConstraint.All, _) -> true
             | (AccessConstraint.Instance _, AccessConstraint.All) -> false
             | (Instance x, Instance y) -> x = y

    let rec mergeDefinitions (newPermissions: AccessDefinition list) (src: AccessDefinition list) =
        match newPermissions with
        | [] -> src
        | def:: rest ->
            if src |> List.exists (containsDefinition def) then src
            else def :: src
            |> mergeDefinitions rest

   
    /// Group 用来为一群用户授权
    type UserGroup(id: Guid) =
        member val Id = id
        member val Users = List<Guid>.Empty with get, set
        member val Name = String.Empty with get, set
        member val AccessDefinitions = List<AccessDefinition>.Empty with get, set
    with
        member this.AddAccessDefinitions defs =
            this.AccessDefinitions <- mergeDefinitions this.AccessDefinitions defs
        member this.AddPermissions
            (permissions: Permission list) =
                let accessDefs =
                    permissions
                    |> List.map
                        ( fun x ->
                            { Target = x.Target
                              Constraint = x.Constraint
                              Restrict = false
                              AccessOperation = x.AccessOperation
                            }
                        )
                this.AccessDefinitions <- mergeDefinitions this.AccessDefinitions accessDefs

    [<Extension>]
    type PermissionUtils () =
        [<Extension>]
        static member inline Test (this: AccessDefinition seq) (others: Permission List) =
            let defs = this |> List.ofSeq
            others
            |> List.fold
                (fun result other -> if result then defs |> List.exists (containsPermission other) else false)
                true 