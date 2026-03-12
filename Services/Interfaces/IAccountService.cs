using ManageAccountWebAPI.Data.DTOs;

namespace ManageAccountWebAPI.Services.Interfaces
{
    public interface IAccountService
    {
        IEnumerable<AccountDTO> GetAll();
        AccountDTO? GetById(int id);
        AccountDTO Create(CreateAccountRequestDTO request);
    }
}
