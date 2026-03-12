using Identity.Domain.Entities;

namespace Identity.Application.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByUserNameAsync(string userName);
    Task<User?> FindByIdAsync(Guid id);
    Task<bool> CreateUserAsync(User user, string password);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> IsUserExistsAsync(Guid userId);

    Task<bool> AddUserToRoleAsync(User user, string role);
    Task<IList<string>> GetUserRolesAsync(User user);

    Task<bool> CheckPasswordAsync(User user, string password);
    Task<bool> ChangePasswordAsync(User user, string currentPassword, string newPassword);
    Task<bool> ResetPasswordAsync(User user, string token, string newPassword);
    Task<string?> GeneratePasswordResetTokenAsync(User user);

    Task<string?> GenerateEmailConfirmationTokenAsync(User user);
    Task<bool> VerifyConfirmaionEmailAsync(User user, string token);

    Task<bool> IsLockedOutAsync(User user);
    Task<DateTime?> GetLockoutEndDateAsync(User user);
    Task IncrementAccessFailedCountAsync(User user);
    Task ResetAccessFailedCountAsync(User user);
    Task<int> GetAccessFailedCountAsync(User user);
    Task<int> GetMaxFailedAccessAttemptsAsync();

    Task<bool> IsTwoFactorEnabledAsync(User user);

    Task UpdateLastLoginAsync(User user, DateTime loginTime);

    Task<string> GenerateAndStoreRefreshTokenAsync(Guid userId, string clientId, string userAgent, string ipAddress);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string ipAddress);

    Task<bool> IsValidClientAsync(string clientId);

    Task<MedicalFile?> AddOrUpdateMedicalFileAsync(MedicalFile medicalFile);
    Task<MedicalFile?> GetMedicalFileByUserIdAsync(Guid userId);
}