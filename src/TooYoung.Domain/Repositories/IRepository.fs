namespace  TooYoung.Domain.Repositories

type IRepository =
    abstract member BeginTransaction: unit -> UnitOfWork

module Repository =
    open UnitOfWork
    /// 开始一个单元任务，回调函数结束时自动提交，遇到异常自动回滚
    let startWork (repo: IRepository) f =
        using (repo.BeginTransaction())
              (fun work ->
                try f work finally rollback work)
    /// 将一个函数单元化，函数执行前开启事务，函数执行完成后自动提交事务，遇到异常自动回滚
    let unitWork (repo: IRepository) f p =
        startWork repo (fun _ -> f p )