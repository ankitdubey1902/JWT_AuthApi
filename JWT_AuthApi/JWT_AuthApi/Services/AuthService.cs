using JWT_AuthApi.Models.Auth;
using JWT_AuthApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace JWT_AuthApi.Services
{
    public class AuthService
    {
        private readonly AuthDemoDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(AuthDemoDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<User> Authenticate(string username, string password, string ipAddress)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == username);

            // Check if user exists and password is correct
            if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> Register(User user, string password, string ipAddress)
        {
            // Validate
            if (await _context.Users.AnyAsync(x => x.Username == user.Username))
                throw new Exception("Username \"" + user.Username + "\" is already taken");

            if (await _context.Users.AnyAsync(x => x.Email == user.Email))
                throw new Exception("Email \"" + user.Email + "\" is already registered");

            // Create password hash
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            // Save user
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<AuthenticateResponse> GenerateTokenResponse(User user, string ipAddress)
        {
            // Generate JWT token
            var jwtToken = _jwtService.GenerateJwtToken(user);

            // Generate refresh token
            var refreshToken = _jwtService.GenerateRefreshToken(ipAddress);

            // Save refresh token
            refreshToken.UserId = user.Id;
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthenticateResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                JwtToken = jwtToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<AuthenticateResponse> RefreshToken(string token, string ipAddress)
        {
            var user = await GetUserByRefreshToken(token);
            var refreshToken = await _context.RefreshTokens.SingleAsync(x => x.Token == token);

            // Replace old refresh token with a new one
            var newRefreshToken = _jwtService.GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            newRefreshToken.UserId = user.Id;
            await _context.RefreshTokens.AddAsync(newRefreshToken);
            await _context.SaveChangesAsync();

            // Generate new JWT
            var jwtToken = _jwtService.GenerateJwtToken(user);

            return new AuthenticateResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                JwtToken = jwtToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        public async Task RevokeToken(string token, string ipAddress)
        {
            var refreshToken = await _context.RefreshTokens.SingleOrDefaultAsync(x => x.Token == token);

            if (refreshToken == null || !refreshToken.IsActive)
                throw new Exception("Invalid token");

            // Revoke token
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        private async Task<User> GetUserByRefreshToken(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.Token == token);

            if (refreshToken == null || !refreshToken.IsActive)
                throw new Exception("Invalid token");

            return refreshToken.User;
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (storedHash.Length != 64 || storedSalt.Length != 128)
                throw new ArgumentException("Invalid password hash or salt");

            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(storedHash);
        }
    }
}