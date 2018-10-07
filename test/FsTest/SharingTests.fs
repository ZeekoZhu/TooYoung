module SharingTests

open System
open Xunit
open TooYoung.Domain
open Sharing
open FluentAssertions
open Xunit

type SharingTests() =
    
    [<Theory>]
    [<MemberData("PublicEntryTestData")>]
    member this.``Everyone can access public entry`` (claims: AccessClaim) =
        /// 一个不包含任何认证规则的共享入口就是完全公开的
        let entry = SharingEntry("2333", "owner", "file id")
        
        let result = claims |> canAccess entry
        result.Should().BeTrue(null, null)
    
    static member PublicEntryTestData() =
        let data = TheoryData<AccessClaim>()
        [ { Host = Some "2333"; Token = Some ("some token", DateTime.Now, "some password") }
          { Host = None; Token = Some ("some token", DateTime.Now, "") }
        ]
        |> List.iter data.Add
        data
    
    static member RefererHostTestData() =
        let data = TheoryData<AccessClaim, bool>()
        [ { Host = Some "2333"; Token = None }, false
          { Host = Some "gianthard.rocks"; Token = None }, true
          { Host = Some "img.gianthard.rocks"; Token = None }, true
          { Host = Some "2333.img.gianthard.rocks"; Token = None }, true
          { Host = None; Token = None }, false
          { Host = None; Token = Some ("2333", DateTime.Now, "password") }, false ]
        |> List.iter data.Add
        data
    
    [<Theory>]
    [<MemberData("RefererHostTestData")>]
    member this.``Referer host tests`` (claims: AccessClaim, expected: bool) =
        let entry = SharingEntry("2333", "owner", "file id")
        entry.RefererRules <- { AllowedHost = "(.+\.)*gianthard\.rocks"; Id = "" } :: entry.RefererRules
        let result = claims |> canAccess entry
        result.Should().Be(expected, null, null)
    
    static member TokenTestData() =
        let data = TheoryData<AccessClaim, bool>()
        [ { Host = Some "2333"; Token = None }, false
          { Host = Some "gianthard.rocks"; Token = None }, false
          { Host = Some "img.gianthard.rocks"
            Token = Some ("some token", DateTime.Now, "Some password") }, false
          { Host = None; Token = Some ("2333", DateTime(2018, 5, 1), "password") }, true
          { Host = None; Token = Some ("23333", DateTime(2018, 5, 1), "password") }, false
          { Host = None; Token = Some ("2333", DateTime(2018, 5, 1), "23333") }, false
          { Host = None; Token = None }, false
          { Host = None; Token = Some ("2333", DateTime(2019, 1, 1), "password") }, false ]
        |> List.iter data.Add
        data
    
    [<Theory>]
    [<MemberData("TokenTestData")>]
    member this.``Token tests`` (claims: AccessClaim, expected: bool) =
        let entry = SharingEntry("2333", "owner", "file id")
        
        let tokenRule: TokenRule =
            { Token = "2333"
              ExpiredAt = DateTime(2018, 6, 1, 12, 0, 0) |> Some
              Password = "password"
              Id = ""
            }
        entry.TokenRules <- tokenRule :: entry.TokenRules
        
        let result = claims |> canAccess entry
        result.Should().Be(expected, null, null)


    static member TokenNoExpiredTestData() =
            let data = TheoryData<AccessClaim, bool>()
            [ { Host = None; Token = Some ("2333", DateTime(2018, 5, 1), "password") }, true
              { Host = None; Token = Some ("23333", DateTime(2018, 5, 1), "password") }, false
              { Host = None; Token = Some ("2333", DateTime(2018, 5, 1), "23333") }, false
              { Host = None; Token = None }, false
              { Host = None; Token = Some ("2333", DateTime(2019, 1, 1), "password") }, true ]
            |> List.iter data.Add
            data

    [<Theory>]
    [<MemberData("TokenNoExpiredTestData")>]
    member this.``Token with no exprired date tests`` (claims: AccessClaim, expected: bool) =
        let entry = SharingEntry("2333", "owner", "file id")
                
        let tokenRule: TokenRule =
            { Token = "2333"
              ExpiredAt = None
              Password = "password"
              Id = ""
             }
        entry.TokenRules <- tokenRule :: entry.TokenRules
        
        let result = claims |> canAccess entry
        result.Should().Be(expected, null, null)
