#load "env.fsx"
#r "Facades/netstandard"
#load "../src/TooYoung.Domain/Domain.fs"
#load "./api-gen-utils.fsx"
#load "../src/WebCommon/Library.fs"
open APIGenUtils
open TooYoung.Domain
open TooYoung.Domain.FileDirectory
open TooYoung.Domain.Resource
open TooYoung.Domain.Sharing
open TooYoung.WebCommon.ErrorMessage

let generalError code =
    response code typeof<ErrorMessage>

let testDoc =
    document
        "Too young" "/api"
        "Web api for too young"
        [ resource "Directory" "/dir"
            [ request "Get root directory"
                        [Auth]
                        GET "/root"
                        []
                        [ response 200 typeof<FileDirectory> "Get root directory for current user"
                          generalError 404 "Can not find root directory"
                        ]
                        "Get the root directory of current user"
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
