using Identity.Application.DTOs;

namespace Identity.Application.Interfaces;

public interface IUserService
{
    Task<Guid?> RegisterAsync(RegisterDTO dto);
    Task<LoginResponseDTO> LoginAsync(LoginDTO dto, string ipAddress, string userAgent);
    Task<EmailConfirmationTokenResponseDTO?> SendConfirmationEmailAsync(string email);
    Task<bool> VerifyConfirmationEmailAsync(ConfirmEmailDTO dto);
    Task<RefreshTokenResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO dto, string ipAddress, string userAgent);
    Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress);
    Task<ForgotPasswordResponseDTO?> ForgotPasswordAsync(string email);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> ResetPasswordAsync(Guid userId, string token, string newPassword);
    Task<ProfileDTO?> GetProfileAsync(Guid userId);
    Task<bool> UpdateProfileAsync(UpdateProfileDTO dto);
    Task<bool> IsUserExistsAsync(Guid userId);
    Task<MedicalFileDTO?> UploadMedicalFileAsync(UploadMedicalFileDTO dto);
    Task<MedicalFileDTO?> GetMedicalFileAsync(Guid userId);
    Task<bool> DeactivateUserAsync(Guid userId);
}