using System.Threading.Tasks;

namespace SimpleUnitTest.Domain
{
    public interface IAccountService
    {
        /// <summary>
        /// Returns amount asynchronously of account which has given account id. Throws exception when account not found
        /// </summary>
        /// <param name="accountId">Account id to search account</param>
        /// <returns>double</returns>
        Task<double> GetAccountAmountAsync(int accountId);

        /// <summary>
        /// Returns amount of account which has given account id. Throws exception when account not found
        /// </summary>
        /// <param name="accountId">Account id to search account</param>
        /// <returns>double</returns>
        double GetAccountAmount(int accountId);
    }
}
