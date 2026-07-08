using Authentication.Mfa.Twilio.Common;
using Xunit;

namespace Authentication.Mfa.Twilio.Tests;

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        var result = Result.Success("ok");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("ok", result.Message);
    }

    [Fact]
    public void Failure_CreatesFailedResult()
    {
        var result = Result.Failure("bad", "invalid");

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("bad", result.Message);
        Assert.Equal("invalid", result.ErrorCode);
    }
}
