using System;
using System.Threading.Tasks;

namespace SimpleUnitTest.Domain
{
    /// <summary>
    /// Represents Account Info of a given account id.
    /// </summary>
    public class AccountInfo
    {
        private readonly int _accountId;
        private readonly IAccountService _accountService;

        /// <summary>
        /// Initializes new instance of AccountInfo
        /// </summary>
        /// <param name="accountId">Account id to process</param>
        /// <param name="accountService">Service to read amount of account</param>
        public AccountInfo(int accountId, IAccountService accountService)
        {
            _accountId = accountId;
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public double Amount { get; private set; }

        /// <summary>
        /// Updates Amount property asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task RefreshAmountAsync()
        {
            Amount = await _accountService.GetAccountAmountAsync(_accountId);
        }

        /// <summary>
        ///  Updates Amount property asynchronously
        /// </summary>
        public void RefreshAmount()
        {
            Amount = _accountService.GetAccountAmount(_accountId);
        }
    }
}