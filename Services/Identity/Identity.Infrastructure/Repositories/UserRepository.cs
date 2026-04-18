using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Infrastructure.Identity;
using Identity.Infrastructure.Persistence;
using Identity.Application.Repositories;

namespace Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserDbContext _dbContext;

    public UserRepository(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        UserDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }


    private static User MapToDomain(ApplicationUser appUser) => new()
    {
        Id              = appUser.Id,
        UserName        = appUser.UserName,
        Email           = appUser.Email,
        FirstName       = appUser.FirstName,
        LastName        = appUser.LastName,
        PhoneNumber     = appUser.PhoneNumber,
        ProfilePhotoUrl = appUser.ProfilePhotoUrl,
        IsActive        = appUser.IsActive,
        CreatedAt       = appUser.CreatedAt,
        LastLoginAt     = appUser.LastLoginAt,
        IsEmailConfirmed = appUser.EmailConfirmed,
        MedicalFileUrl  = appUser.MedicalFileUrl,
        Gender          = appUser.Gender,
        DateOfBirth     = appUser.DateOfBirth
    };

    private static ApplicationUser MapToApplicationUser(User user) => new()
    {
        Id              = user.Id,
        UserName        = user.UserName,
        Email           = user.Email,
        FirstName       = user.FirstName,
        LastName        = user.LastName,
        PhoneNumber     = user.PhoneNumber,
        ProfilePhotoUrl = user.ProfilePhotoUrl,
        IsActive        = user.IsActive,
        CreatedAt       = user.CreatedAt,
        LastLoginAt     = user.LastLoginAt,
        EmailConfirmed  = user.IsEmailConfirmed,
        MedicalFileUrl  = user.MedicalFileUrl,
        Gender          = user.Gender,
        DateOfBirth     = user.DateOfBirth
    };


    public async Task<User?> FindByEmailAsync(string email)
    {
        var appUser = await _userManager.FindByEmailAsync(email);
        return appUser is null ? null : MapToDomain(appUser);
    }

    public async Task<User?> FindByUserNameAsync(string userName)
    {
        var appUser = await _userManager.FindByNameAsync(userName);
        return appUser is null ? null : MapToDomain(appUser);
    }

    public async Task<User?> FindByIdAsync(Guid id)
    {
        var appUser = await _userManager.FindByIdAsync(id.ToString());
        return appUser is null ? null : MapToDomain(appUser);
    }

    public async Task<bool> CreateUserAsync(User user, string password)
    {
        var appUser = MapToApplicationUser(user);
        var result = await _userManager.CreateAsync(appUser, password);
        return result.Succeeded;
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null) return false;

        appUser.UserName        = user.UserName;
        appUser.Email           = user.Email;
        appUser.FirstName       = user.FirstName;
        appUser.LastName        = user.LastName;
        appUser.PhoneNumber     = user.PhoneNumber;
        appUser.ProfilePhotoUrl = user.ProfilePhotoUrl;
        appUser.MedicalFileUrl  = user.MedicalFileUrl;
        appUser.Gender          = user.Gender;
        appUser.DateOfBirth     = user.DateOfBirth;

        var result = await _userManager.UpdateAsync(appUser);
        return result.Succeeded;
    }

    public async Task<bool> IsUserExistsAsync(Guid userId)
    {
        return await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId);
    }


    public async Task<IList<string>> GetUserRolesAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null) return new List<string>();
        return await _userManager.GetRolesAsync(appUser);
    }

    public async Task<bool> AddUserToRoleAsync(User user, string role)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null) return false;
        var result = await _userManager.AddToRoleAsync(appUser, role);
        return result.Succeeded;
    }


    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null) return false;
        return await _userManager.CheckPasswordAsync(appUser, password);
    }

    public async Task<bool> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null) return false;
        var result = await _userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> ResetPasswordAsync(User user, string token, string newPassword)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null) return false;
        var result = await _userManager.ResetPasswordAsync(appUser, token, newPassword);
        return result.Succeeded;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null) return null;
        return await _userManager.GeneratePasswordResetTokenAsync(appUser);
    }


    public async Task<string?> GenerateEmailConfirmationTokenAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null) return null;
        return await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
    }

    public async Task<bool> VerifyConfirmaionEmailAsync(User user, string token)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null) return false;
        var result = await _userManager.ConfirmEmailAsync(appUser, token);
        return result.Succeeded;
    }


    public async Task<bool> IsLockedOutAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        return appUser is not null && await _userManager.IsLockedOutAsync(appUser);
    }

    public async Task<DateTime?> GetLockoutEndDateAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        return appUser?.LockoutEnd?.UtcDateTime;
    }

    public async Task IncrementAccessFailedCountAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is not null)
            await _userManager.AccessFailedAsync(appUser);
    }

    public async Task ResetAccessFailedCountAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is not null)
            await _userManager.ResetAccessFailedCountAsync(appUser);
    }

    public async Task<int> GetAccessFailedCountAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        return appUser?.AccessFailedCount ?? 0;
    }

    public Task<int> GetMaxFailedAccessAttemptsAsync()
        => Task.FromResult(_userManager.Options.Lockout.MaxFailedAccessAttempts);


    public async Task<bool> IsTwoFactorEnabledAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        return appUser is not null && await _userManager.GetTwoFactorEnabledAsync(appUser);
    }


    public async Task UpdateLastLoginAsync(User user, DateTime loginTime)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is null) return;
        appUser.LastLoginAt = loginTime;
        await _userManager.UpdateAsync(appUser);
    }


    public async Task<string> GenerateAndStoreRefreshTokenAsync(
        Guid userId, string clientId, string userAgent, string ipAddress)
    {
        await RevokeAllRefreshTokensAsync(userId, clientId, userAgent, ipAddress);

        var refreshToken = new RefreshToken
        {
            Id          = Guid.NewGuid(),
            UserId      = userId,
            ClientId    = clientId,
            UserAgent   = userAgent,
            Token       = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            CreatedAt   = DateTime.UtcNow,
            ExpiresAt   = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return refreshToken.Token;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        => await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string ipAddress)
    {
        refreshToken.RevokedAt   = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        await _dbContext.SaveChangesAsync();
    }

    private async Task RevokeAllRefreshTokensAsync(
        Guid userId, string clientId, string userAgent, string revokedByIp)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId    == userId
                     && t.ClientId  == clientId
                     && t.UserAgent == userAgent
                     && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt   = DateTime.UtcNow;
            token.RevokedByIp = revokedByIp;
        }

        await _dbContext.SaveChangesAsync();
    }


    public async Task<bool> IsValidClientAsync(string clientId)
        => await _dbContext.Clients.AnyAsync(c => c.ClientId == clientId && c.IsActive);


    public async Task<MedicalFile?> AddOrUpdateMedicalFileAsync(MedicalFile medicalFile)
    {
        var existing = await _dbContext.MedicalFiles
            .FirstOrDefaultAsync(mf => mf.UserId == medicalFile.UserId);

        if (existing is not null)
        {
            existing.FileName    = medicalFile.FileName;
            existing.FilePath    = medicalFile.FilePath;
            existing.ContentType = medicalFile.ContentType;
            existing.FileSize    = medicalFile.FileSize;
            existing.UploadedAt  = medicalFile.UploadedAt;
            _dbContext.MedicalFiles.Update(existing);
        }
        else
        {
            await _dbContext.MedicalFiles.AddAsync(medicalFile);
        }

        await _dbContext.SaveChangesAsync();
        return existing ?? medicalFile;
    }

    public async Task<MedicalFile?> GetMedicalFileByUserIdAsync(Guid userId)
        => await _dbContext.MedicalFiles.FirstOrDefaultAsync(mf => mf.UserId == userId);
}