using MongoDB.Driver;

namespace TooYoung.Provider.MongoDB
{
    public class UnitOfWork : TooYoung.Domain.Repositories.UnitOfWork
    {
        private IClientSessionHandle _session;

        public UnitOfWork(IMongoClient client)
        {
            _session = client.StartSession(new ClientSessionOptions
            {
                CausalConsistency = true
            });
        }

        public override void Dispose()
        {
            _session?.Dispose();
            base.Dispose();
        }
    }
}