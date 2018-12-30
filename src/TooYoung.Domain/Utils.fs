module Utils

open TooYoung
open FsToolkit.ErrorHandling

let inline asyncNone () = None |> Async.fromValue

module Result =
    let inline ofNone onSome =
        function
        | Some _ -> onSome()
        | None -> Ok ()
    let inline ofSome onNone =
        function
        | Some v -> Ok v
        | None -> onNone()

module AsyncResult =
    let inline ofSome onNone =
        Async.map (Result.ofSome onNone)
    let inline ofNone onSome =
        Async.map (Result.ofNone onSome)
