namespace TooYoung.Provider.Mongo.Enities
open System
open WebCommon.Impure

/// 为了方便映射对目录下子文件的引用，所以为目录创建了一个用来表达存储关系的实体
[<AllowNullLiteral>]
type FileDirectoryEntity() =
    member val Id = String.Empty with get, set
    member val OwnerId = String.Empty with get, set
    member val IsRoot = false with get, set
    member val Name = String.Empty with get, set
    member val ParentId = String.Empty with get, set
    member val DirectoryChildren = CsList<string>() with get, set
    member val FileChildren = CsList<string>() with get, set


