namespace FsTest
open TooYoung.Domain.Authorization.UserGroup
open Xunit
open FsUnit

type UserGroupTests() =
    static member ``Contains Permission Data`` () =
        let data = TheoryData<AccessDefinition, Permission, bool>()
        [ { Target = "book"
            Constraint = All
            Restrict = false
            AccessOperation = Action "Read"
          },
          { Target = "book"
            Constraint = Instance "123"
            AccessOperation = Action "Read"
          },
          true
          { Target = "book"
            Constraint = All
            Restrict = false
            AccessOperation = Action "Write"
          },
          { Target = "book"
            Constraint = Instance "123"
            AccessOperation = Action "Read"
          },
          false
          { Target = "book"
            Constraint = All
            Restrict = false
            AccessOperation = Action "Read"
          },
          { Target = "book"
            Constraint = Instance "123"
            AccessOperation = Any
          },
          false
          { Target = "book"
            Constraint = Instance "2333"
            Restrict = false
            AccessOperation = Action "Read"
          },
          { Target = "book"
            Constraint = All
            AccessOperation = Action "Read"
          },
          false
          { Target = "book"
            Constraint = All
            Restrict = false
            AccessOperation = Action "Read"
          },
          { Target = "news paper"
            Constraint = Instance "123"
            AccessOperation = Action "Read"
          },
          false
          { Target = "book"
            Constraint = All
            Restrict = true
            AccessOperation = Any
          },
          { Target = "book"
            Constraint = Instance "123"
            AccessOperation = Any
          },
          false
          { Target = "book"
            Constraint = Instance "2333"
            Restrict = false
            AccessOperation = Action "Read"
          },
          { Target = "book"
            Constraint = Instance "2333"
            AccessOperation = Action "Read"
          },
          true
        ]
        |> List.iter data.Add
        data

    [<Theory; MemberData("Contains Permission Data")>]
    member __.``Contains Permission``
        (this: AccessDefinition, other: Permission, expected: bool) =
        containsPermission other this |> should equal expected