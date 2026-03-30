using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Controllers.Filters;
using ManageAccountWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManageAccountWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService accountService;
        private readonly ILogger<AccountController> logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            this.accountService = accountService;
            this.logger = logger;
        }
        [HttpGet]
        public ActionResult<IEnumerable<AccountDTO>> GetAll()
        {
            var accounts = accountService.GetAll().ToList();
            logger.LogInformation("Returned {AccountCount} accounts.", accounts.Count);
            return Ok(accounts);
        }

        [HttpGet("{id:int}")]
        [AuthorizeFunction("Account.Read")]
        public ActionResult<AccountDTO> GetById(int id)
        {
            logger.LogInformation("Fetching account {AccountId}.", id);
            var account = accountService.GetById(id);
            if (account is null)
            {
                logger.LogWarning("Account {AccountId} was not found.", id);
                return NotFound($"Không tìm thấy tài khoản có id = {id}.");
            }

            logger.LogInformation("Returned account {AccountId}.", id);
            return Ok(account);
        }

        [HttpPost]
        [AuthorizeFunction("Account.Create")]
        public ActionResult<AccountDTO> Create([FromBody] CreateAccountRequestDTO request)
        {
            logger.LogInformation("Received request to create account for {AccountName}.", request.Name);
            var createdAccount = accountService.Create(request);
            logger.LogInformation("Created account {AccountId} for {AccountName}.", createdAccount.Id, createdAccount.Name);
            return CreatedAtAction(nameof(GetById), new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPut("{id:int}")]
        [AuthorizeFunction("Account.Update")]
        public ActionResult<AccountDTO> Update(int id, [FromBody] UpdateAccountRequestDTO request)
        {
            logger.LogInformation("Received request to update account {AccountId}.", id);
            var updatedAccount = accountService.Update(id, request);
            if (updatedAccount is null)
            {
                logger.LogWarning("Account {AccountId} was not found for update.", id);
                return NotFound($"Không tìm thấy tài khoản có id = {id}.");
            }

            logger.LogInformation("Updated account {AccountId}.", id);
            return Ok(updatedAccount);
        }

        [HttpDelete("{id:int}")]
        [AuthorizeFunction("Account.Delete")]
        public ActionResult Delete(int id)
        {
            logger.LogInformation("Received request to delete account {AccountId}.", id);
            var success = accountService.Delete(id);
            if (!success)
            {
                logger.LogWarning("Account {AccountId} was not found for deletion.", id);
                return NotFound($"Không tìm thấy tài khoản có id = {id}.");
            }

            logger.LogInformation("Deleted account {AccountId}.", id);
            return NoContent();
        }

        [HttpGet("ranked")]
        [AuthorizeFunction("Account.Read")]
        public ActionResult<IEnumerable<AccountDTO>> GetAccountsRankedByBalance()
        {
            var accounts = accountService.GetAccountsRankedByBalance().ToList();
            logger.LogInformation("Returned {AccountCount} ranked accounts.", accounts.Count);
            return Ok(accounts);
        }

        [HttpGet("below-balance")]
        [AuthorizeFunction("Account.Read")]
        public ActionResult<IEnumerable<AccountDTO>> GetAccountsBelowBalance([FromQuery] decimal threshold)
        {
            logger.LogInformation("Fetching accounts with total balance below {Threshold}.", threshold);
            var accounts = accountService.GetAccountsBelowBalance(threshold).ToList();
            logger.LogInformation("Returned {AccountCount} accounts with total balance below {Threshold}.", accounts.Count, threshold);
            return Ok(accounts);
        }

        [HttpGet("top-checking/{topN:int}")]
        [AuthorizeFunction("Account.Read")]
        public ActionResult<IEnumerable<AccountDTO>> GetTopNCheckingAccounts(int topN)
        {
            logger.LogInformation("Fetching top {TopN} checking accounts with the lowest balances.", topN);
            var accounts = accountService.GetTopNCheckingAccounts(topN).ToList();
            logger.LogInformation("Returned {AccountCount} checking accounts for top {TopN} request.", accounts.Count, topN);
            return Ok(accounts);
        }

        [HttpPost("apply-interest")]
        [AuthorizeFunction("Account.ApplyInterest")]
        public ActionResult ApplyInterest()
        {
            accountService.ApplyInterest();
            logger.LogInformation("Applied interest to all accounts.");
            return Ok();
        }
    }
}
