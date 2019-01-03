module TooYoung.Provider.Mongo.MongoHelper
open System.Collections.Generic
open MongoDB.Driver.Linq
open System.Linq
open Microsoft.FSharp.Quotations



let toMongoQuery (query: IQueryable<'t>) =
    query :?> IMongoQueryable<'t>
    
module MongoQueryCE =
    open System
    open MongoDB.Driver.Linq
    open System.Linq.Expressions
    open Microsoft.FSharp.Quotations
    open Microsoft.FSharp.Linq.RuntimeHelpers
    


    let toLinq (expr : Expr<'a -> 'b>) =
      let linq = LeafExpressionConverter.QuotationToExpression expr
      let call = linq :?> MethodCallExpression
      let lambda = call.Arguments.[0] :?> LambdaExpression
      Expression.Lambda<Func<'a, 'b>>(lambda.Body, lambda.Parameters) 

    type MongoQueryBuilder() =
        member __.For (source:IMongoQueryable<'T>, body: 'T -> IEnumerable<'T2>) :IMongoQueryable<'T2> =
            source.SelectMany(body)
//        member __.For (source: IMongoQueryable<'T>, body: 'T -> IMongoQueryable<'T2>) =
//            source.SelectMany(body)
        member __.YieldFrom x = x
        member __.Zero<'T> () : IMongoQueryable<'T> =
            null

        [<CustomOperation("groupJoin", IsLikeGroupJoin = true, JoinConditionWord = "on")>]
        member __.GroupJoin (outerSource: IMongoQueryable<'TO>, innerSource:IMongoQueryable<'TI>, outerKeySelector, innerKeySelector, resultSelector) =
            MongoQueryable.GroupJoin(outerSource, innerSource, outerKeySelector, innerKeySelector, resultSelector)
            
        [<CustomOperation("where", AllowIntoPattern = true, MaintainsVariableSpace = true)>]
        member __.Where (source: IMongoQueryable<'T>, predicate: 'T -> bool) =
            source.Where(predicate)
            
        [<CustomOperation("select", AllowIntoPattern = true)>]
        member __.Select (source: IMongoQueryable<'T>, projection) =
            MongoQueryable.Select(source, projection)

let mongoQuery = MongoQueryCE.MongoQueryBuilder()

