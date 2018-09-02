namespace TooYoung.Domain
open System.Reactive.Concurrency
open FSharp.Control.Reactive
open System.Reactive.Subjects
open TooYoung.Domain.FileDirectory

type DirId = string
type FileIds = string list

type SystemEvent =
    | Rmrf of DirId
    | RmFiles of FileIds
    | CreateUser of string

type EventBus() =
    let subject: Subject<SystemEvent> = Subject.broadcast

    member val Events = Observable.subscribeOn NewThreadScheduler.Default subject

    member this.Publish event =
        subject.OnNext(event)

