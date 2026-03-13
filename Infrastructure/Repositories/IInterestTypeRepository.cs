using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Infrastructure.Repositories
{
    /// <summary>
    /// Interface cho InterestType Repository
    /// </summary>
    public interface IInterestTypeRepository : IBaseRepository<InterestType>
    {
        InterestType? GetByRate(decimal rate);
    }
}