namespace TooYoung.Domain

module Resource =
    open System
    open System.Collections.Generic
    
    [<CLIMutable>]
    type DocumentInfo =
        { Id: string
          OwnerId: string
          Name: string;
          FileSize: string;
          Metadatas: string;
          DateModified: DateTime;
        }
    
    /// 文件信息
    [<AllowNullLiteral>]
    type FileInfo(id: string, ownerId: string, name: string) =
        member val Id = id with get, set
        member val OwnerId = ownerId with get, set
        member val Name = name with get, set

        /// 文件大小, bytes
        member val FileSize = 0 with get, set
        member val BinaryId = String.Empty with get, set
        member val DateModified = DateTime.Now with get, set
        member val Metadatas = Dictionary<string, string>() with get, set

    /// 文件二进制存储实体
    type FileBinary(id: string) =
        member val Id = id with get, set
        member val Binary = Array.empty<byte> with get, set

module Sharing =
    open System
    open System.Text.RegularExpressions
    open Microsoft.FSharp.Quotations.Patterns
    open Microsoft.FSharp.Reflection
    open Resource

    /// 来源引用规则
    type RefererRule =
        { AllowedHost: string
          Id: string
        }

    /// Token 规则
    type TokenRule =
        { Token: string
          ExpiredAt: DateTime option
          Password: string
          Id: string
        }

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

    type SharingType =
        | File = 0
        | Directory = 1

    /// 文件分享信息
    [<AllowNullLiteral>]
    type SharingEntry(id: string, ownerId: string, resourceId: string) =
        member val Id = id
        member val Type = SharingType.File
        member val ResourceId = resourceId
        member val OwnerId = ownerId
        member val TokenRules = List.empty<TokenRule> with get, set
        member val RefererRules = List.empty<RefererRule> with get, set

    let canAccess (entry: SharingEntry) (claims: AccessClaim) =
        if entry.RefererRules.IsEmpty && entry.TokenRules.IsEmpty then false
        else canAccessViaReferer
                (entry.RefererRules |> List.map (fun x -> x.AllowedHost))
                claims.Host
             || canAccessViaSharingToken entry.TokenRules claims.Token

module FileDirectory =
    open Resource

    open System
    /// 考虑到在数据库层面的实现中，为了能够准确对文件夹下面的子项进行批量的移动操作，所以将这些东西抽象成了 Operation
    type DirectoryOperaion =
        | AddSubDir of string
        | RemoveSubDir of string
        | AddItem of FileInfo
        | RemoveItem of string

    [<AllowNullLiteral>]
    type FileDirectory(id: string, ownerId: string, isRoot: bool) =
        member val Id = id
        member val OwnerId = ownerId
        member val IsRoot = isRoot
        member val Name = String.Empty with get, set
        member val ParentId = String.Empty with get, set
        member val DirectoryChildren = List.empty<string> with get, set
        member val FileChildren = List.empty<FileInfo> with get, set
        member val PendingOperations = List.empty<DirectoryOperaion> with get, set
        member this.ApplyOperations () =
            for op in this.PendingOperations do
                match op with
                | AddSubDir x ->
                    this.DirectoryChildren <- x :: this.DirectoryChildren
                | RemoveSubDir x ->
                    this.DirectoryChildren <-
                        this.DirectoryChildren
                        |> List.filter (fun d -> d <> x)
                | AddItem x ->
                    this.FileChildren <- x :: this.FileChildren
                | RemoveItem x ->
                    this.FileChildren <-
                        this.FileChildren
                        |> List.filter (fun f -> f.Id <> x)
            this.PendingOperations <- List.empty<DirectoryOperaion>
     
        member this.AppendTo (other: FileDirectory) =
            other.PendingOperations <- AddSubDir this.Id :: other.PendingOperations
            this.ParentId <- other.Id
        member this.RemoveFrom (other: FileDirectory) =
            if this.ParentId <> other.Id then ()
            else other.PendingOperations <-
                    RemoveSubDir this.Id :: other.PendingOperations
        member this.AddFile file =
            this.PendingOperations <- AddItem file :: this.PendingOperations
        member this.RemoveFile fileId =
            this.PendingOperations <- RemoveItem fileId :: this.PendingOperations

module User =
    open System
    
    [<CLIMutable>]
    type UserInfo =
        { Id: string;
          UserName: string;
          DisplayName: string;
          Email: string;
          SizeUsedValue: int64;
          Locked: bool;
        }

    [<AllowNullLiteral>]
    type User(id: Guid) =
        member val Id = id with get, set
        member val UserName = String.Empty with get, set
        member val Password = String.Empty with get, set
        member val DisplayName = String.Empty with get, set
        member val Email = String.Empty with get, set
        member val Locked = false with get, set
