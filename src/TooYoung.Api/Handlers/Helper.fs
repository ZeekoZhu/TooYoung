module TooYoung.Api.Handlers.Helper

open Giraffe

let combineFunc<'a, 'b, 'c>
    (func1: ('a -> 'c) -> 'c)
    (func2: ('b -> 'c) -> 'c)
    (func3: ('a -> 'b -> 'c)) =
    func1 (func3 >> func2)

type ParamedHandler<'t> = ('t -> HttpHandler) -> HttpHandler

let combineParam<'t1, 't2> (ph1: ParamedHandler<'t1>) (ph2: ParamedHandler<'t2>) (fn: 't1 -> 't2 -> HttpHandler) =
    ph1 (fn >> ph2)
