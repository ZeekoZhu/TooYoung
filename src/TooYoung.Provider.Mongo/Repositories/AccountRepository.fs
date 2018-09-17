namespace TooYoung.Provider.Mongo.Repositories
open MongoDB.Driver
open TooYoung.Core.Models
open TooYoung.Core.Repository
open FSharp.Control.Tasks.V2

type AccountRepository(db: IMongoDatabase) =
    let users = db.GetCollection<User>("User")
    
    interface IAccountRepository with
        /// 根据用户登录名查找用户信息
        member this.FindByUserName (userName:string) =
            users.Find(fun u -> u.UserName = userName).FirstOrDefaultAsync()
        /// 插入一个新的用户信息
        member this.Create (user: User) =
            task {
                do! users.InsertOneAsync(user)
                return user
            }
