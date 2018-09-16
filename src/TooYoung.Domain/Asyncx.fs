namespace TooYoung


module Asyncx =
    open Cvdm.ErrorHandling.Helpers
    open Cvdm.ErrorHandling

    /// map async
    let inline (<!>) x f =
        async {
            let! x = x
            return f x
        }

    /// Async Result map
    let inline (>=>) (x:Async<Result<_, _>>) f =
        AsyncResult.map f x

    /// Async Result bind on rail way
    let inline (=>>) (x:Async<Result<_, _>>) f =
        let fn = function
            | Error e -> e |> Error |> async.Return
            | Ok v -> f v
        async.Bind (x, fn)

    let inline bindResult a b = b =>> a

    let inline compose f2 f1 x=
        (f1 x) =>> f2
    /// combine two async operation
    let inline (>+>) f1 f2 x = compose f1 f2 x

module Async =
    open Asyncx
    let inline fromOption none x =
            x <!> function
                    | None -> none
                    | Some v -> Ok v
    
    let inline bind f x =
        async.Bind(x, f)

    let inline map f x =
        x <!> f

    let fromValue = async.Return