[<AutoOpen>]
module AppErrors

exception InvalidState of string

type AppError =
/// 400
| Validation of string
/// 403
| Forbidden of string
/// 401
| Unauthorized of string
/// Multiple 500
| Multiple of string
