#SIMPLE UNIT TEST SOLUTION
This project contains solutions to the "Developer Candidate Assignment". It consists of 2 projects, one for solution to the problems and one for testing the implementations.

##Getting Started
This solutions are implemented in order to test and solve the problems described in the assignment. All 3 problems has been resolved and required implementations has been accomplished.

##Tech/framework used
The techs used in the solution as follows;
	
    1. .Net core 2.1
	2. .Net standart 2.0
	3. xunit
	4. Moq
	5. FluentAssertions
	6. Git
    7. Visual Studio Community 2017
    
Build with
	
    1. dotnet build (Bitbucket pipeline)
	
##How is it done?
2 projects has been added to the solution which are "SimpleUnitTest.Domain" and "SimpleUnitTest.Domain.Test". 
EVisionAssignment.Domain project consists of "AccountInfo" and "IAccountService".
EVisionAssignment.Domain.Test project consists of "AccountInfoTests" class. There are 5 base tests and with "InlineData", it expands to 13.

**NOTE!! : I did not use any try catch block on purpose, I assume that a global exception handling exists in the application. **

**Problem#1** : As it is described in the assignment, solution to the problem#1 is to assert the behaviour of the method "RefreshAmount" of class "AccountInfo". The method in "AccountInfo" class remained as follows;

    public void RefreshAmount()
    {
        Amount = _accountService.GetAccountAmount(_accountId);
    }
		
In order to the assert the behaviour of the method, 2 main tests have been implemented. Those are;

    1. public void AccountInfo_RefreshAmount_should_throw_exception_due_to_not_found_account(int accountId)
	2. public void AccountInfo_RefreshAmount_should_return_amount(int accountId, double expectedAmount)

**a.** The 1st method asserts if there is no account found in the "IAccountService". **NOTE !!** I assumed that an exception would be thrown if there is no account found with the given "accountId".  		
**b.** The 2nd method asserts that proper amount would be returned from the "GetAccountAmount" method of "IAccountService". Inline attribute has been used to assert amounts returned of the different account id's. The method asserts that 
"Amount" property is equal to the incoming "expectedAmount" parameter. Required mocking of interface has been implemented as follows;

	1. _accountService.Setup(a => a.GetAccountAmount(accountId)).Returns(() => { throw new Exception(exceptionMessage); });
	2. _accountService.Setup(a => a.GetAccountAmount(accountId)).Returns(expectedAmount);

**Problem#2** : It is found out that "IAccountService.GetAccountAmount()" returns the amount with some delay and it decreases the performance of the application and it has to be called asynchronous. Furthermore it is mentioned that 
concurrent invocations may occur to "AccountInfo.RefreshAmount()" method.

**NOTE!!: Because of mentioning concurrent invocation, I hereby do assume that "AccountInfo" class is designed to be used in cache or similar, meaning only one instance of "AccountInfo" exist in memory per account id. I used a locking ("SemaphoreSlim") in order the achieve "thread safe" setting of 
the value of "Amount". I also assumed that "Amount" could be updated to the persistance layer somewhere else in the application and threrefore it has to be the up-to-date value when reading the value of it. Because of that, I used the locking it right before 
reading and setting the value from the interface. The implementations are based on this assumption.**

In order to call the method asynchronous, another method has been added to the interface and to the class, now both has 2 methods;

    public interface IAccountService
    {
        Task<double> GetAccountAmountAsync(int accountId);

        double GetAccountAmount(int accountId);
    }
	
	public class AccountInfo
    {
        public async Task RefreshAmountAsync()
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                Amount = await _accountService.GetAccountAmountAsync(_accountId);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public void RefreshAmount()
        {
            Amount = _accountService.GetAccountAmount(_accountId);
        }
    }

In order the assert the behaviour of the async method, 2 main tests have been implemented. Those are;

	1. public void AccountInfo_RefreshAmountAsync_should_throw_exception_due_to_not_found_account(int accountId)
	2. public void AccountInfo_RefreshAmountAsync_should_return_amount(int accountId, double expectedAmount)

**a.** The 1st method, again, asserts if there is no account found in the "IAccountService". I assumed that an exception would be thrown if there is no account found with given "accountId". It has been implemented according 
to asynchronous architecture. Requeired mocking of interface has been implemented as follows;

	_accountService.Setup(a => a.GetAccountAmountAsync(accountId)).Returns(async () =>
    {
        Random rnd = new Random();

        await Task.Delay(rnd.Next(500, 1000));

        throw new Exception(exceptionMessage);
    });

**b.** The 2nd method is the most important method which is about assertioning of the amount returned from the interface according the asynchronous architecture with concurrent invocations. Required mocking of interface has been 
implemented as follows;

	_accountService.Setup(a => a.GetAccountAmountAsync(accountId)).Returns(async () =>
    {
        Random rnd = new Random();

        await Task.Delay(rnd.Next(500, 1000));
        
        return await Task.FromResult<double>(expectedAmount);
    });
			
With **Task.Delay**, I simulated the slowness of the method. In the test method, I also simulated the concurrent invocations as well with the following code;
    
    int concurrentCount = 10;
    
	List<Task> tasks = new List<Task>();

    for (int i = 0; i < concurrentCount; i++)
    {
        tasks.Add(Task.Run(() => accountInfo.RefreshAmountAsync()));
    }

    Task.WhenAll(tasks).GetAwaiter().GetResult();

Not only the methods asserts that amount return from the interface is correct, it also asserts that it has been called "concurrentCount" times concurrently. A "SemaphoreSlim" has been used to ensure that only one Thread can update
"Amount" at a time.

**c.** I also had added a constructor test in order to check whether "IAccountService" has been passed null. Consequently, it looks like this;

	public AccountInfo(int accountId, IAccountService accountService)
    {
        _accountId = accountId;
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

        _semaphoreSlim = new SemaphoreSlim(1, 1);
    }

and the test method makes sure that if the interface gets passed null, it throws an exception. The name of the test is;
	
	public void AccountInfo_Ctor_Test()


**Problem#3** : So it is all good so far but we have to run a DevOps process after committing/merging the code. To achieve this, I have used Bitbucket's pipeline. It has been configured in the **bitbucket-pipelines.yml** file. It basically
builds the code, runs the tests and publishes a nuget package to https://www.myget.org". It does it in a order as follows;
    
    -on pull requests, it builds the application and runs the tests
    -when merged to master branch, it builds the project, creates a nuget package and pushes it to MyGet.

Just to demonstrate it I have used MyGet. Full package path is "https://www.myget.org/feed/ktt/package/nuget/SimpleUnitTestAssignment.Domain". I ll keep it public until you check it
then I will delete it. Some statements for build process are;

	- dotnet build $PROJECT_NAME=> for building the project
	- dotnet test $TEST_NAME => test the project 
	- dotnet pack => to pack
	- dotnet nuget push => push the package to the repo with extension of "*.nupkg"


##Credits
	Credit to push to myget Maarten Balliauw https://blog.maartenballiauw.be/post/2016/08/17/building-nuget-netcore-using-atlassian-bitbucket-pipelines.html
	Credit to inspiration repos mongodb c# driver https://github.com/mongodb/mongo-csharp-driver and Neo4J c# driver https://github.com/neo4j/neo4j-dotnet-driver