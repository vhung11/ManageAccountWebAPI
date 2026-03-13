using Microsoft.AspNetCore.Mvc;
using ManageAccountWebAPI.Services.Interfaces;

namespace ManageAccountWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost("deposit/{id:int}/savings")]
        public ActionResult DepositToSavings(int id, [FromQuery] decimal amount)
        {
            _logger.LogInformation("Received savings deposit request for account {AccountId} with amount {Amount}.", id, amount);
            var validationError = ValidateAmount(id, amount, "savings deposit");
            if (validationError != null) return validationError;

            var success = _transactionService.DepositToSavings(id, amount);
            if (!success)
            {
                _logger.LogWarning("Savings deposit failed for account {AccountId} with amount {Amount}.", id, amount);
                return NotFound($"Không tìm thấy tài khoản có id = {id} hoặc tài khoản không có số dư tiết kiệm.");
            }

            _logger.LogInformation("Completed savings deposit for account {AccountId} with amount {Amount}.", id, amount);
            return Ok($"Đã nạp {amount} vào tài khoản tiết kiệm của tài khoản có id = {id}.");
        }

        [HttpPost("deposit/{id:int}/checking")]
        public ActionResult DepositToChecking(int id, [FromQuery] decimal amount)
        {
            _logger.LogInformation("Received checking deposit request for account {AccountId} with amount {Amount}.", id, amount);
            var validationError = ValidateAmount(id, amount, "checking deposit");
            if (validationError != null) return validationError;

            var success = _transactionService.DepositToChecking(id, amount);
            if (!success)
            {
                _logger.LogWarning("Checking deposit failed for account {AccountId} with amount {Amount}.", id, amount);
                return NotFound($"Không tìm thấy tài khoản có id = {id} hoặc tài khoản không có số dư thanh toán.");
            }

            _logger.LogInformation("Completed checking deposit for account {AccountId} with amount {Amount}.", id, amount);
            return Ok($"Đã nạp {amount} vào tài khoản thanh toán của tài khoản có id = {id}.");
        }

        [HttpPost("withdraw/{id:int}/savings")]
        public ActionResult WithdrawFromSavings(int id, [FromQuery] decimal amount)
        {
            _logger.LogInformation("Received savings withdrawal request for account {AccountId} with amount {Amount}.", id, amount);
            var validationError = ValidateAmount(id, amount, "savings withdrawal");
            if (validationError != null) return validationError;

            var success = _transactionService.WithdrawFromSavings(id, amount);
            if (!success)
            {
                _logger.LogWarning("Savings withdrawal failed for account {AccountId} with amount {Amount}.", id, amount);
                return NotFound($"Không tìm thấy tài khoản có id = {id} hoặc tài khoản không có số dư tiết kiệm.");
            }

            _logger.LogInformation("Completed savings withdrawal for account {AccountId} with amount {Amount}.", id, amount);
            return Ok($"Đã rút {amount} từ tài khoản tiết kiệm của tài khoản có id = {id}.");
        }

        [HttpPost("withdraw/{id:int}/checking")]
        public ActionResult WithdrawFromChecking(int id, [FromQuery] decimal amount)
        {
            _logger.LogInformation("Received checking withdrawal request for account {AccountId} with amount {Amount}.", id, amount);
            var validationError = ValidateAmount(id, amount, "checking withdrawal");
            if (validationError != null) return validationError;

            var success = _transactionService.WithdrawFromChecking(id, amount);
            if (!success)
            {
                _logger.LogWarning("Checking withdrawal failed for account {AccountId} with amount {Amount}.", id, amount);
                return NotFound($"Không tìm thấy tài khoản có id = {id} hoặc tài khoản không có số dư thanh toán.");
            }

            _logger.LogInformation("Completed checking withdrawal for account {AccountId} with amount {Amount}.", id, amount);
            return Ok($"Đã rút {amount} từ tài khoản thanh toán của tài khoản có id = {id}.");
        }

        [HttpGet("savings/total")]
        public ActionResult<decimal> GetTotalSavingsBalance()
        {
            var totalSavings = _transactionService.GetTotalSavingsBalance();
            _logger.LogInformation("Returned total savings balance {TotalSavingsBalance}.", totalSavings);
            return Ok(totalSavings);
        }

        private BadRequestObjectResult? ValidateAmount(int id, decimal amount, string operationDescription)
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Rejected {OperationDescription} for account {AccountId} because amount {Amount} is not positive.", operationDescription, id, amount);
                return BadRequest("Số tiền phải lớn hơn 0.");
            }
            return null;
        }
    }
}