namespace TooYoung

module Taskx = FSharpx.Task

module TaskxAlias =
    open System.Threading.Tasks;
    let inline (>>=) m f = Taskx.bind f m
    let inline (<!>) x f = Taskx.map f x

    /// Taskx.map on rail way
    let inline (>=>) (x:Task<Result<_, _>>) f =
        x <!> (function
            | Error e -> Error e
            | Ok x -> f x
            )

    /// Taskx.bind on rail way
    let inline (=>>) (x:Task<Result<_, _>>) f =
        x >>= (function
            | Error e -> Error e |> Task.FromResult
            | Ok x -> f x
            )

    /// combine two async operation
    let inline (>+>) f1 f2 x =
        (f1 x) =>> f2