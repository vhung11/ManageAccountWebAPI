using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Context;
using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Mappers;
using ManageAccountWebAPI.Services.Interfaces;

namespace ManageAccountWebAPI.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAccountBalanceRepository _accountBalanceRepository;
        private readonly IInterestTypeRepository _interestTypeRepository;
        private readonly ApplicationDbContext _dbContext;

        public AccountService(
            IAccountRepository accountRepository,
            IAccountBalanceRepository accountBalanceRepository,
            IInterestTypeRepository interestTypeRepository,
            ApplicationDbContext dbContext)
        {
            _accountRepository = accountRepository;
            _accountBalanceRepository = accountBalanceRepository;
            _interestTypeRepository = interestTypeRepository;
            _dbContext = dbContext;
        }

        public IEnumerable<AccountDTO> GetAll()
        {
            var accounts = _accountRepository.GetAll();
            var accountBalances = _accountBalanceRepository.GetAll();

            return AccountMapper.ToDTOList(accounts, accountBalances);
        }

        public AccountDTO? GetById(int id)
        {
            var account = _accountRepository.GetById(id);
            if (account is null)
            {
                return null;
            }

            var accountBalances = _accountBalanceRepository.GetByAccountId(id);
            return AccountMapper.ToDTO(account, accountBalances);
        }

        public AccountDTO Create(CreateAccountRequestDTO request)
        {
            var accountName = request.Name.Trim();
            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new ArgumentException("Tên tài khoản là bắt buộc.", nameof(request));
            }

            using var transaction = _dbContext.Database.BeginTransaction();

            var savingsInterestType = EnsureInterestType(4.7m);
            var checkingInterestType = EnsureInterestType(5.1m);

            var account = _accountRepository.Add(new Account
            {
                Name = accountName
            });

            var balances = new List<AccountBalance>
            {
                new()
                {
                    Account = account,
                    Type = "Tài khoản tiết kiệm",
                    Balance = request.SavingsBalance,
                    InterestType = savingsInterestType
                },
                new()
                {
                    Account = account,
                    Type = "Tài khoản thanh toán",
                    Balance = request.CheckingBalance,
                    InterestType = checkingInterestType
                }
            };

            foreach (var balance in balances)
            {
                _accountBalanceRepository.Add(balance);
                _accountBalanceRepository.SaveChanges();
            }

            _accountRepository.SaveChanges();
            transaction.Commit();

            return AccountMapper.ToDTO(account, balances);
        }

        private InterestType EnsureInterestType(decimal rate)
        {
            var interestType = _interestTypeRepository.GetByRate(rate);
            if (interestType is not null)
            {
                return interestType;
            }

            return _interestTypeRepository.Add(new InterestType
            {
                Rate = rate
            });
        }
    }
}
