using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.ResponseModels.Session;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Interfaces
{
    public interface IAccountService
    {
        /// <summary>
        /// Refresh tokens
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <param name="roles">Roles</param>
        /// <returns></returns>
        Task<TokenResponseModel> RefreshTokenAsync(string refreshToken, List<string> roles);

        /// <summary>
        /// Register a new user using email
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RegisterResponseModel> Register(RegisterRequestModel model);

        /// <summary>
        /// Login using email
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<LoginResponseModel> Login(LoginRequestModel model);

        /// <summary>
        /// Admin login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<LoginResponseModel> AdminLogin(AdminLoginRequestModel model);

        /// <summary>
        /// Logout
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns></returns>
        Task Logout(int userId);
    }
}
