module TooYoung.Domain.JsonConverters
open System
open Newtonsoft.Json

type ToStringConverter() =
    inherit JsonConverter() with
        override __.CanConvert (objType: Type) =
            true
        override __.WriteJson(writer, value, serializer) =
            writer.WriteValue(value.ToString())
        override __.CanRead = false
        override __.ReadJson (_,_,_,_) =
            raise (NotImplementedException())