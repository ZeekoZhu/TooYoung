module TooYoung.Api.Handlers.AuthGuard

open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Http
open Giraffe

let authenticationScheme = "Cookies"
let notLoggedIn =
    RequestErrors.UNAUTHORIZED authenticationScheme "too young" "You must sign in first"

let requireLogin f = requiresAuthentication notLoggedIn f




