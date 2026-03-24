using Microsoft.AspNetCore.Mvc;
using ManageAccountWebAPI.Services.Interfaces;
using ManageAccountWebAPI.Infrastructure.Filters;

namespace ManageAccountWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService transactionService;
        private readonly ILogger<TransactionController> logger;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
        {
            this.transactionService = transactionService;
            this.logger = logger;
        }
        [HttpPost("deposit/{id:int}/savings")]
        [AuthorizeFunction("Account.Update")]
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
        [AuthorizeFunction("Account.Update")]
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
        [AuthorizeFunction("Account.Update")]
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
        [AuthorizeFunction("Account.Update")]
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
        [AuthorizeFunction("Account.Read")]
        public ActionResult<decimal> GetTotalSavingsBalance()
        {
            var totalSavings = transactionService.GetTotalSavingsBalance();
            logger.LogInformation("Returned total savings balance {TotalSavingsBalance}.", totalSavings);
            return Ok(totalSavings);
        }

        [HttpPost("withdraw/{id:int}/checking/all")]
        [AuthorizeFunction("Account.Update")]
        public ActionResult<decimal> WithdrawAllCheckingBalance(int id)
        {
            logger.LogInformation("Received request to withdraw all checking balance for account {AccountId}.", id);
            var withdrawnAmount = transactionService.WithdrawAllCheckingBalance(id);
            if (withdrawnAmount == 0)
            {
                logger.LogWarning("Withdraw all checking balance failed for account {AccountId}.", id);
                return NotFound($"Không tìm thấy tài khoản có id = {id} hoặc tài khoản không có số dư thanh toán.");
            }

            logger.LogInformation("Withdrew all checking balance {Amount} for account {AccountId}.", withdrawnAmount, id);
            return Ok(withdrawnAmount);
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