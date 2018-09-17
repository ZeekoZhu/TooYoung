namespace TooYoung.Domain.Repositories
open TooYoung.Domain.Resource



type IFileRepository =
    inherit IRepository
    abstract member ListByIdAsync: ids: string list -> Async<FileInfo list>

    abstract member AddAsync: FileInfo -> Async<Result<FileInfo, string>>
    
    abstract member GetByIdAsync: string -> Async<FileInfo option>
    
    abstract member CreateBinaryAsync: byte[] -> Async<Result<FileBinary, string>>

    abstract member DeleteBinaryAsync: string -> Async<Result<unit, string>>

    abstract member GetByIdAndUserAsync: userId: string -> fileInfoId: string -> Async<FileInfo option>

    abstract member UpdateAsync: FileInfo -> Async<Result<unit, string>>

    abstract member ExistsAsync: fileInfoId: string -> userId: string -> Async<bool>

    abstract member DeleteFileAsync: fileInfoId: string -> Async<Result<unit, string>>

    abstract member GetBinaryAsync: binaryId: string -> Async<Result<FileBinary, string>>