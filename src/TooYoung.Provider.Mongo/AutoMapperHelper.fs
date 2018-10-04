namespace TooYoung.Provider.Mongo

module MappingConfigBuilder =
    open AutoMapper
    open AutoMapper.Configuration
    open FSharp.Quotations
    open FSharp.Linq.RuntimeHelpers
    
    open AutoMapper
    
    open System
    open System.Linq.Expressions
    type MappingProfile() =
        inherit Profile()
    
    
    let inline memberAccess<'instance, 'field> (expr: Expr<'field>) =
        expr
        |> LeafExpressionConverter.QuotationToExpression
        |> unbox<MemberExpression>
    
    let inline memberExpr<'instance, 'field> (expr: Expr<'field>) =
        let access = memberAccess expr
        let xExpr = Expression.Parameter (access.Expression.Type, "x")
        let memberExpr = Expression.MakeMemberAccess(xExpr, access.Member)
        Expression.Lambda<Func<'instance, 'field>>(memberExpr, xExpr)
    
    type MappingWrapper<'src, 'target> =
        | ProfileDef of Profile
        | MappingDef of Profile * IMappingExpression<'src, 'target>
    let mapping fn = function
        | ProfileDef p -> ProfileDef p
        | MappingDef (p, m) -> fn m |> ignore; MappingDef (p, m)
    type MapperConfigBuilder() =
        let getProfile = function
        | ProfileDef profile -> profile
        | MappingDef (p, _) -> p
        member this.Yield<'src, 'target> (x: MappingWrapper<'src, 'target>) = x
    
        [<CustomOperation("create")>]
        member this.Create
            ( wrapper: MappingWrapper<'src, 'target>
            ) =
            let profile = getProfile wrapper
            profile
            
        [<CustomOperation("map")>]
        member this.Map
            ( wrapper: MappingWrapper<'src, 'target>,
              [<ReflectedDefinition>] srcField: Expr<'sourceMember>,
              [<ReflectedDefinition>] targetField: Expr<'targetMember>
            ) =
                wrapper
                |> mapping (fun map ->
                    let mapFrom = memberExpr<'src, 'sourceMember> srcField
                    let mapTo = memberExpr<'target, 'targetMember> targetField
                    map.ForMember(mapTo, (fun x -> x.MapFrom(mapFrom)))
                    )
    
        [<CustomOperation("notMap")>]
        member this.Ignore
            ( wrapper: MappingWrapper<'src, 'target>
            , [<ReflectedDefinition>] field: Expr<'field>
            ) =
                wrapper
                |> mapping (fun map ->
                    let fieldSelector = memberExpr<'target, 'field> field
                    map.ForMember(fieldSelector, (fun opt -> opt.Ignore()))
                    )
    
        [<CustomOperation("useValue")>]
        member this.Value
            ( wrapper: MappingWrapper<'src, 'target>
            , [<ReflectedDefinition>] field: Expr<'field>
            , value: 'field
            ) =
                wrapper
                |> mapping (fun map ->
                    let fieldSelector = memberExpr<'target, 'field> field
                    map.ForMember(fieldSelector, (fun opt -> opt.UseValue(value)))
                    )
                    
        [<CustomOperation("resolve")>]
        member this.Resolve
            ( wrapper: MappingWrapper<'src, 'target>
            , [<ReflectedDefinition>] field: Expr<'field>
            , fn: 'src -> 'target -> 'field
            ) =
                wrapper
                |> mapping (fun map ->
                    let fieldSelector = memberExpr<'target, 'field> field
                    map.ForMember(fieldSelector, (fun opt -> opt.ResolveUsing<'field>(fn)))
                    )
    
        [<CustomOperation("ignoreRest")>]
        member this.IgnoreRest (wrapper: MappingWrapper<'src, 'target>) =
            wrapper
            |> mapping (fun map ->
                    map.ForAllOtherMembers(fun opt -> opt.Ignore())
                )
    
        [<CustomOperation("swap")>]
        member this.Reverse (wrapper: MappingWrapper<'src, 'target>) =
            wrapper
            |> (function
                | ProfileDef x -> ProfileDef x
                | MappingDef (p, m) -> MappingDef (p, p.CreateMap<'target, 'src>())
            )
        
        [<CustomOperation("reverseMap")>]
        member this.ReverseMap (wrapper: MappingWrapper<'src, 'target>) =
            wrapper
            |> mapping (fun map ->
                map.ReverseMap()
            )
    
        [<CustomOperation("mapping")>]
        member x.CreateMapping
            ( wrapper: MappingWrapper<'src, 'target>
            , a: 'src
            , b: 'target
            ) =
                let profile = getProfile wrapper
                MappingDef (profile, profile.CreateMap<'src, 'target>())
    
        member this.Yield (x: unit) =
            MappingProfile() :> Profile |> ProfileDef
                        
        member this.Zero () =
            MappingProfile() :> Profile |> ProfileDef
    

module AutoMapperBuilder =
    open MappingConfigBuilder
    let prepare<'t> = Unchecked.defaultof<'t>
    let automapper = MapperConfigBuilder()
