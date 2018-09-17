namespace TooYoung.Provider.Mongo
open MongoDB.Driver

open System
open MongoDB.Driver
open TooYoung.Domain.Repositories

type MongoUnitOfWork(client: IMongoClient) =
    inherit UnitOfWork()
    let session = client.StartSession(ClientSessionOptions(CausalConsistency = Nullable(true)))
    do session.StartTransaction()

    override this.Commit() =
        if session.IsInTransaction
        then session.CommitTransaction()
        base.Commit()

    override this.Rollback() =
        if session.IsInTransaction
        then session.AbortTransaction()
        base.Rollback()

    override this.Close() =
        base.Close()
        session.Dispose()