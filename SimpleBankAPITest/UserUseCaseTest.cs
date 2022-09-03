using Microsoft.Extensions.Logging;
using Moq;
using Serilog.Core;
using SimpleBankAPI.Application.Usecases;
using SimpleBankAPI.Infrastructure.Repositories;

namespace SimpleBankAPITest
{
    public class UserUseCaseTest
    {
        //CreateUser
        
        private Mock<ILogger> _logger;
        private Mock<IUnitOfWork> _unitOfWork;

        [Fact]
        public void CreateUserTest()
        {
            var userUserCase = new UserUseCase(_logger, _unitOfWork);
            
        }
        
    }
}