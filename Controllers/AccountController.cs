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

		public AccountController(IAccountService accountService)
		{
			_accountService = accountService;
		}

		[HttpGet]
		public ActionResult<IEnumerable<AccountDTO>> GetAll()
		{
			var accounts = _accountService.GetAll();
			return Ok(accounts);
		}

		[HttpGet("{id:int}")]
		public ActionResult<AccountDTO> GetById(int id)
		{
			var account = _accountService.GetById(id);
			if (account is null)
			{
				return NotFound($"Không tìm thấy tài khoản có id = {id}.");
			}

			return Ok(account);
		}

		[HttpPost]
		public ActionResult<AccountDTO> Create([FromBody] CreateAccountRequestDTO request)
		{
			var createdAccount = _accountService.Create(request);
			return CreatedAtAction(nameof(GetById), new { id = createdAccount.Id }, createdAccount);
		}
	}
}
