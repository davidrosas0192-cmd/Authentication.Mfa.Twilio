using Authentication.Mfa.Twilio.Data;
using Authentication.Mfa.Twilio.Data.Entities;
using Authentication.Mfa.Twilio.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Mfa.Twilio.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;

    public UserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
