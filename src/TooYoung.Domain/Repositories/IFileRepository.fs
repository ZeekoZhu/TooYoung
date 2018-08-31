namespace TooYoung.Domain.Repositories
open System.Threading.Tasks
open TooYoung.Domain.Resource



type IFileRepository =
    inherit IRepository

    abstract member AddAsync: FileInfo -> Task<Result<FileInfo, string>>
    
    abstract member GetByIdAsync: string -> Task<FileInfo option>
    
    abstract member CreateBinaryAsync: byte[] -> Task<Result<FileBinary, string>>

    abstract member DeleteBinaryAsync: string -> Task<Result<unit, string>>

    abstract member GetByIdAndUserAsync: userId: string -> fileInfoId: string -> Task<FileInfo option>

    abstract member UpdateAsync: FileInfo -> Task<Result<unit, string>>

    abstract member ExistsAsync: fileInfoId: string -> userId: string -> Task<bool>

    abstract member DeleteFileAsync: fileInfoId: string -> Task<Result<unit, string>>