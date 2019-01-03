namespace WebCommon

module Helper =
    open System.Text.RegularExpressions
    open System.Linq

    let parseEnvParams str =
        let regex = Regex("""\$\((.+?)\)""")
        let matches = regex.Matches(str).Cast<Match>();
        matches.Select(fun x -> x.Groups.[1].Value)

module Impure =

    type CsList<'t> = System.Collections.Generic.List<'t>
    let ofCsList csList =
        List.ofSeq csList
    let toCsList<'t> (someList: seq<'t>) =
        CsList(someList)
    let mapToCsList mapper =
        Seq.map mapper >> toCsList
    let mapFromCsList mapper =
        ofCsList >> List.map mapper