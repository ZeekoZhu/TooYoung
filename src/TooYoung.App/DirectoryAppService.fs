namespace TooYoung.App
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open TooYoung.Async
open TooYoung.Domain
open TooYoung.Domain.FileDirectory
open TooYoung.Domain.Repositories
open TooYoung.Domain.Resource
open TooYoung.Domain.Services
open TooYoung.FunxAlias

type DirectoryAppService
    ( dirSvc: DirectoryService,
      fileAppSvc: FileAppService,
      dirRepo: IDirectoryRepository
    ) =
    let allTaskComplete =
        Async.Parallel
        >> Async.map (fun results ->
            results
            |> (Seq.forall (function
               | Ok _ -> true
               | Error _ -> false
               )
               >> (function
                   | true -> Ok() 
                   | false -> Error (Multiple "Operation failed, please check log")
                   )
               )
        )
        
    /// 彻底删除文件夹中直接包含的文件
    let deleteAllFilesOf (dir: FileDirectory) =
        dir.FileChildren
        |> List.map
            ( fun file -> fileAppSvc.DeleteFileAndSharing file
            )
        |> allTaskComplete
    
    /// 彻底递归删除指定的文件夹
    let rec ``rm -rf`` (dir: FileDirectory) =
        if dir.DirectoryChildren.IsEmpty
        then () |> AsyncResult.retn
        else dir.DirectoryChildren
                |> List.map (fun dirId -> dirSvc.GetDir (dirId, dir.OwnerId))
                |> List.map (AsyncResult.bind ``rm -rf``)
                |> allTaskComplete
        >>= just deleteAllFilesOf dir
        >>= just dirRepo.DeleteDir dir
   
    member __.DeleteFileInDir (file: FileInfo) (dir: FileDirectory) =
        dirSvc.DeleteFile file dir
        >>= ( fun dir ->
            fileAppSvc.DeleteFileAndSharing file
            <>> switchTo dir
        )

    /// 递归删除指定文件夹，只用于在后台进程删除文件夹
    member __.RmRf dir =
        ``rm -rf`` dir

    member __.GetDirs dirIds userId =
        dirRepo.GetDirs dirIds userId