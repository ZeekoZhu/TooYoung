module Utils

open TooYoung

let inline asyncNone () = None |> Async.fromValue
module Result =
    let inline ofSome onNone =
        function
        | Some v -> Ok v
        | None -> onNone()