using AuthTask.Shared;

namespace AuthTask.Interfaces
{
    public interface IAuthManager
    {
        Result SignIn(string email, string password);

        void SignOut();

        bool IsSignedIn();
    }
}
