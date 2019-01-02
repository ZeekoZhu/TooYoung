namespace TooYoung.Domain.Repositories
open System.IO
open TooYoung.Domain.Resource



type IFileRepository =
    inherit IRepository
    abstract member ListByIdAsync: ids: string list -> Async<FileInfo list>

    abstract member AddAsync: FileInfo -> Async<Result<FileInfo, AppError>>
    
    abstract member GetByIdAsync: string -> Async<FileInfo option>
    
    abstract member CreateBinaryAsync: byte[] -> Async<Result<FileBinary, AppError>>

    abstract member DeleteBinaryAsync: string -> Async<Result<unit, AppError>>

    abstract member GetByIdAndUserAsync: userId: string -> fileInfoId: string -> Async<FileInfo option>

    abstract member UpdateAsync: FileInfo -> Async<Result<unit, AppError>>

    abstract member ExistsAsync: fileInfoId: string -> userId: string -> Async<bool>

    abstract member DeleteFileAsync: fileInfoId: string -> Async<Result<unit, AppError>>

    abstract member GetBinaryAsync: binaryId: string -> Async<Result<FileBinary, AppError>>

    abstract member GetBinaryStreamAsync: binaryId: string -> Async<Result<Stream, AppError>>
    
    abstract member CreateBinaryFromStreamAsync: stream: Stream -> Async<Result<FileBinary, AppError>>