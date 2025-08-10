using System.Threading.Tasks;
using BourbonVault.Core.Models;

namespace BourbonVault.Core.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Token, string Message)> RegisterUserAsync(string username, string email, string password, string displayName);
        Task<(bool Success, string Token, string Message)> LoginAsync(string email, string password);
        Task<bool> UserExistsAsync(string email);
    }
}
