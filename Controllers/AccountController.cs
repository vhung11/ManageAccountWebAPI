using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManageAccountWebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AccountController : ControllerBase
	{
		private readonly IAccountService _accountService;
		private readonly ILogger<AccountController> _logger;

		public AccountController(IAccountService accountService, ILogger<AccountController> logger)
		{
			_accountService = accountService;
			_logger = logger;
		}

		[HttpGet]
		public ActionResult<IEnumerable<AccountDTO>> GetAll()
		{
			var accounts = _accountService.GetAll().ToList();
			_logger.LogInformation("Returned {AccountCount} accounts.", accounts.Count);
			return Ok(accounts);
		}

		[HttpGet("{id:int}")]
		public ActionResult<AccountDTO> GetById(int id)
		{
			_logger.LogInformation("Fetching account {AccountId}.", id);
			var account = _accountService.GetById(id);
			if (account is null)
			{
				_logger.LogWarning("Account {AccountId} was not found.", id);
				return NotFound($"Không tìm thấy tài khoản có id = {id}.");
			}

			_logger.LogInformation("Returned account {AccountId}.", id);
			return Ok(account);
		}

		[HttpPost]
		public ActionResult<AccountDTO> Create([FromBody] CreateAccountRequestDTO request)
		{
			_logger.LogInformation("Received request to create account for {AccountName}.", request.Name);
			var createdAccount = _accountService.Create(request);
			_logger.LogInformation("Created account {AccountId} for {AccountName}.", createdAccount.Id, createdAccount.Name);
			return CreatedAtAction(nameof(GetById), new { id = createdAccount.Id }, createdAccount);
		}

		[HttpPut("{id:int}")]
		public ActionResult<AccountDTO> Update(int id, [FromBody] UpdateAccountRequestDTO request)
		{
			_logger.LogInformation("Received request to update account {AccountId}.", id);
			var updatedAccount = _accountService.Update(id, request);
			if (updatedAccount is null)
			{
				_logger.LogWarning("Account {AccountId} was not found for update.", id);
				return NotFound($"Không tìm thấy tài khoản có id = {id}.");
			}

			_logger.LogInformation("Updated account {AccountId}.", id);
			return Ok(updatedAccount);
		}

		[HttpDelete("{id:int}")]
		public ActionResult Delete(int id)
		{
			_logger.LogInformation("Received request to delete account {AccountId}.", id);
			var success = _accountService.Delete(id);
			if (!success)
			{
				_logger.LogWarning("Account {AccountId} was not found for deletion.", id);
				return NotFound($"Không tìm thấy tài khoản có id = {id}.");
			}

			_logger.LogInformation("Deleted account {AccountId}.", id);
			return NoContent();
		}

		[HttpGet("ranked")]
		public ActionResult<IEnumerable<AccountDTO>> GetAccountsRankedByBalance()
		{
			var accounts = _accountService.GetAccountsRankedByBalance().ToList();
			_logger.LogInformation("Returned {AccountCount} ranked accounts.", accounts.Count);
			return Ok(accounts);
		}

		[HttpGet("below-balance")]
		public ActionResult<IEnumerable<AccountDTO>> GetAccountsBelowBalance([FromQuery] decimal threshold)
		{
			_logger.LogInformation("Fetching accounts with total balance below {Threshold}.", threshold);
			var accounts = _accountService.GetAccountsBelowBalance(threshold).ToList();
			_logger.LogInformation("Returned {AccountCount} accounts with total balance below {Threshold}.", accounts.Count, threshold);
			return Ok(accounts);
		}

		[HttpGet("top-checking/{topN:int}")]
		public ActionResult<IEnumerable<AccountDTO>> GetTopNCheckingAccounts(int topN)
		{
			_logger.LogInformation("Fetching top {TopN} checking accounts with the lowest balances.", topN);
			var accounts = _accountService.GetTopNCheckingAccounts(topN).ToList();
			_logger.LogInformation("Returned {AccountCount} checking accounts for top {TopN} request.", accounts.Count, topN);
			return Ok(accounts);
		}
	}
}
