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

type DirectoryService
    ( repo: IDirectoryRepository, bus: EventBus
    ) =
    let startWork = Repository.startWork repo
    let unitWork = Repository.unitWork repo

    let getDir dirId userId =
        repo.GetDir dirId userId
        |> Async.fromOption (Error <| Validation "Directory not found")
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


    /// 将一个文件夹从父文件夹中移除，并在后台递归删除其中的内容
    let deattachFromParent (dir: FileDirectory) =
        getDir dir.ParentId dir.OwnerId
        >>= (fun parent ->
                dir.RemoveFrom parent
                updateDir parent
            )
        |> Async.map (fun _ -> Rmrf dir |>  bus.Publish |> Ok)

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
                | true -> Error <| Validation "Directory already exists"
                )
            )
        >>= createAsSubSir dto

    /// 创建一个根目录
    member this.CreateRootDir userId =
        repo.GetRootDir userId
        |> Async.map (function
            | Some _ -> Error (Validation "Root directory has been initialized")
            | None -> Ok userId
            )
        >>= createRootDir

    member this.Rename userId dirId newName =
        repo.GetDir dirId userId
        |> Async.bind (function
            | None -> Validation "Directory not found" |> AsyncResult.returnError
            | Some dir ->
                if dir.IsRoot then Validation "Can not rename root directory" |> AsyncResult.returnError
                elif dir.Name = newName then dir |> AsyncResult.retn
                else dir.Name <- newName
                     repo.UpdateNode dir
                     |> AsyncResult.map (fun _ -> dir)
            )

    /// 删除一个文件夹，并发布删除文件夹的领域事件
    member this.DeleteDir userId dirId force =
        repo.GetDir dirId userId
        |> Async.map (function
            | None -> Error <| Validation "Directory not found"
            | Some dir ->
                if dir.IsRoot then Error <| Validation "Can not delete root directory"
                elif force || (dir.FileChildren.IsEmpty
                    && dir.DirectoryChildren.IsEmpty)
                then Ok dir
                else Error <| Validation "Directory is not empty"
            )
        >>= deattachFromParent

    /// 向文件夹中添加文件
    member this.AddFile (file: FileInfo) (dir: FileDirectory) =
        let checkFileName (file: FileInfo) dir =
            repo.ContainsFileName file.Name dir
            |> Async.map
                ( function
                | true -> "File name has been taken" |> Validation |> Error
                | false -> Ok file
                )
        let updateDir () =
            dir.AddFile file
            updateDir dir
            |> AsyncResult.map (fun  _ ->
                    dir.ApplyOperations()
                    file
                )
        checkFileName file dir
        >>= just updateDir ()

    /// 从文件夹中删除文件
    member this.DeleteFile (file: FileInfo) (dir: FileDirectory) =
        dir.RemoveFile file.Id
        updateDir dir
