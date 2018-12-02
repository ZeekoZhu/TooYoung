namespace TooYoung.Domain.Services

open System.Threading.Tasks
open TooYoung.Domain.Repositories
open TooYoung
open TooYoung.Domain
open TooYoung.Domain.Resource
open System
open System.Text.RegularExpressions
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open FunxAlias
open FileDirectory
open FsToolkit.ErrorHandling
open TooYoung

type DirectoryAddDto = {
    Name: string
    ParentId: string
}

type DirectoryService (repo: IDirectoryRepository, fileSvc: FileService, bus: EventBus) =
    let startWork = Repository.startWork repo
    let unitWork = Repository.unitWork repo

    let getDir dirId userId =
        repo.GetDir dirId userId
        |> Async.fromOption (Error "Directory not found")
    let rec getDirPath dirId userId =
        getDir dirId userId
        >>= (fun dir ->
            if dir.IsRoot
            then [dir] |> Ok |> async.Return
            else (getDirPath dir.ParentId userId)
                    |> AsyncResult.map
                        (fun parents -> ( dir :: parents ))
            )

    /// 更新文件夹的子项结构
    let updateDir (dir: FileDirectory) =
        Repository.startWork repo (fun _ ->
            dir.PendingOperations
            |> List.choose (function
                | RemoveSubDir sub -> Some sub
                | _ -> None
                )
            |> repo.DeattachSubDirsAsync dir
            >>= (fun _ ->
                    dir.PendingOperations
                    |> List.choose (function
                        | AddSubDir sub -> Some sub
                        | _ -> None
                        )
                    |>  repo.AttachSubDirsAsync dir
                )
            >>= (fun _ ->
                    dir.PendingOperations
                    |> List.choose (function 
                        | RemoveItem file -> Some file
                        | _ -> None
                        )
                    |> repo.DeattachFilesAsync dir
                )
            >>= (fun _ ->
                    dir.PendingOperations
                    |> List.choose (function 
                        | AddItem file -> Some file.Id
                        | _ -> None
                        )
                    |> repo.AttachFilesAsync dir
                )
        )
        |> AsyncResult.map (fun _ -> dir.ApplyOperations(); dir)

    let createRootDir userId =
        let root = FileDirectory(Guid.NewGuid().ToString(), userId, true)
        unitWork repo.SaveNewNode root

    let createAsSubSir (dto: DirectoryAddDto) (parent: FileDirectory) =
        let subDir = FileDirectory(Guid.NewGuid().ToString(), parent.OwnerId, false)
        subDir.Name <- dto.Name
        subDir.AppendTo parent
        startWork (fun _ ->
            repo.SaveNewNode subDir
            >>= (fun newNode -> updateDir parent)
            |> AsyncResult.map (fun _ -> subDir)
        )

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
                   | false -> Error "Operation failed, please check log")
               )
        )

    /// 彻底删除文件夹中直接包含的文件
    let deleteAllFilesOf (dir: FileDirectory) =
        dir.FileChildren
        |> List.map (fun file -> fileSvc.DeleteFile (file.Id, dir.OwnerId))
        |> allTaskComplete

    /// 彻底递归删除指定的文件夹
    let rec ``rm -rf`` (dir: FileDirectory) =
        if dir.DirectoryChildren.IsEmpty
        then () |> Ok |> Async.fromValue
        else dir.DirectoryChildren
                |> List.map (flip getDir dir.OwnerId)
                |> List.map (AsyncResult.bind ``rm -rf``)
                |> allTaskComplete
        >>= just deleteAllFilesOf dir
        >>= just repo.DeleteDir dir

    /// 将一个文件夹从父文件夹中移除，并在后台递归删除其中的内容
    let deattachFromParent (dir: FileDirectory) =
        getDir dir.ParentId dir.OwnerId
        >>= (fun parent ->
            dir.RemoveFrom parent
            unitWork updateDir parent
            )
        |> AsyncResult.map (fun _ -> Rmrf dir.Id |>  bus.Publish |> Ok)

    /// 获取用户的根文件夹
    member this.GetRootDirectory userId =
        repo.GetRootDir userId
        |> Async.fromOption ( Error "Not initialized" )

    /// 获取指定的文件夹及其所有的父级文件夹
    member this.GetDirWithPath dirId userId =
        getDirPath dirId userId
        |> AsyncResult.map (List.rev)

    /// 获取指定的文件夹
    member this.GetDir (dirId, userId) =
        getDir dirId userId


    /// 创建一个文件夹
    member this.CreateDirectory (dto: DirectoryAddDto) (userId:string) =
        getDir dto.ParentId userId
        |> AsyncResult.bind (fun dir ->
            repo.ContainsName dto.Name dir
            |> Async.map (function
                | false -> Ok dir
                | true -> Error "Directory already exists"
                )
            )
        >>= createAsSubSir dto

    /// 创建一个根目录
    member this.CreateRootDir userId =
        repo.GetRootDir userId
        |> Async.map (function
            | Some _ -> Error "Root directory has been initialized"
            | None -> Ok userId
            )
        >>= createRootDir

    member this.Rename userId dirId newName =
        repo.GetDir dirId userId
        |> Async.bind (function
            | None -> "Directory not found" |> AsyncResult.returnError
            | Some dir ->
                if dir.IsRoot then "Can not rename root directory" |> AsyncResult.returnError
                elif dir.Name = newName then dir |> AsyncResult.retn
                else dir.Name <- newName
                     repo.UpdateNode dir
                     |> AsyncResult.map (fun _ -> dir)
            )

    /// 删除一个文件夹
    member this.DeleteDir userId dirId force =
        repo.GetDir dirId userId
        |> Async.map (function
            | None -> Error "Directory not found"
            | Some dir ->
                if dir.IsRoot then Error "Can not delete root directory"
                elif force || (dir.FileChildren.IsEmpty
                    && dir.DirectoryChildren.IsEmpty)
                then Ok dir
                else Error "Directory is not empty"
            )
        >>= deattachFromParent

    /// 递归删除指定文件夹，只用于在后台进程删除文件夹
    member this.RmRf dirId =
        ``rm -rf`` dirId

    /// 向文件夹中添加文件
    member this.AddFile (file: FileInfo) (dir: FileDirectory) =
        dir.AddFile file
        updateDir dir
        |> AsyncResult.map (fun  _ ->
                dir.ApplyOperations()
                file
            )

    /// 从文件夹中删除文件
    member this.DeleteFile (file: FileInfo) (dir: FileDirectory) =
        repo.ContainsName file.Name dir
        |> Async.map (function
            | true -> Error "File already exists"
            | false -> Ok ()
            )
        >>= (fun _ ->
                dir.RemoveFile file.Id
                updateDir dir
                >>= just fileSvc.DeleteFile (file.Id, file.OwnerId)
            )