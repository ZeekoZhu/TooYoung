#load "../src/TooYoung.Domain/Domain.fs"
#load "./api-gen-utils.fsx"
open APIGenUtils
open TooYoung.Domain
open TooYoung.Domain.Resource
open TooYoung.Domain.Sharing

let testDoc =
    document
        "Test"
        "A test"
        [ resource "TestItem" "/api/test"
            [ request "Get All"
                        [Auth; CORS]
                        GET ""
                        []
                        [ response 200 typeof<SharingEntry list> "list"
                        ]
                        "Get all test item"
              request "Get test"
                        [Auth; CORS]
                        GET "/{test}/{id}"
                        [ param "test" typeof<string> Header "233333"
                          param "test" typeof<string> Url "test in url"
                          param "id" typeof<int> Url "some id"
                          param "filter" typeof<string> Query "some id"
                        ]
                        [ response 200 typeof<string> "23333"
                        ]
                        "2333"
              request "Post test"
                        [Auth]
                        POST "/{id}"
                        [ param "test" typeof<string> Header "233333"
                          param "id" typeof<string> Url "233333"
                          param "content" typeof<FileInfo> Body "233333"
                        ]
                        [ response 200 typeof<string> "23333"
                          response 400 null "23333"
                        ]
                        "2333"
            ]
        ]

generateDoc testDoc "test.adoc"
