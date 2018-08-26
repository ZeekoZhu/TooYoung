namespace TooYoung.Domain.Repositories
open System.Threading.Tasks
open TooYoung.Domain.Resource



type IFileRepository =
    abstract member AddAsync: FileInfo -> Task<Result<FileInfo, string>>
    
    abstract member  GetByIdAsync: string -> Task<FileInfo option>
    
    abstract  member CreateBinaryAsync: byte[] -> Task<Result<FileBinary, string>>