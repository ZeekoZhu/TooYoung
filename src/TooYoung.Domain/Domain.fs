namespace TooYoung.Domain

module Resource =
    open System
    open System.Collections.Generic
    
    /// 文件信息
    type FileInfo(id: string, ownerId: string, name: string) =
        member val Id = id
        member val OwnerId = ownerId
        member val Name = name with get, set

        /// 文件大小
        member val FileSize = 0 with get, set
        member val BinaryId = String.Empty with get, set
        member val Metadatas = Dictionary<string, string>()
        member val Extension = String.Empty with get, set

    /// 文件二进制存储实体
    type FileBinary(id: string) =
        member val Id = id
        member val Binary = Array.empty<byte> with get, set

module Sharing =
    open System
    open System.Text.RegularExpressions
    open Microsoft.FSharp.Quotations.Patterns
    open Microsoft.FSharp.Reflection
    open Resource

    /// 来源引用规则
    type RefererRule =
        { AllowedHost: string }

    /// Token 规则
    type TokenRule =
        { Token: string
          ExpiredAt: DateTime option
          Password: string }

    /// 访问认证信息
    type AccessClaim =
        { Host: string option
          Token: (string * DateTime * string) option }
    
    let rec isUnionCase =
        function 
        | Lambda(_, expr) | Let(_, _, expr) -> isUnionCase expr
        | NewTuple exprs -> 
            let iucs = List.map isUnionCase exprs
            fun value -> List.exists ((|>) value) iucs
        | NewUnionCase(uci, _) -> 
            let utr = FSharpValue.PreComputeUnionTagReader uci.DeclaringType
            box
            >> utr
            >> (=) uci.Tag
        | _ -> failwith "Expression is no union case."

    /// 测试来源规则是否满足
    let refererRules host rule =
        let regex = Regex(rule)
        regex.IsMatch(host)

    /// 测试 token 是否满足
    let isTokenMatch (token, date, password) (rule: TokenRule) =
                rule.Token = token
                 && match rule.ExpiredAt with
                    | Some expire -> expire >= date
                    | None -> true
                 && rule.Password = password

    let rec canAccessVia test rules claim =
        List.exists (test claim) rules

    let rec canAccessViaReferer (allowedHosts: string list) (claim: string option) =
        match claim with
        | None -> false
        | Some host -> canAccessVia refererRules allowedHosts host
    
    let rec canAccessViaSharingToken (tokens: TokenRule list) (claim: (string * DateTime * string) option) =
        match claim with 
        | None -> false
        | Some token -> canAccessVia isTokenMatch tokens token

    /// 文件分享信息
    type SharingEntry(id: string, ownerId: string, resourceId: string) =
        member val Id = id
        member val ResourceId = resourceId
        member val OwnerId = ownerId
        member val TokenRules = List.empty<TokenRule> with get, set
        member val RefererRules = List.empty<RefererRule> with get, set

    let canAccess (entry: SharingEntry) (claims: AccessClaim) =
        if entry.RefererRules.IsEmpty && entry.TokenRules.IsEmpty then true
        else canAccessViaReferer
                (entry.RefererRules |> List.map (fun x -> x.AllowedHost))
                claims.Host
             || canAccessViaSharingToken entry.TokenRules claims.Token


module FileDirectory =
    open Resource

    open System
    type FileDirectory(id: string, ownerId: string) =
        member val Id = id
        member val OwnerId = ownerId
        member val DirectoryChildren = List.empty<FileDirectory> with get, set
        member val FileChildren = List.empty<FileInfo> with get, set
