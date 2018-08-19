namespace TooYoung.Domain

module Resource =
    open System

    type RefererRule =
        { AllowedHost: string
        }
    type SharingRule =
        { Token: string
          ExpiredAt: DateTime option
        }
    type AccessRule =
        | Referer of RefererRule
        | SharingToken of SharingRule

    type AccessClaim =
        | RefererHost of string
        | SharingToken of string * DateTime

    type Metadata =
        { Name: string
          Value: string
        }

    type FileInfo(id: string, ownerId: string, name:string ) =
        member val Id = id
        member val OwnerId = ownerId with get, set
        member val Name = name with get, set
        /// 文件大小
        member val FileSize = 0 with get, set
        member val Metadatas: Metadata list = []
        member val FileId = "" with get, set
        member val Extension = "" with get, set
        member val AccessRules: AccessRule list = [] with get, set

    type FileBinary(id: string) =
        member val Id = id
        member val Binary: byte array = [||] with get, set

open Resource

module ResouceService =
    open Microsoft.FSharp.Quotations.Patterns
    open Microsoft.FSharp.Reflection
    open System


    let rec isUnionCase = function
    | Lambda (_, expr) | Let (_, _, expr) -> isUnionCase expr
    | NewTuple exprs -> 
        let iucs = List.map isUnionCase exprs
        fun value -> List.exists ((|>) value) iucs
    | NewUnionCase (uci, _) ->
        let utr = FSharpValue.PreComputeUnionTagReader uci.DeclaringType
        box >> utr >> (=) uci.Tag
    | _ -> failwith "Expression is no union case."

    let testHost x host = host = x
    let testToken (token, date) rule =
        rule.Token = token
                && match rule.ExpiredAt with
                    | Some expire -> expire >= date
                    | None -> true
    let rec canAccessVia test rules claims =
        match claims with
        | [] -> false
        | x::rest ->
            List.exists (test x) rules || canAccessVia test rules rest

    let rec canAccessViaReferer (allowedHosts:string list) (claims:string list) =
        canAccessVia testHost allowedHosts claims
        
    let rec canAccessViaSharingToken (tokens:SharingRule list) (claims:(string*DateTime) list) =
        canAccessVia testToken tokens claims