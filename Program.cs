using Authentication.Mfa.Twilio.Data;
using Authentication.Mfa.Twilio.Data.Entities;
using Authentication.Mfa.Twilio.Extensions;
using Authentication.Mfa.Twilio.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
        db.Users.Add(new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "demo",
            Email = "demo@example.com",
            PasswordHash = passwordHasher.HashPassword("Password123!"),
            IsMfaEnabled = false
        });
        db.SaveChanges();
    }
}

app.Run();
