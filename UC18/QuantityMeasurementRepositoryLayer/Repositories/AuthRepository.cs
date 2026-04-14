using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuantityMeasurementModelLayer.Entities;
using QuantityMeasurementRepositoryLayer.Interfaces;
using QuantityMeasurementRepositoryLayer.Repositories.EfCore;

namespace QuantityMeasurementRepositoryLayer.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly QuantityMeasurementDbContext _db;

        public AuthRepository(QuantityMeasurementDbContext db)
        {
            _db = db;
        }

        public async Task<UserEntity?> GetUserByUsernameAsync(string username) =>
            await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        public async Task<UserEntity?> GetUserByEmailAsync(string email) =>
            await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<UserEntity?> GetUserByIdAsync(long id) =>
            await _db.Users.FindAsync(id);

        public async Task<UserEntity> CreateUserAsync(UserEntity user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<UserEntity> UpdateUserAsync(UserEntity user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<List<UserEntity>> GetAllUsersAsync() =>
            await _db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();

        public async Task<RefreshTokenEntity> CreateRefreshTokenAsync(RefreshTokenEntity token)
        {
            _db.RefreshTokens.Add(token);
            await _db.SaveChangesAsync();
            return token;
        }

        public async Task<RefreshTokenEntity?> GetRefreshTokenAsync(string token) =>
            await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);

        public async Task RevokeAllUserTokensAsync(long userId, string ipAddress)
        {
            var tokens = await _db.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null)
                .ToListAsync();

            foreach (var t in tokens)
            {
                t.RevokedAt    = DateTime.UtcNow;
                t.RevokedByIp  = ipAddress;
            }

            await _db.SaveChangesAsync();
        }
    }
}
