namespace TooYoung.Domain.Repositories
open TooYoung.Domain.User

type IAccountRepository =
    inherit IRepository
    abstract member Create: user:User -> Async<Result<User, string>>
    abstract member FindByUserName: username:string -> Async<User option> 

