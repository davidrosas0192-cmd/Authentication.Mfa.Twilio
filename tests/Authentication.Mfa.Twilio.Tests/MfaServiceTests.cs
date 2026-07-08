using Authentication.Mfa.Twilio.Data;
using Authentication.Mfa.Twilio.Data.Entities;
using Authentication.Mfa.Twilio.DTOs;
using Authentication.Mfa.Twilio.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Authentication.Mfa.Twilio.Tests;

public class MfaServiceTests
{
    [Fact]
    public async Task StartEnrollmentChallengeAsync_ReturnsFailure_ForUnsupportedMethod()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        context.Users.Add(new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user1",
            Email = "user1@example.com",
            PasswordHash = "hash"
        });
        await context.SaveChangesAsync();

        var user = context.Users.First();
        var service = new MfaService(context);
        var request = new MfaEnrollStartRequest { Method = "push", Email = "user1@example.com" };

        var result = await service.StartEnrollmentChallengeAsync(user.Id, (MfaMethod)999, request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("unsupported_method", result.ErrorCode);
    }

    [Fact]
    public async Task StartEnrollmentChallengeAsync_ReturnsSuccess_ForSms()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user2",
            Email = "user2@example.com",
            PasswordHash = "hash"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new MfaService(context);
        var request = new MfaEnrollStartRequest { Method = "sms", PhoneNumber = "+15551234567" };

        var result = await service.StartEnrollmentChallengeAsync(user.Id, MfaMethod.Sms, request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(MfaMethod.Sms, result.Value!.Method);
    }

    [Fact]
    public async Task VerifyEnrollmentAsync_ReturnsFailure_ForInvalidCode()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user3",
            Email = "user3@example.com",
            PasswordHash = "hash"
        };
        context.Users.Add(user);
        context.MfaTransactions.Add(new MfaTransaction
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Purpose = "enrollment",
            Method = MfaMethod.Email,
            Target = "user3@example.com",
            Code = "123456",
            CodeHash = "hash",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new MfaService(context);
        var result = await service.VerifyEnrollmentAsync(user.Id, "000000", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("invalid_code", result.ErrorCode);
    }
}
