namespace TooYoung.Domain.Repositories
open System.Threading.Tasks
open TooYoung.Domain.FileDirectory

type IDirectoryRepository =
    inherit IRepository
    abstract GetRootDir: userId: string -> Async<FileDirectory option>
    abstract GetDir: dirId: string -> userId: string -> Async<FileDirectory option>
    abstract SaveNewNode: dir: FileDirectory -> Async<Result<FileDirectory, string>>
    abstract UpdateNode: dir: FileDirectory -> Async<Result<unit, string>>
    abstract ContainsName: name: string -> dir: FileDirectory -> Async<bool>
    abstract AttachSubDirsAsync: dir: FileDirectory -> string list -> Async<Result<unit, string>>
    abstract AttachFilesAsync: dir: FileDirectory -> string list -> Async<Result<unit, string>>
    abstract DeattachSubDirsAsync: dir: FileDirectory -> string list -> Async<Result<unit, string>>
    abstract DeattachFilesAsync: dir: FileDirectory -> string list -> Async<Result<unit, string>>
    abstract DeleteDir: dir: FileDirectory -> Async<Result<unit, string>>