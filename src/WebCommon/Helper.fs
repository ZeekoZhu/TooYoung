namespace WebCommon

module Helper =
    open System.Text.RegularExpressions
    open System.Linq

    let parseEnvParams str =
        let regex = Regex("""\$\((.+?)\)""")
        let matches = regex.Matches(str).Cast<Match>();
        matches.Select(fun x -> x.Groups.[1].Value)

