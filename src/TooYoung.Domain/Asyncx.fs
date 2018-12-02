namespace TooYoung

module Async =
    open FsToolkit.ErrorHandling
    let inline fromOption none x =
            x |> Async.map (function
                            | None -> none
                            | Some v -> Ok v)

    let fromValue = async.Return
    let combine x y =
        AsyncResult.bind
            ( fun xResult ->
                AsyncResult.map
                    ( fun yResult ->
                        (xResult, yResult)
                    )
                    y
            )
            x