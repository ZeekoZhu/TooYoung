namespace TooYoung.Domain.Repositories
open FSharpx.Task
open System.Threading.Tasks
open TooYoung.Domain.FileDirectory

type IDirectoryRepository =
    inherit IRepository
    abstract GetRootDir: userId: string -> Task<FileDirectory option>
    abstract GetDir: dirId: string -> userId: string -> Task<FileDirectory option>
    abstract SaveNewNode: dir: FileDirectory -> Task<Result<FileDirectory, string>>
    abstract UpdateNode: dir: FileDirectory -> Task<Result<unit, string>>
    abstract ContainsName: name: string -> dir: FileDirectory -> Task<bool>
    abstract AttachSubDirsAsync: dir: FileDirectory -> string list -> Task<Result<unit, string>>
    abstract AttachFilesAsync: dir: FileDirectory -> string list -> Task<Result<unit, string>>
    abstract DeattachSubDirsAsync: dir: FileDirectory -> string list -> Task<Result<unit, string>>
    abstract DeattachFilesAsync: dir: FileDirectory -> string list -> Task<Result<unit, string>>
    abstract DeleteDir: dir: FileDirectory -> Task<Result<unit, string>>