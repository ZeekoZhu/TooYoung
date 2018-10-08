// [Auth][CORS] GET url/{x: type}
// query x: type
// return
// | 200 -> type
// | 400 -> type
// description: xxxxx

(*
param type position description
x     int  url      xxxxx
*)

// POST url
// body x: type
// return
// | 200 -> type
// | 400 -> type
// description: xxxx

(*
## Title

**Auth**, **CORS**
----
GET url/{x: type}
----

.Params
[cols=1,1,1,3]
|===
|Param  |Type   |Position   |Description

|x      |int    |url        |some int value
|===
*)

module APIGenUtils

open System
open System.Collections
open System.Collections.Generic
open System.IO
open System.Reflection
open System.Text
open Microsoft.FSharp.Reflection

///Returns the case name of the object with union type 'ty.
let GetUnionCaseName (x:'a) = 
    match FSharpValue.GetUnionFields(x, typeof<'a>) with
    | case, _ -> case.Name  

type EntityType = {
    Name: string
    Member: (string * ModelType) list
    RelatedType: EntityType list
}
and GenericType =
    { Literal: string
      Nested: ModelType list
    }
and ModelType =
    | Value of string
    | Entity of EntityType
    | Generic of GenericType

let typeDict: System.Collections.Generic.Dictionary<Type, EntityType> = Dictionary()

/// Get all related types include itself
let rec getRelatedTypes (type_: ModelType) =
    match type_ with
    | Value _ -> []
    | Entity entity ->
        let relatedEnitty =
            entity.Member
            |> List.map snd
            |> List.choose (function | Entity e -> Some e | _ -> None)
        let relatedGeneric =
            entity.Member
            |> List.map snd
            |> List.choose (function | Generic e -> Some e | _ -> None)
            |> List.map (Generic >> getRelatedTypes)
            |> List.concat
        // printfn "related Generic %A" relatedGeneric
        entity :: relatedEnitty @ relatedGeneric
    | Generic g ->
        printfn "get related Generic %A" g
        g.Nested
        |> List.map getRelatedTypes
        |> List.concat
    |> List.distinct

let rec modelType (type_: Type) =
    if type_.IsGenericType then printfn "parse generic type %s" type_.Name; genericType type_ |> Generic
    else if type_.IsPrimitive || type_.Namespace.StartsWith("System") then type_.Name |> Value
    else printfn "parse entity type %s" type_.Name; entityType type_ |> Entity

and genericType (type_: Type) =
    if  (typeof<IDictionary>).IsAssignableFrom type_
    then {Literal = "dict"; Nested = type_.GenericTypeArguments |> Seq.map modelType |> List.ofSeq }
    else if (typeof<IEnumerable>).IsAssignableFrom type_ 
    then {Literal = "list"; Nested = type_.GenericTypeArguments |> Seq.map modelType |> List.ofSeq }
    else {Literal = type_.Name; Nested = type_.GenericTypeArguments |> Seq.map modelType |> List.ofSeq }

and entityType (type_: Type) =
    if typeDict.ContainsKey(type_) then typeDict.[type_]
    else
        let name = type_.Name
        let members =
            type_.GetProperties(BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.GetField)
            |> Seq.map (fun x -> x.Name, (modelType x.PropertyType))
            |> List.ofSeq

        let result =
            { Name = name; Member = members; RelatedType = [] }
        let relatedTypes = getRelatedTypes (Entity result) |> List.filter (fun x -> x <> result)
        let result = { result with RelatedType = relatedTypes }
        typeDict.Add(type_, result)
        result

let rec sprintTypeName refer = function
    | Value s -> s
    | Entity entity ->
        if refer then " <<" + entity.Name + ">> " else entity.Name
    | Generic generic ->
        let types =
            generic.Nested
            |> List.map (sprintTypeName refer)
        let rendered = String.Join(" * ", types)
        "(" + rendered + ") " + generic.Literal

let sprintType (entity: EntityType) =
        printfn "print type %s" entity.Name
        let sb = StringBuilder()
        sb.AppendLine("{") |> ignore
        entity.Member
        |> List.iter
            ( fun (name, type_) ->
                sb.AppendLine(sprintf "    %s: %s" (name) (sprintTypeName false type_))
                |> ignore
            )
        sb.Append("}").ToString()

let sprintTypeSection (type_: EntityType) =
    let sb = StringBuilder()
    sb.AppendLine("### "+ type_.Name)
        .AppendLine()
        .AppendLine("----")
        .AppendLine(sprintType type_)
        .AppendLine("----")
        .AppendLine()
        .AppendLine("#### Related Types")
        .AppendLine() |> ignore
    type_.RelatedType
    |> List.iter
        ( fun x ->
            sb.AppendLine(sprintf ". <<%s>>" x.Name) |> ignore
        )
    sb.AppendLine()
        .ToString()

type HttpMethod = GET | POST | PUT | DELETE

type Attribute = CORS | Auth

type ParamsPosition = Url | Query | Header | Body

type ReqParam = {
    Name: string
    Type: ModelType
    Position: ParamsPosition
    Description: string
}

type Response = {
    Code: int
    Type: ModelType option
    Description: string
}

type APISection = {
    Method: HttpMethod
    Attributes: Attribute list
    Url: string
    Title: string
    Description: string
    Params: ReqParam list
    Return: Response list
}

type ResourceSection =
    { Name: string
      Base: string
      Reqs: APISection list
    }

let resource name base_ reqs =
    { Name = name
      Base = base_
      Reqs = reqs
    }
    

let param name (type_: Type) position des: ReqParam =
    { Name = name
      Type = modelType type_
      Position = position
      Description = des
    }


let response code (type_: Type) des: Response =
    { Code = code
      Type = Option.ofObj (type_) |> Option.map modelType
      Description = des
    }

let request title attrs method url params_ resp des: APISection =
    { Title = title
      Attributes = attrs
      Method = method
      Url = url
      Params = params_
      Return = resp
      Description = des
    }

let sortParams (params_: ReqParam list) =
    let mapper = function
        | Header -> 0
        | Url -> 1
        | Query -> 2
        | Body -> 3
    params_
    |> List.sortBy (fun x -> x.Position |> mapper)

let sortResp (resp: Response list) =
    resp
    |> List.sortBy (fun x -> x.Code)

let sprintApi (section: APISection) =
    let normalized = 
        { section with
            Params = sortParams section.Params
            Return = sortResp section.Return
        }
    let sb = StringBuilder()
    sb.AppendLine(sprintf "### %s" section.Title)
        .AppendLine() |> ignore
    let rendered =
        section.Attributes
        |> List.map (fun x -> sprintf "**%s**" (GetUnionCaseName x))

    let attrs = String.Join(",", rendered)
    let section =
        section.Params
        |> List.choose (fun x -> match x.Position with Url -> Some x | _ -> None)
        |> List.fold
                ( fun (api: APISection) (x: ReqParam) ->
                    let pattern = sprintf "{%s}" x.Name
                    let replacement = sprintf "{%s: %s}" (x.Name) ( sprintTypeName true x.Type)
                    let newUrl = api.Url.Replace(pattern, replacement)
                    { api with Url = newUrl }
                )
                section
    sb.AppendLine(attrs)
        .AppendLine()
        .AppendLine("----")
        .AppendLine(sprintf "%s %s" (GetUnionCaseName section.Method) (section.Url))
        .AppendLine("----")
        .AppendLine()
        .AppendLine(".Params")
        .AppendLine("[%header,cols=\"1,^1,^1,3\"]")
        .AppendLine("|===")
        .AppendLine("|Name  |Type   |Position   |Description")
        .AppendLine()
    |> ignore
    section.Params
    |> List.iter
        (fun x ->
            sb.AppendLine(sprintf "|%s |%s |%s |%s" (x.Name) ( sprintTypeName true x.Type) (GetUnionCaseName x.Position) (x.Description))
            |> ignore
        )
    sb.AppendLine("|===")
        .AppendLine()
        .AppendLine(".Response")
        .AppendLine("[%header,cols=\"1,^1,3\"]")
        .AppendLine("|===")
        .AppendLine("|Status     |Type   |Description")
    |> ignore
    section.Return
    |> List.iter
        ( fun x ->
            sb.AppendLine( sprintf "|%s |%s |%s" (string x.Code) (match x.Type with Some t -> sprintTypeName true t | None -> "empty") (x.Description) )
            |> ignore 
        )
    sb.AppendLine("|===")
        .AppendLine()
        .AppendLine("**Description**")
        .AppendLine()
        .AppendLine(section.Description)
        .AppendLine()
        .ToString()

let sprintResource (section: ResourceSection) =
    let sb = StringBuilder()
    sb.AppendLine("## " + section.Name)
        .AppendLine()
    |> ignore
    section.Reqs
    |> List.iter
        ( fun x ->
            sb.AppendLine(sprintApi x) |> ignore
        )
    sb.ToString()


type ApiDocument =
    { Title: string
      Description: string
      Sections: ResourceSection list
    }

let document title des sections =
    { Title = title
      Description = des
      Sections = sections
    }

let generateDoc (doc: ApiDocument) output =
    let sb = new StringBuilder()
    sb.AppendLine("# " + doc.Title)
        .AppendLine(":toc:")
        .AppendLine(":table-caption!:")
        .AppendLine()
        .AppendLine(doc.Description)
        .AppendLine()
    |> ignore
    doc.Sections
    |> List.iter
        ( fun x ->
            sb.AppendLine(sprintResource x)
            |> ignore
        )
    sb.AppendLine()
        .AppendLine("## Model Types")
        .AppendLine()
        |> ignore

    typeDict
    |> Seq.iter (fun (KeyValue(_, v)) ->
            sb.AppendLine()
                .AppendLine(sprintTypeSection v)
            |> ignore
        )
        
    System.IO.File.WriteAllText(output, sb.ToString())