using System.Threading.Tasks;
using TooYoung.Core.Models;

namespace TooYoung.Core.Repository
{
    public interface IAccountRepository
    {

        Task<User> Create(User user);
        Task<User> FindByUserName(string username);
    }
}