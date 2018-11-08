namespace TooYoung.Provider.Mongo.Repositories
open MongoDB.Driver
open FSharp.Control.Tasks.V2
open TooYoung.Domain.User
open TooYoung.Domain.Repositories
open FsToolkit.ErrorHandling
open TooYoung
open TooYoung.Provider.Mongo

type AccountRepository(db: IMongoDatabase) =
    let users = db.GetCollection<User>("User")
    let create user =
        task {
            do! users.InsertOneAsync(user)
            return user
        }
        |> Async.AwaitTask
        |> Async.map Ok
    interface IAccountRepository with
        member this.BeginTransaction() =
            new MongoUnitOfWork(db.Client) :> UnitOfWork
        /// 根据用户登录名查找用户信息
        member this.FindByUserName (userName:string) =
            users.Find(fun u -> u.UserName = userName).FirstOrDefaultAsync()
            |> Async.AwaitTask
            |> Async.map Option.ofObj
        /// 插入一个新的用户信息
        member this.Create (user: User) =
            try create user
            with e -> e.Message |> Error |> Async.fromValue