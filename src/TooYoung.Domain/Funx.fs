namespace TooYoung

module FunxAlias =
    open FSharpx.Functional

    let flip = Prelude.flip
    let inline just fn p _ = 
        fn p
    let inline tap fn p value =
        fn p |> ignore
        value