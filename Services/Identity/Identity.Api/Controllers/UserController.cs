using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Shared.Wrappers;
using UAParser;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService _userService) : ControllerBase
{

   
    /// <summary> Registers a new member. Send as multipart/form-data to include an optional medical file.</summary>
   

    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Register([FromForm] RegisterDTO dto)
    {
        try
        {
            var userId = await _userService.RegisterAsync(dto);
            if (userId == null)
                return BadRequest(Response<string>.FailResponse("Registration failed. Email or username might already exist."));

            return Ok(Response<string>.SuccessResponse(userId.Value.ToString(), "User registered successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Response<string>.FailResponse("Error during registration.", new List<string> { ex.Message }));
        }
    }



    /// <summary> Sends a confirmation email to the user.</summary>


    [HttpPost("send-confirmation-email")]
    [ProducesResponseType(typeof(Response<EmailConfirmationTokenResponseDTO>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> SendConfirmationEmail([FromBody] EmailDTO dto)
    {
        try
        {
            var emailTokenResponse = await _userService.SendConfirmationEmailAsync(dto.Email);
            if (emailTokenResponse == null)
                return NotFound(Response<string>.FailResponse("User with this email not found"));

            return Ok(Response<EmailConfirmationTokenResponseDTO>.SuccessResponse(emailTokenResponse, "Email confirmation token generated successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Response<string>.FailResponse("Error generating confirmation token.", new List<string> { ex.Message }));
        }
    }


    /// <summary> Verifies the email confirmation token.</summary>
    
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(Response<string>), 200)]
    [ProducesResponseType(typeof(Response<string>), 400)]
    public async Task<IActionResult> VerifyConfirmationEmailAsync([FromBody] ConfirmEmailDTO dto)
    {
        try
        {
            var success = await _userService.VerifyConfirmationEmailAsync(dto);
            if (!success)
                return BadRequest(Response<string>.FailResponse("Invalid confirmation token or user."));

            return Ok(Response<string>.SuccessResponse("Email confirmed successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Response<string>.FailResponse("Error confirming email.", new List<string> { ex.Message }));
        }
    }


    /// <summary> Logs in a user.</summary>
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var userAgent = GetNormalizedUserAgent();
        var loginResponse = await _userService.LoginAsync(dto, ipAddress, userAgent);

        if (!string.IsNullOrEmpty(loginResponse.ErrorMessage))
        {
            loginResponse.Succeeded = false;
            return Unauthorized(Response<LoginResponseDTO>.FailResponse(loginResponse.ErrorMessage, errors: null, data: loginResponse));
        }

        loginResponse.Succeeded = true;
        return Ok(Response<LoginResponseDTO>.SuccessResponse(loginResponse,
            loginResponse.RequiresTwoFactor ? "Two-factor authentication required." : "Login successful."));
    }


    /// <summary> Refreshes the access token.</summary>
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO dto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var userAgent = GetNormalizedUserAgent();

        var refreshTokenResponse = await _userService.RefreshTokenAsync(dto, ipAddress, userAgent);

        if (!string.IsNullOrEmpty(refreshTokenResponse.ErrorMessage))
            return Unauthorized(Response<string>.FailResponse(refreshTokenResponse.ErrorMessage));

        return Ok(Response<RefreshTokenResponseDTO>.SuccessResponse(refreshTokenResponse, "Token refreshed successfully."));
    }

    /// <summary> Revokes a refresh token.</summary>
    
    [HttpPost("revoke-token")]
    [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDTO dto)
    {
        try
        {
            var success = await _userService.RevokeRefreshTokenAsync(dto.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");
            if (!success)
                return BadRequest(Response<string>.FailResponse("Invalid token or token already revoked."));

            return Ok(Response<string>.SuccessResponse("Token revoked successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Response<string>.FailResponse("Error revoking token.", new List<string> { ex.Message }));
        }
    }


    /// <summary> Initiates the password recovery process.</summary>
    
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] EmailDTO dto)
    {
        try
        {
            var forgotPassword = await _userService.ForgotPasswordAsync(dto.Email);
            if (forgotPassword == null)
                return NotFound(Response<string>.FailResponse("Email not found."));

            return Ok(Response<ForgotPasswordResponseDTO>.SuccessResponse(forgotPassword, "Password reset token sent to email."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Response<string>.FailResponse("Error in forgot password process.", new List<string> { ex.Message }));
        }
    }

    /// <summary> Resets the user password.</summary>
    
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(Response<string>), 200)]
    [ProducesResponseType(typeof(Response<string>), 400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
    {
        try
        {
            var success = await _userService.ResetPasswordAsync(dto.UserId, dto.Token, dto.NewPassword);
            if (!success)
                return BadRequest(Response<string>.FailResponse("Invalid token or user."));

            return Ok(Response<string>.SuccessResponse("Password reset successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Response<string>.FailResponse("Error resetting password.", new List<string> { ex.Message }));
        }
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(Response<string>), 200)]
    [ProducesResponseType(typeof(Response<string>), 400)]
    [ProducesResponseType(typeof(Response<string>), 401)]
    /// <summary> Changes the authenticated user's password.</summary>
    
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(Response<string>.FailResponse("Invalid user token."));

            var success = await _userService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
            if (!success)
                return BadRequest(Response<string>.FailResponse("Password change failed."));

            return Ok(Response<string>.SuccessResponse("Password changed successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Response<string>.FailResponse("Error changing password.", new List<string> { ex.Message }));
        }
    }


    /// <summary> Gets the user profile.</summary>
    
    [HttpGet("profile/{userId}")]
    [ProducesResponseType(typeof(Response<ProfileDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        try
        {
            var profile = await _userService.GetProfileAsync(userId);
            if (profile == null)
                return NotFound(Response<string>.FailResponse("User profile not found."));

            return Ok(Response<ProfileDTO>.SuccessResponse(profile));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Response<string>.FailResponse("Error fetching profile.", new List<string> { ex.Message }));
        }
    }

    /// <summary> Updates the user profile.</summary>
    
    [HttpPut("profile")]
    [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
    {
        try
        {
            var success = await _userService.UpdateProfileAsync(dto);
            if (!success)
                return BadRequest(Response<string>.FailResponse("Failed to update profile."));

            return Ok(Response<string>.SuccessResponse("Profile updated successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Response<string>.FailResponse("Error updating profile.", new List<string> { ex.Message }));
        }
    }

    /// <summary> Checks if the user exists.</summary>
    
    [HttpGet("{userId}/exists")]
    public async Task<IActionResult> UserExists(Guid userId)
    {
        bool exists = await _userService.IsUserExistsAsync(userId);
        return Ok(new Response<bool>
        {
            Success = true,
            Data = exists,
            Message = exists ? "User exists." : "User does not exist."
        });
    }


    /// <summary>
    /// Uploads or replaces the medical file for a user.
    /// Send as multipart/form-data with fields: UserId (Guid) and File (IFormFile).
    /// The file is stored under wwwroot/medical-files and served as a public static file.
    /// </summary>
    
    [HttpPost("medical-file")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Response<MedicalFileDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UploadMedicalFile([FromForm] UploadMedicalFileDTO dto)
    {
        try
        {
            var result = await _userService.UploadMedicalFileAsync(dto);
            if (result == null)
                return BadRequest(Response<string>.FailResponse("Failed to upload medical file."));

            return Ok(Response<MedicalFileDTO>.SuccessResponse(result, "Medical file uploaded successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Response<string>.FailResponse("Error uploading medical file.", new List<string> { ex.Message }));
        }
    }

    /// <summary> Gets the medical file record for a user. </summary>
    
    [HttpGet("{userId}/medical-file")]
    [ProducesResponseType(typeof(Response<MedicalFileDTO>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetMedicalFile(Guid userId)
    {
        try
        {
            var file = await _userService.GetMedicalFileAsync(userId);
            if (file == null)
                return NotFound(Response<string>.FailResponse("Medical file not found."));

            return Ok(Response<MedicalFileDTO>.SuccessResponse(file));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Response<string>.FailResponse("Error fetching medical file.", new List<string> { ex.Message }));
        }
    }


    private string GetNormalizedUserAgent()
    {
        var userAgentRaw = HttpContext.Request.Headers["User-Agent"].ToString();
        if (string.IsNullOrWhiteSpace(userAgentRaw))
            return "Unknown";

        try
        {
            var uaParser = Parser.GetDefault();
            var clientInfo = uaParser.Parse(userAgentRaw);
            var browser = clientInfo.UA.Family ?? "UnknownBrowser";
            var browserVersion = clientInfo.UA.Major ?? "0";
            var os = clientInfo.OS.Family ?? "UnknownOS";
            return $"{browser}-{browserVersion}_{os}";
        }
        catch
        {
            return "Unknown";
        }
    }
}