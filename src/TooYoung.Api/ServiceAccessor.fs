module TooYoung.Api.ServiceAccessor

open Giraffe
open Microsoft.AspNetCore.Http
open TooYoung.App
open TooYoung.Domain.Repositories
open TooYoung.Domain.Services


let getAccountRepo (ctx: HttpContext) = ctx.GetService<IAccountRepository>()
let getAccountSvc (ctx: HttpContext) = ctx.GetService<AccountAppService>()
let getAuthService (ctx: HttpContext) = ctx.GetService<AuthorizationService>()
let getDirService (ctx: HttpContext) = ctx.GetService<DirectoryService>()
let getFileSvc (ctx: HttpContext) = ctx.GetService<FileService>()
let getDirSvc (ctx: HttpContext) = ctx.GetService<DirectoryService>()
let getFileAppSvc (ctx: HttpContext) = ctx.GetService<FileAppService>()