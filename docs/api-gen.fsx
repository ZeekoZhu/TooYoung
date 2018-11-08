#load "env.fsx"
#r "netstandard"
#r "../src/TooYoung.Domain/bin/Debug/netstandard2.0/TooYoung.Domain.dll"
#load "./api-gen-utils.fsx"
#load "../src/WebCommon/Library.fs"
open APIGenUtils
open TooYoung.Domain
open TooYoung.Domain.FileDirectory
open TooYoung.Domain.Resource
open TooYoung.Domain.Sharing
open TooYoung.WebCommon.ErrorMessage
open TooYoung.Domain.Services

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
                        [ response 200 typeof<FileDirectory> "Root directory for current user"
                          generalError 404 "Can not find root directory"
                        ]
                        "Get the root directory of current user"
              request "Get directory info by id for current user"
                        [Auth]
                        GET "/{dirId}"
                        [ param "dirId" typeof<string> Url "Directory Id"
                        ]
                        [ response 200 typeof<FileDirectory> "Directory info"
                          generalError 404 "Directory not found"
                        ]
                        "Get directory info by id for current user"
              request "Get directory info with path by id for current user"
                        [Auth]
                        GET "/{dirId}/path"
                        [ param "dirId" typeof<string> Url "Directory Id"
                        ]
                        [ response 200 typeof<FileDirectory list> "A list of direcotries"
                          generalError 404 "Directory not found"
                        ]
                        "Get directory info with path by id for current user"
              request "Create new directory"
                        [Auth]
                        GET ""
                        [ param "dto" typeof<DirectoryAddDto> Body "New directory info"
                        ]
                        [ response 200 typeof<FileDirectory> "New directory info after added"
                          generalError 400 "Some thing wrong with request body"
                        ]
                        "Create a new sub directory"
            ]
        ]

generateDoc testDoc "api.adoc"
