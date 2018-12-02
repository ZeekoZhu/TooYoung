module Utils

open TooYoung

let inline asyncNone () = None |> Async.fromValue
