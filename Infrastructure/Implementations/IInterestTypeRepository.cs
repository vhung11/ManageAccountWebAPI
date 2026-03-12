using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Context;

namespace ManageAccountWebAPI.Infrastructure.Implementations
{
    /// <summary>
    /// Implementation của InterestType Repository
    /// </summary>
    public class InterestTypeRepository : IInterestTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public InterestTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public InterestType? GetById(int id)
        {
            return _context.InterestTypes.FirstOrDefault(it => it.Id == id);
        }

        public InterestType? GetByRate(decimal rate)
        {
            return _context.InterestTypes.FirstOrDefault(it => it.Rate == rate);
        }

        public InterestType Add(InterestType interestType)
        {
            _context.InterestTypes.Add(interestType);
            return interestType;
        }

        public void Delete(InterestType interestType)
        {
            _context.InterestTypes.Remove(interestType);
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }
    }
}
