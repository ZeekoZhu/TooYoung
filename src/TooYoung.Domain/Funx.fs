namespace TooYoung

module FunxAlias =
    open FSharpPlus

    let flip = Operators.flip
    let inline just fn p _ = 
        fn p
    let inline switchTo x =
        fun _ -> x
    let inline tap fn p value =
        fn p |> ignore
        value