module Utils

open TooYoung

/// 简化的错误处理方法
let inline onError errorMsg fn x =
    try fn x with e ->
        match errorMsg with
        | None -> Error e.Message |> Async.fromValue
        | Some err -> Error err |> Async.fromValue

