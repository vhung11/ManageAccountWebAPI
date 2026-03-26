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

        // ===================== USER SCOPE =====================

        [HttpGet]
        [AuthorizeFunction("Account.Read")]
        public ActionResult<IEnumerable<AccountDTO>> GetMyAccounts()
        {
            var userId = GetUserId();

            var accounts = accountService.GetAllByUserId(userId).ToList();
            logger.LogInformation("Returned {AccountCount} accounts for user {UserId}.", accounts.Count, userId);

            return Ok(accounts);
        }

        [HttpGet("{accountId:int}")]
        [AuthorizeFunction("Account.Read")]
        public ActionResult<AccountDTO> GetById(int accountId)
        {
            var userId = GetUserId();

            logger.LogInformation("Fetching account {AccountId} for user {UserId}.", accountId, userId);

            var account = accountService.GetById(userId, accountId);
            if (account is null)
            {
                logger.LogWarning("Account {AccountId} not found or not owned by user {UserId}.", accountId, userId);
                return NotFound($"Không tìm thấy tài khoản có id = {accountId}.");
            }

            return Ok(account);
        }

        [HttpPost]
        [AuthorizeFunction("Account.Create")]
        public ActionResult<AccountDTO> Create([FromBody] CreateAccountRequestDTO request)
        {
            var userId = GetUserId();

            logger.LogInformation("Creating account for user {UserId}.", userId);

            var createdAccount = accountService.Create(userId, request);

            logger.LogInformation("Created account {AccountId} for user {UserId}.", createdAccount.AccountId, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { accountId = createdAccount.AccountId },
                createdAccount
            );
        }

        [HttpDelete("{accountId:int}")]
        [AuthorizeFunction("Account.Delete")]
        public IActionResult Delete(int accountId)
        {
            var userId = GetUserId();

            logger.LogInformation("Deleting account {AccountId} for user {UserId}.", accountId, userId);

            var success = accountService.Delete(userId, accountId);
            if (!success)
            {
                logger.LogWarning("Delete failed for account {AccountId}, user {UserId}.", accountId, userId);
                return NotFound($"Không tìm thấy tài khoản có id = {accountId}.");
            }

            logger.LogInformation("Deleted account {AccountId}.", accountId);
            return NoContent();
        }

        // ===================== ADMIN / SYSTEM SCOPE =====================

        [HttpGet("admin/ranked")]
        [AuthorizeFunction("Account.Read")]
        public ActionResult<IEnumerable<AccountDTO>> GetAccountsRankedByBalance()
        {
            var accounts = accountService.GetAccountsRankedByBalance().ToList();

            logger.LogInformation("Returned {AccountCount} ranked accounts system-wide.", accounts.Count);

            return Ok(accounts);
        }

        [HttpGet("admin/below-balance")]
        [AuthorizeFunction("Account.Read")]
        public ActionResult<IEnumerable<AccountDTO>> GetAccountsBelowBalance([FromQuery] decimal threshold)
        {
            logger.LogInformation("Fetching accounts with total balance below {Threshold}.", threshold);

            var accounts = accountService.GetAccountsBelowBalance(threshold).ToList();

            logger.LogInformation("Returned {AccountCount} accounts below {Threshold}.", accounts.Count, threshold);

            return Ok(accounts);
        }

        [HttpGet("admin/top-checking/{topN:int}")]
        [AuthorizeFunction("Account.Read")]
        public ActionResult<IEnumerable<AccountDTO>> GetTopNCheckingAccounts(int topN)
        {
            logger.LogInformation("Fetching top {TopN} checking accounts.", topN);

            var accounts = accountService.GetTopNCheckingAccounts(topN).ToList();

            logger.LogInformation("Returned {AccountCount} checking accounts.", accounts.Count);

            return Ok(accounts);
        }

        [HttpPost("admin/apply-interest")]
        [AuthorizeFunction("Account.ApplyInterest")]
        public IActionResult ApplyInterest()
        {
            accountService.ApplyInterest();

            logger.LogInformation("Applied interest to all accounts.");

            return Ok();
        }

        // ===================== HELPER =====================

        private int GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (claim == null)
                throw new UnauthorizedAccessException("Token không chứa userId.");

            return int.Parse(claim.Value);
        }
    }
}