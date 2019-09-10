using SimpleUnitTest.Domain;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleUnitTest.Domain.Test
{ 
    public class AccountInfoTests
    {
        private Mock<IAccountService> _accountService;
        private const string exceptionMessage = "Account not found";

        public AccountInfoTests()
        {
            _accountService = new Mock<IAccountService>(MockBehavior.Loose);
        }

        [Fact]
        public void AccountInfo_Ctor_Test()
        {
            Assert.Throws<ArgumentNullException>(() => { new AccountInfo(1, null); });
        }

        [Theory]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public void AccountInfo_RefreshAmount_should_throw_exception_due_to_not_found_account(int accountId)
        {
            _accountService.Setup(a => a.GetAccountAmount(accountId)).Returns(() => { throw new Exception(exceptionMessage); });

            AccountInfo accountInfo = new AccountInfo(accountId, _accountService.Object);

            Exception ex = Assert.Throws<Exception>(() => { accountInfo.RefreshAmount(); });

            ex.Message.Should().Contain(exceptionMessage);
        }

        [Theory]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public async Task AccountInfo_RefreshAmountAsync_should_throw_exception_due_to_not_found_account(int accountId)
        {
            _accountService.Setup(a => a.GetAccountAmountAsync(accountId)).Returns(async () =>
            {
                Random rnd = new Random();

                await Task.Delay(rnd.Next(500, 1000));

                throw new Exception(exceptionMessage);
            });

            AccountInfo accountInfo = new AccountInfo(accountId, _accountService.Object);

            Exception ex = await Assert.ThrowsAsync<Exception>(async () => { await accountInfo.RefreshAmountAsync(); });

            ex.Message.Should().Contain(exceptionMessage);
        }

        [Theory]
        [InlineData(1, 50.00)]
        [InlineData(2, 100.00)]
        [InlineData(3, 150.00)]
        public async Task AccountInfo_RefreshAmountAsync_should_return_amount(int accountId, double expectedAmount)
        {
            _accountService.Setup(a => a.GetAccountAmountAsync(accountId)).Returns(async () =>
            {
                Random rnd = new Random();

                await Task.Delay(rnd.Next(500, 1000));

                return await Task.FromResult<double>(expectedAmount);
            });

            AccountInfo accountInfo = new AccountInfo(accountId, _accountService.Object);
            
            int concurrentCount = 10;
            
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < concurrentCount; i++)
            {
                tasks.Add(Task.Run(() => accountInfo.RefreshAmountAsync()));
            }

            await Task.WhenAll(tasks);

            _accountService.Verify(s => s.GetAccountAmountAsync(accountId), Times.Exactly(concurrentCount));

            accountInfo.Amount.Should().Be(expectedAmount);
        }

        [Theory]
        [InlineData(1, 50.00)]
        [InlineData(2, 100.00)]
        [InlineData(3, 150.00)]
        public void AccountInfo_RefreshAmount_should_return_amount(int accountId, double expectedAmount)
        {
            _accountService.Setup(a => a.GetAccountAmount(accountId)).Returns(expectedAmount);

            AccountInfo accountInfo = new AccountInfo(accountId, _accountService.Object);

            accountInfo.RefreshAmount();

            accountInfo.Amount.Should().Be(expectedAmount);
        }
    }
}
