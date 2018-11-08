namespace  TooYoung.Provider.Mongo

module BsonHelper =
    open FSharpPlus
    open MongoDB.Bson
    open MongoDB.Bson

    let bVal value = BsonValue.Create(value)
    let bArray values =
        values
        |> Seq.map BsonValue.Create
        |> BsonArray
    let bson pairs =
        pairs
        |> Seq.map (fun (name, value) -> BsonElement(name, value) )
        |> BsonDocument

module test =
    open BsonHelper
    let bsonObj =
        bson [ "$match",
          bson [ "$or",
                 bson [ "scroe", bson [ "$gt", bVal 70; "$lt", bVal 100]
                        "views", bson [ "$gte", bVal 1000]
                      ]
               ]
        ]