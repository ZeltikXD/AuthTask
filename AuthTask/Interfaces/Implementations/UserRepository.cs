using AuthTask.Data;
using AuthTask.Models;
using AuthTask.Shared;

namespace AuthTask.Interfaces.Implementations
{
    public class UserRepository(AuthDbContext context, ILogger<UserRepository> logger) : IUserRepository
    {
        Result IUserRepository.ChangeActiveStatus(Guid userId, bool newStatus)
        {
            try
            {
                var current = GetUserById(userId);
                if (current is null) return Result.Failure("The user doesn't exist.", StatusCodes.Status404NotFound);
                current.IsActive = newStatus;
                context.User.Update(current);
                int rows = context.SaveChanges();
                return rows > 0 ? Result.Success()
                    : Result.Failure("Changes could not be saved. User status could not be changed.", StatusCodes.Status422UnprocessableEntity);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating user status.");
                return Result.Failure("An error occurred while updating user status.", StatusCodes.Status500InternalServerError);
            }
        }

        Result<Guid> IUserRepository.Create(User user)
        {
            try
            {
                var (password, salt) = CryptoPassword.HashPassword(user.Password);
                user.Password = password;
                user.Salt = salt;
                context.User.Add(user);
                context.SaveChanges();
                return Result.Success(user.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while registering a user.");
                return Result.Failure<Guid>("An error occurred while registering a user.", StatusCodes.Status500InternalServerError);
            }
        }

        Result IUserRepository.Delete(Guid userId)
        {
            try
            {
                var current = GetUserById(userId);
                if (current is null) return Result.Failure("The user doesn't exist.", StatusCodes.Status404NotFound);
                context.User.Remove(current);
                int rows = context.SaveChanges();
                return rows > 0 ? Result.Success()
                    : Result.Failure("Changes could not be saved. The user could not be deleted.", StatusCodes.Status422UnprocessableEntity);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while removing a user from the database.");
                return Result.Failure("An error occurred while removing a user from the database.", StatusCodes.Status500InternalServerError);
            }
        }

        Result<User?> IUserRepository.GetUserByEmail(string email)
            => GetUserByEmail(email);

        Result<IEnumerable<User>> IUserRepository.GetUsers(int page, int size)
        {
            try
            {
                var users = context.User.OrderByDescending(x => x.RegistrationTime).Skip((page - 1) * size).Take(size).ToList();
                return Result.Success(users as IEnumerable<User>);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while searching users.");
                return Result.Failure<IEnumerable<User>>("An error occurred while searching users.", StatusCodes.Status500InternalServerError);
            }
        }

        Result<bool> IUserRepository.NameExists(string name)
        {
            try
            {
                var result = context.User.Any(x => x.Name.ToLower() == name.ToLower());
                return Result.Success(result);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "An error occurred while comparing names.");
                return Result.Failure<bool>("An error occurred while comparing names.", StatusCodes.Status500InternalServerError);
            }
        }

        Result<bool> IUserRepository.EmailExists(string email)
        {
            try
            {
                var result = context.User.Any(x => x.Email.ToLower() == email.ToLower());
                return Result.Success(result);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "An error occurred while comparing emails.");
                return Result.Failure<bool>("An error occurred while comparing emails.", StatusCodes.Status500InternalServerError);
            }
        }

        private Result<User?> GetUserByEmail(string email)
        {
            try
            {
                var user = context.User.FirstOrDefault(x => x.Email.ToLower() == email.ToLower());
                return Result.Success(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while searching for a user by name.");
                return Result.Failure<User?>("An error occurred while searching for a user by name.", StatusCodes.Status500InternalServerError);
            }
        }

        private User? GetUserById(Guid id)
            => context.User.Find(id);

        Result<bool> IUserRepository.CheckPassword(string email, string password)
        {
            try
            {
                var currentRes = GetUserByEmail(email);
                if (currentRes.IsFailure) return Result.Failure<bool>(currentRes.Message, currentRes.StatusCode);
                if (currentRes.Value is null) return Result.Failure<bool>("The user doesn't exist.", StatusCodes.Status404NotFound);
                var result = CryptoPassword.CheckHash(password, currentRes.Value.Password, currentRes.Value.Salt);
                return Result.Success(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while comparing information.");
                return Result.Failure<bool>("An error occurred while comparing information.", StatusCodes.Status500InternalServerError);
            }
        }

        Result IUserRepository.SetLastLogin(Guid userId)
        {
            try
            {
                var current = GetUserById(userId);
                if (current is null) return Result.Failure("The user doesn't exist.", StatusCodes.Status404NotFound);
                current.LastLoginTime = DateTimeOffset.UtcNow;
                context.User.Update(current);
                int rows = context.SaveChanges();
                return rows > 0 ? Result.Success()
                    : Result.Failure("The user could not be processed.", StatusCodes.Status422UnprocessableEntity);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while setting last login time to a user.");
                return Result.Failure("An internal error occurred.", StatusCodes.Status500InternalServerError);
            }
        }

        Result<int> IUserRepository.GetTotalUsers()
        {
            try
            {
                return Result.Success(context.User.Count());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while counting the total of users.");
                return Result.Failure<int>("An internal error occurred.", StatusCodes.Status500InternalServerError);
            }
        }
    }
}
