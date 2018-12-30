namespace TooYoung.Provider.Mongo.Repositories
open System
open MongoDB.Driver
open FSharp.Control.Tasks.V2
open TooYoung.Domain.User
open TooYoung.Domain.Repositories
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open TooYoung
open TooYoung.Provider.Mongo
open Utils

type AccountRepository(db: IMongoDatabase) =
    let users = db.GetCollection<User>("User")
    
    let findByUserName (userName:string) =
        users.Find(fun u -> u.UserName = userName).FirstOrDefaultAsync()
        |> Async.AwaitTask
        |> Async.map Option.ofObj

    let findByUserId (userId: Guid) =
        users.Find(fun u -> u.Id = userId).FirstOrDefaultAsync()
        |> Async.AwaitTask
        |> Async.map Option.ofObj

    let create user =
        task {
            do! users.InsertOneAsync(user)
            return user
        }
        |> Async.AwaitTask
        |> Async.map Ok

    let shouldExists findBy p errorMsg =
        findBy p
        |> Async.map (Result.ofSome (fun _ -> Error errorMsg))
    
    let shouldNotExists findBy p errorMsg =
        findBy p
        |> Async.map (Result.ofNone (fun _ -> Error errorMsg))
    
    let getAllUsers () =
        users.AsQueryable().ToListAsync()
        |> Async.AwaitTask
        |> Async.map List.ofSeq
    
    let deleteUser (user: User) =
        users.DeleteOneAsync((fun u -> u.Id = user.Id))
        |> Async.AwaitTask
        |> Async.map (fun _ -> Ok user)

    interface IAccountRepository with
        member this.BeginTransaction() =
            new MongoUnitOfWork(db.Client) :> UnitOfWork
        /// 根据用户登录名查找用户信息
        member this.FindByUserName u = findByUserName u
        member this.FindByIdAsync u = findByUserId u
        member this.GetAllUsers () = getAllUsers()
        member this.DeleteUser user = deleteUser user
        /// 插入一个新的用户信息
        member this.Create (user: User) =
            create user
        /// 更新用户信息
        member this.UpdateAsync (user: User) =
            users.ReplaceOneAsync((fun u -> u.Id = user.Id), user)
            |> Async.AwaitTask
            |> Async.map (fun _ -> Ok user)
