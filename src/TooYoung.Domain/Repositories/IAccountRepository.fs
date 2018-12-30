namespace TooYoung.Domain.Repositories

open System
open TooYoung
open TooYoung.Domain.User

type IAccountRepository =
    inherit IRepository
    abstract Create: user:User -> Async<Result<User, AppError>>
    abstract FindByUserName: username:string -> Async<User option>
    abstract FindByIdAsync: userId:Guid -> Async<User option>
    abstract UpdateAsync: user:User -> Async<Result<User, AppError>>
    abstract GetAllUsers: unit -> Async<User list>
    abstract DeleteUser: user: User -> Async<Result<User, AppError>>
