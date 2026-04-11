using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Entities;
using Stoq.Identity.Domain.Enums;
using Stoq.Identity.Infrastructure.Entities;
using Stoq.Identity.Infrastructure.Persistence;

namespace Stoq.Identity.Infrastructure.Services;

internal sealed class IdentityServiceImpl(
    UserManager<ApplicationUser> userManager,
    IdentityModuleDbContext dbContext)
    : IIdentityService
{
    private static readonly TimeSpan _otpLifetime = TimeSpan.FromMinutes(15);

    public async Task<UserInfo?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return null;
        }

        return MapToUserInfo(user);
    }

    public async Task<UserInfo?> FindByIdAsync(Guid userId, CancellationToken ct = default)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return null;
        }

        return MapToUserInfo(user);
    }

    public async Task<UserInfo?> FindBySocialProviderAsync(string provider, string providerId, CancellationToken ct = default)
    {
        ApplicationUser? user = await userManager.FindByLoginAsync(provider, providerId);

        if (user is null)
        {
            return null;
        }

        return MapToUserInfo(user);
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken ct = default)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        return user is not null && await userManager.CheckPasswordAsync(user, password);
    }

    public async Task<(UserInfo? User, string[] Errors)> CreateUserAsync(
        string email,
        string password,
        string displayName,
        string preferredLanguage,
        CancellationToken ct = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = displayName,
            PreferredLanguage = preferredLanguage,
            Role = AppRole.HouseholdMember,
            CreatedAt = DateTimeOffset.UtcNow,
            EmailConfirmed = true, // Skip email confirmation for now.
        };

        IdentityResult result = await userManager.CreateAsync(user, password);
        UserInfo? userInfo = result.Succeeded ? MapToUserInfo(user) : null;

        return (userInfo, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Success, string[] Errors)> CreateSocialUserAsync(
        string email,
        string displayName,
        string provider,
        string providerId,
        CancellationToken ct = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = displayName,
            PreferredLanguage = "en",
            Role = AppRole.HouseholdMember,
            CreatedAt = DateTimeOffset.UtcNow,
            EmailConfirmed = true,
        };

        IdentityResult createResult = await userManager.CreateAsync(user);

        if (!createResult.Succeeded)
        {
            return (false, createResult.Errors.Select(e => e.Description).ToArray());
        }

        var loginInfo = new UserLoginInfo(provider, providerId, provider);
        IdentityResult loginResult = await userManager.AddLoginAsync(user, loginInfo);

        if (loginResult.Succeeded)
        {
            return (true, []);
        }

        return (false, loginResult.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<string> GeneratePasswordResetOtpAsync(Guid userId, CancellationToken ct = default)
    {
        ApplicationUser user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"User {userId} not found.");

        var otp = GenerateNumericOtp();
        user.PasswordResetOtpHash = HashOtp(otp);
        user.PasswordResetOtpExpiry = DateTimeOffset.UtcNow.Add(_otpLifetime);

        await userManager.UpdateAsync(user);

        return otp;
    }

    public async Task<bool> ResetPasswordAsync(Guid userId, string otp, string newPassword, CancellationToken ct = default)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.PasswordResetOtpHash is null || user.PasswordResetOtpExpiry is null)
        {
            return false;
        }

        if (DateTimeOffset.UtcNow > user.PasswordResetOtpExpiry)
        {
            return false;
        }

        if (!string.Equals(user.PasswordResetOtpHash, HashOtp(otp), StringComparison.Ordinal))
        {
            return false;
        }

        // Clear OTP before resetting password to prevent replay.
        user.PasswordResetOtpHash = null;
        user.PasswordResetOtpExpiry = null;
        await userManager.UpdateAsync(user);

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        IdentityResult result = await userManager.ResetPasswordAsync(user, token, newPassword);

        return result.Succeeded;
    }

    public async Task StoreRefreshTokenAsync(Guid userId, string token, DateTimeOffset expiry, CancellationToken ct = default)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiry,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default)
    {
        RefreshToken? refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.Token == token, ct);

        return refreshToken?.IsActive ?? false;
    }

    public async Task RevokeRefreshTokenAsync(string token, CancellationToken ct = default)
    {
        RefreshToken? refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);

        if (refreshToken is not null && refreshToken.RevokedAt is null)
        {
            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<HouseholdMembership> GetHouseholdMembershipAsync(Guid userId, CancellationToken ct = default)
    {
        HouseholdMember? member = await dbContext.HouseholdMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId, ct);

        if (member is null)
        {
            return new HouseholdMembership(null, null);
        }

        return new HouseholdMembership(member.HouseholdId, member.Role);
    }

    private static UserInfo MapToUserInfo(ApplicationUser user) =>
        new(user.Id, user.Email!, user.DisplayName, user.AvatarUrl, user.PreferredLanguage, user.Role);

    private static string GenerateNumericOtp()
    {
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string HashOtp(string otp)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(otp));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
