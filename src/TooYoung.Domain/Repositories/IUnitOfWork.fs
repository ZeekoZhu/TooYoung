namespace TooYoung.Domain.Repositories
open System.Threading.Tasks
open System

[<AbstractClass>]
type UnitOfWork() =
    let mutable finished = false
    member this.Finished
        with get() = finished
        and private set(value) = finished <- value
    member this.Commit () =
        this.Finished <- true
    member this.Rollback() =
        this.Finished <- true

    interface IDisposable with 
        member this.Dispose() = 
            if this.Finished = false then this.Commit()

module UnitOfWork =
    let commit (uow: UnitOfWork) =
        uow.Commit()
    let rollback (uow: UnitOfWork) =
        uow.Rollback()