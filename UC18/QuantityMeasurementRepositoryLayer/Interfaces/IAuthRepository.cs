using System.Collections.Generic;
using System.Threading.Tasks;
using QuantityMeasurementModelLayer.Entities;

namespace QuantityMeasurementRepositoryLayer.Interfaces
{
    public interface IAuthRepository
    {
        Task<UserEntity?>          GetUserByUsernameAsync(string username);
        Task<UserEntity?>          GetUserByEmailAsync(string email);
        Task<UserEntity?>          GetUserByIdAsync(long id);
        Task<UserEntity>           CreateUserAsync(UserEntity user);
        Task<UserEntity>           UpdateUserAsync(UserEntity user);
        Task<List<UserEntity>>     GetAllUsersAsync();
        Task<RefreshTokenEntity>   CreateRefreshTokenAsync(RefreshTokenEntity token);
        Task<RefreshTokenEntity?>  GetRefreshTokenAsync(string token);
        Task                       RevokeAllUserTokensAsync(long userId, string ipAddress);
    }
}
