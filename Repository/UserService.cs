
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace WebFunctionApp.Repository
{
    public class UserService 
    {
        private readonly string _connectionString;

        public UserService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IdentityUser> AuthenticateAsync(string username, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("SELECT Id, PasswordHash FROM Users WHERE UserName = @UserName", connection);
            command.Parameters.Add(new SqlParameter("UserName", SqlDbType.NVarChar) { Value = username });

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var storedHash = reader.GetString(1);
                if (VerifyPasswordHash(password, storedHash))
                {
                    return new IdentityUser { UserName = username, Id = reader.GetInt32(0).ToString() };
                }
            }

            return null;
        }

        public async Task RegisterAsync(string username, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var passwordHash = HashPassword(password);

            using var command = new SqlCommand("INSERT INTO Users (UserName, PasswordHash) VALUES (@UserName, @PasswordHash)", connection);
            command.Parameters.Add(new SqlParameter("UserName", SqlDbType.NVarChar) { Value = username });
            command.Parameters.Add(new SqlParameter("PasswordHash", SqlDbType.NVarChar) { Value = passwordHash });

            await command.ExecuteNonQueryAsync();
        }

        private static string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        private static bool VerifyPasswordHash(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var hash = parts[1];

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hash == hashed;
        }
    }
}
