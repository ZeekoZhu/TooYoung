module TooYoung.Domain.JsonConverters
open System
open Newtonsoft.Json
open Microsoft.FSharp.Reflection


type ToStringConverter() =
    inherit JsonConverter() with
        override __.CanConvert (objType: Type) =
            true
        override __.WriteJson(writer, value, serializer) =
            writer.WriteValue(value.ToString())
        override __.CanRead = false
        override __.ReadJson (_,_,_,_) =
            raise (NotImplementedException())

type OptionConverter() =
    inherit JsonConverter()

    override x.CanConvert(t) = 
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>

    override x.WriteJson(writer, value, serializer) =
        let value = 
            if value = null then null
            else 
                let _,fields = FSharpValue.GetUnionFields(value, value.GetType())
                fields.[0]
        serializer.Serialize(writer, value)

    override x.ReadJson(reader, t, existingValue, serializer) =        
        let innerType = t.GetGenericArguments().[0]
        let innerType = 
            if innerType.IsValueType then (typedefof<Nullable<_>>).MakeGenericType([|innerType|])
            else innerType        
        let value = serializer.Deserialize(reader, innerType)
        let cases = FSharpType.GetUnionCases(t)
        if value = null then FSharpValue.MakeUnion(cases.[0], [||])
        else FSharpValue.MakeUnion(cases.[1], [|value|])
