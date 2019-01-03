namespace TooYoung.Domain.Repositories
open System.Threading.Tasks
open TooYoung.Domain.FileDirectory
open TooYoung.Domain.Resource

type IDirectoryRepository =
    inherit IRepository
    abstract GetRootDir: userId: string -> Async<FileDirectory option>
    abstract GetDir: dirId: string -> userId: string -> Async<FileDirectory option>
    abstract SaveNewNode: dir: FileDirectory -> Async<Result<FileDirectory, AppError>>
    abstract UpdateNode: dir: FileDirectory -> Async<Result<unit, AppError>>
    abstract ContainsName: name: string -> dir: FileDirectory -> Async<bool>
    abstract ContainsFileName: fileName: string -> dir: FileDirectory -> Async<bool>
    /// 向一个文件夹中添加子文件夹引用
    abstract AttachSubDirsAsync: dir: FileDirectory -> string list -> Async<Result<unit, AppError>>
    /// 像一个文件夹中添加文件引用
    abstract AttachFilesAsync: dir: FileDirectory -> string list -> Async<Result<unit, AppError>>
    abstract DeattachSubDirsAsync: dir: FileDirectory -> string list -> Async<Result<unit, AppError>>
    abstract DeattachFilesAsync: dir: FileDirectory -> string list -> Async<Result<unit, AppError>>
    abstract DeleteDir: dir: FileDirectory -> Async<Result<unit, AppError>>
    abstract GetParentDirAsync: file:FileInfo -> Async<Result<FileDirectory, AppError>>