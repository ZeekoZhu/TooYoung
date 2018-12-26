namespace TooYoung.Domain.Repositories
open System

[<AbstractClass>]
type UnitOfWork() =
    let mutable finished = false
    member this.Finished
        with get() = finished
        and private set(value) = finished <- value

    abstract member Commit: unit -> unit
    default this.Commit () =
        this.Finished <- true

    abstract member Rollback: unit -> unit
    default  this.Rollback() =
        this.Finished <- true

    abstract member Close: unit -> unit 
    default this.Close () =
        if this.Finished = false then this.Commit()

    interface IDisposable with 
        member this.Dispose() = this.Close()
            

module UnitOfWork =
    let startWork (uow: UnitOfWork) f =
        using uow
            ( fun work ->
                try f work
                finally work.Commit()
            )
    let commit (uow: UnitOfWork) =
        uow.Commit()
    let rollback (uow: UnitOfWork) =
        uow.Rollback()