using Microsoft.AspNetCore.Mvc;
using ManageAccountWebAPI.Services.Interfaces;

namespace ManageAccountWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger) : ControllerBase
    {
        [HttpPost("deposit/{id:int}/savings")]
        public ActionResult DepositToSavings(int id, [FromQuery] decimal amount)
        {
            logger.LogInformation("Received savings deposit request for account {AccountId} with amount {Amount}.", id, amount);
            var validateError = ValidateAmount(id, amount, "savings deposit");
            if (validateError != null)
            {
                return validateError;
            }

            var success = transactionService.DepositToSavings(id, amount);
            if (!success)
            {
                logger.LogWarning("Savings deposit failed for account {AccountId} with amount {Amount}.", id, amount);
                return NotFound($"Không tìm thấy tài khoản có id = {id} hoặc tài khoản không có số dư tiết kiệm.");
            }

            logger.LogInformation("Completed savings deposit for account {AccountId} with amount {Amount}.", id, amount);
            return Ok($"Đã nạp {amount} vào tài khoản tiết kiệm của tài khoản có id = {id}.");
        }

        [HttpPost("deposit/{id:int}/checking")]
        public ActionResult DepositToChecking(int id, [FromQuery] decimal amount)
        {
            logger.LogInformation("Received checking deposit request for account {AccountId} with amount {Amount}.", id, amount);
            var validateError = ValidateAmount(id, amount, "checking deposit");
            if (validateError != null)
            {
                return validateError;
            }

            var success = transactionService.DepositToChecking(id, amount);
            if (!success)
            {
                logger.LogWarning("Checking deposit failed for account {AccountId} with amount {Amount}.", id, amount);
                return NotFound($"Không tìm thấy tài khoản có id = {id} hoặc tài khoản không có số dư thanh toán.");
            }

            logger.LogInformation("Completed checking deposit for account {AccountId} with amount {Amount}.", id, amount);
            return Ok($"Đã nạp {amount} vào tài khoản thanh toán của tài khoản có id = {id}.");
        }

        [HttpPost("withdraw/{id:int}/savings")]
        public ActionResult WithdrawFromSavings(int id, [FromQuery] decimal amount)
        {
            logger.LogInformation("Received savings withdrawal request for account {AccountId} with amount {Amount}.", id, amount);
            var validateError = ValidateAmount(id, amount, "savings withdrawal");
            if (validateError != null)
            {
                return validateError;
            }

            var success = transactionService.WithdrawFromSavings(id, amount);
            if (!success)
            {
                logger.LogWarning("Savings withdrawal failed for account {AccountId} with amount {Amount}.", id, amount);
                return NotFound($"Không tìm thấy tài khoản có id = {id} hoặc tài khoản không có số dư tiết kiệm.");
            }

            logger.LogInformation("Completed savings withdrawal for account {AccountId} with amount {Amount}.", id, amount);
            return Ok($"Đã rút {amount} từ tài khoản tiết kiệm của tài khoản có id = {id}.");
        }

        [HttpPost("withdraw/{id:int}/checking")]
        public ActionResult WithdrawFromChecking(int id, [FromQuery] decimal amount)
        {
            logger.LogInformation("Received checking withdrawal request for account {AccountId} with amount {Amount}.", id, amount);
            var validateError = ValidateAmount(id, amount, "checking withdrawal");
            if (validateError != null)
            {
                return validateError;
            }

            var success = transactionService.WithdrawFromChecking(id, amount);
            if (!success)
            {
                logger.LogWarning("Checking withdrawal failed for account {AccountId} with amount {Amount}.", id, amount);
                return NotFound($"Không tìm thấy tài khoản có id = {id} hoặc tài khoản không có số dư thanh toán.");
            }

            logger.LogInformation("Completed checking withdrawal for account {AccountId} with amount {Amount}.", id, amount);
            return Ok($"Đã rút {amount} từ tài khoản thanh toán của tài khoản có id = {id}.");
        }

        [HttpGet("savings/total")]
        public ActionResult<decimal> GetTotalSavingsBalance()
        {
            var totalSavings = transactionService.GetTotalSavingsBalance();
            logger.LogInformation("Returned total savings balance {TotalSavingsBalance}.", totalSavings);
            return Ok(totalSavings);
        }

        private BadRequestObjectResult? ValidateAmount(int id, decimal amount, string operationDescription)
        {
            if (amount <= 0)
            {
                logger.LogWarning("Rejected {Operation} for account {AccountId} because amount {Amount} is not positive.", operationDescription, id, amount);
                return BadRequest("Số tiền phải lớn hơn 0.");
            }
            return null;
        }
    }
}