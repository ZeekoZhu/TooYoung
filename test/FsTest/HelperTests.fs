module HelperTests
open WebCommon
open FluentAssertions
open FsUnit
open Xunit

type HelperTests() =
    [<Fact>]
    member this.``Test Env Parser`` () =
        let connStr = "mongodb://$(MONGO_USER):$(MONGO_PWD)@$(MONGO_HOST):$(MONGO_PORT)"
        let result = Helper.parseEnvParams connStr |> List.ofSeq
        result |> should haveLength 4
        result |> should contain "MONGO_USER"
        result |> should contain "MONGO_PWD"
        result |> should contain "MONGO_HOST"
        result |> should contain "MONGO_PORT"

