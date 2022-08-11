using SimpleBankAPI.Models;

namespace SimpleBankAPI.Usecases
{
    public class UserUseCase
    {
        public async Task<UserModel> CreateUser(UserModel user)
        {
            return await Task.FromResult(user);
        }
    }
}
