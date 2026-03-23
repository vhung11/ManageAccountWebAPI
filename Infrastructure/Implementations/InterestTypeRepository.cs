using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ManageAccountWebAPI.Infrastructure.Implementations
{
    /// <summary>
    /// Implementation của InterestType Repository
    /// </summary>
    public class InterestTypeRepository : BaseRepository<InterestType>, IInterestTypeRepository
    {
        public InterestTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        protected override DbSet<InterestType> DbSet => _context.InterestTypes;

        public InterestType? GetByRate(decimal rate)
        {
            return _context.InterestTypes.FirstOrDefault(it => it.Rate == rate);
        }
    }
}
