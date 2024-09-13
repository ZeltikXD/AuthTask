using AuthTask.Models;
using AuthTask.Shared;

namespace AuthTask.Interfaces
{
    public interface IUserRepository
    {
        Result<Guid> Create(User user);

        Result<IEnumerable<User>> GetUsers(int page, int size);

        Result<User?> GetUserByEmail(string email);

        Result ChangeActiveStatus(Guid userId, bool newStatus);

        Result Delete(Guid userId);

        Result<bool> NameExists(string name);

        Result<bool> EmailExists(string email);

        Result<bool> CheckPassword(string username, string password);

        Result SetLastLogin(Guid userId);

        Result<int> GetTotalUsers();
    }
}
