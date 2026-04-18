using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Application.Repositories;
using Identity.Application.Interfaces;
using MassTransit;
using Shared.Events;
using Microsoft.AspNetCore.Http;

namespace Identity.Application.Services;

public class UserService(IUserRepository _userRepository, ITokenService _tokenService, IPublishEndpoint _publishEndpoint) : IUserService
{
    private const string MEDICAL_FILE_FOLDER = "medical-files";
    private const string PROFILE_PHOTO_FOLDER = "profile-photos";


    public async Task<Guid?> RegisterAsync(RegisterDTO dto)
    {
        if (await _userRepository.FindByEmailAsync(dto.Email) != null)
            return null;

        if (await _userRepository.FindByUserNameAsync(dto.UserName) != null)
            return null;

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsEmailConfirmed = false,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth
        };

        var created = await _userRepository.CreateUserAsync(user, dto.Password);
        if (!created)
            return null;

        await _userRepository.AddUserToRoleAsync(user, "Member");

        // Save profile photo if provided
        if (dto.ProfilePicture != null && dto.ProfilePicture.Length > 0)
        {
            user.ProfilePhotoUrl = await UploadProfilePhotoInternalAsync(user.Id, dto.ProfilePicture);
            await _userRepository.UpdateUserAsync(user);
        }

        // Save medical file if provided
        if (dto.MedicalFile != null && dto.MedicalFile.Length > 0)
        {
            await UploadMedicalFileAsync(new UploadMedicalFileDTO
            {
                UserId = user.Id,
                File = dto.MedicalFile
            });
        }
        await _publishEndpoint.Publish(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            RegisteredAt = DateTime.UtcNow
        });


        return user.Id;
    }

    private async Task<string?> UploadProfilePhotoInternalAsync(Guid userId, IFormFile file)
    {
        var userFolder = Path.Combine("wwwroot", PROFILE_PHOTO_FOLDER, userId.ToString());
        Directory.CreateDirectory(userFolder);

        var extension = Path.GetExtension(file.FileName);
        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var physicalPath = Path.Combine(userFolder, safeFileName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/{PROFILE_PHOTO_FOLDER}/{userId}/{safeFileName}";
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginDTO dto, string ipAddress, string userAgent)
    {
        var response = new LoginResponseDTO();

        if (!await _userRepository.IsValidClientAsync(dto.ClientId))
        {
            response.ErrorMessage = "Invalid client ID.";
            return response;
        }

        var user = dto.EmailOrUserName.Contains('@')
            ? await _userRepository.FindByEmailAsync(dto.EmailOrUserName)
            : await _userRepository.FindByUserNameAsync(dto.EmailOrUserName);

        if (user == null)
        {
            response.ErrorMessage = "Invalid username or password.";
            return response;
        }

        if (await _userRepository.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userRepository.GetLockoutEndDateAsync(user);
            if (lockoutEnd.HasValue && lockoutEnd > DateTime.UtcNow)
            {
                var timeLeft = lockoutEnd.Value - DateTime.UtcNow;
                response.ErrorMessage = $"Account is locked. Try again after {timeLeft.Minutes} minute(s) and {timeLeft.Seconds} second(s).";
                response.RemainingAttempts = 0;
                return response;
            }
            else
            {
                await _userRepository.ResetAccessFailedCountAsync(user);
            }
        }

        if (!user.IsEmailConfirmed)
        {
            response.ErrorMessage = "Email not confirmed. Please verify your email.";
            return response;
        }

        var passwordValid = await _userRepository.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
        {
            await _userRepository.IncrementAccessFailedCountAsync(user);

            if (await _userRepository.IsLockedOutAsync(user))
            {
                response.ErrorMessage = "Account locked due to multiple failed login attempts.";
                response.RemainingAttempts = 0;
                return response;
            }

            var maxAttempts = await _userRepository.GetMaxFailedAccessAttemptsAsync();
            var failedCount = await _userRepository.GetAccessFailedCountAsync(user);
            var attemptsLeft = maxAttempts - failedCount;

            response.ErrorMessage = "Invalid username or password.";
            response.RemainingAttempts = attemptsLeft > 0 ? attemptsLeft : 0;
            return response;
        }

        await _userRepository.ResetAccessFailedCountAsync(user);

        if (await _userRepository.IsTwoFactorEnabledAsync(user))
        {
            response.RequiresTwoFactor = true;
            return response;
        }

        await _userRepository.UpdateLastLoginAsync(user, DateTime.UtcNow);

        var roles = await _userRepository.GetUserRolesAsync(user);

        response.Token = _tokenService.GenerateAccessToken(user, roles, dto.ClientId);
        response.RefreshToken = await _userRepository.GenerateAndStoreRefreshTokenAsync(user.Id, dto.ClientId, userAgent, ipAddress);

        return response;
    }

    public async Task<EmailConfirmationTokenResponseDTO?> SendConfirmationEmailAsync(string email)
    {
        var user = await _userRepository.FindByEmailAsync(email);
        if (user == null)
            return null;

        var token = await _userRepository.GenerateEmailConfirmationTokenAsync(user);
        if (token == null)
            return null;

        return new EmailConfirmationTokenResponseDTO
        {
            UserId = user.Id,
            Token = token
        };
    }

    public async Task<bool> VerifyConfirmationEmailAsync(ConfirmEmailDTO dto)
    {
        var user = await _userRepository.FindByIdAsync(dto.UserId);
        if (user == null)
            return false;

        var result = await _userRepository.VerifyConfirmaionEmailAsync(user, dto.Token);
        if (result)
        {
            user.IsActive = true;
            await _userRepository.UpdateUserAsync(user);
        }
        return result;
    }

    public async Task<RefreshTokenResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO dto, string ipAddress, string userAgent)
    {
        var response = new RefreshTokenResponseDTO();

        if (!await _userRepository.IsValidClientAsync(dto.ClientId))
        {
            response.ErrorMessage = "Invalid client ID.";
            return response;
        }

        var refreshTokenEntity = await _userRepository.GetRefreshTokenAsync(dto.RefreshToken);
        if (refreshTokenEntity == null || !refreshTokenEntity.IsActive)
        {
            response.ErrorMessage = "Invalid or expired refresh token.";
            return response;
        }

        var newRefreshToken = await _userRepository.GenerateAndStoreRefreshTokenAsync(
            refreshTokenEntity.UserId, dto.ClientId, userAgent, ipAddress);

        var user = await _userRepository.FindByIdAsync(refreshTokenEntity.UserId);
        if (user == null)
        {
            response.ErrorMessage = "User not found.";
            return response;
        }

        var roles = await _userRepository.GetUserRolesAsync(user);

        response.Token = _tokenService.GenerateAccessToken(user, roles, dto.ClientId);
        response.RefreshToken = newRefreshToken;

        return response;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress)
    {
        var refreshToken = await _userRepository.GetRefreshTokenAsync(token);
        if (refreshToken == null || !refreshToken.IsActive)
            return false;

        await _userRepository.RevokeRefreshTokenAsync(refreshToken, ipAddress);
        return true;
    }

    public async Task<ForgotPasswordResponseDTO?> ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.FindByEmailAsync(email);
        if (user == null)
            return null;

        var token = await _userRepository.GeneratePasswordResetTokenAsync(user);
        if (token == null)
            return null;

        return new ForgotPasswordResponseDTO
        {
            UserId = user.Id,
            Token = token
        };
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        if (user == null)
            return false;

        return await _userRepository.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<bool> ResetPasswordAsync(Guid userId, string token, string newPassword)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        if (user == null)
            return false;

        return await _userRepository.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<ProfileDTO?> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        if (user == null)
            return null;

        return new ProfileDTO
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Email = user.Email,
            LastLoginAt = user.LastLoginAt,
            UserName = user.UserName
        };
    }

    public async Task<bool> UpdateProfileAsync(UpdateProfileDTO dto)
    {
        var user = await _userRepository.FindByIdAsync(dto.UserId);
        if (user == null)
            return false;

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Gender = dto.Gender;
        user.DateOfBirth = dto.DateOfBirth;

        if (dto.ProfilePicture != null && dto.ProfilePicture.Length > 0)
        {
            // Delete old photo if exists
            if (!string.IsNullOrEmpty(user.ProfilePhotoUrl))
            {
                var oldPhysicalPath = Path.Combine("wwwroot", user.ProfilePhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPhysicalPath))
                {
                    System.IO.File.Delete(oldPhysicalPath);
                }
            }

            user.ProfilePhotoUrl = await UploadProfilePhotoInternalAsync(user.Id, dto.ProfilePicture);
        }

        return await _userRepository.UpdateUserAsync(user);
    }

    public async Task<bool> IsUserExistsAsync(Guid userId)
    {
        return await _userRepository.IsUserExistsAsync(userId);
    }


    public async Task<MedicalFileDTO?> UploadMedicalFileAsync(UploadMedicalFileDTO dto)
    {
        if (dto.File == null || dto.File.Length == 0)
            return null;

        var userFolder = Path.Combine("wwwroot", MEDICAL_FILE_FOLDER, dto.UserId.ToString());
        Directory.CreateDirectory(userFolder);

        var extension = Path.GetExtension(dto.File.FileName);
        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var physicalPath = Path.Combine(userFolder, safeFileName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        var relativeUrl = $"/{MEDICAL_FILE_FOLDER}/{dto.UserId}/{safeFileName}";

        var entity = new MedicalFile
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            FileName = dto.File.FileName,
            FilePath = relativeUrl,
            ContentType = dto.File.ContentType,
            FileSize = dto.File.Length,
            UploadedAt = DateTime.UtcNow
        };

        var saved = await _userRepository.AddOrUpdateMedicalFileAsync(entity);
        if (saved == null)
            return null;

        return MapToDto(saved);
    }

    public async Task<MedicalFileDTO?> GetMedicalFileAsync(Guid userId)
    {
        var file = await _userRepository.GetMedicalFileByUserIdAsync(userId);
        return file == null ? null : MapToDto(file);
    }

    private static MedicalFileDTO MapToDto(MedicalFile f) => new()
    {
        Id = f.Id,
        UserId = f.UserId,
        FileName = f.FileName,
        FileUrl = f.FilePath,
        ContentType = f.ContentType,
        FileSize = f.FileSize,
        UploadedAt = f.UploadedAt
    };
}
