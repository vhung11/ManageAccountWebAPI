using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}