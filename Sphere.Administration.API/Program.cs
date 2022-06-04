using Microsoft.IdentityModel.Tokens;
using Serilog;
using Sphere.Shared;

Log.Logger = SphericalLogger.SetupLogger();

Log.Information("Starting up");

var registration = Services.Administration.GetServiceRegistration();

try
{
    var result = await Services.RegisterService(registration);

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = Services.Auth.Address;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ApiScope", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("scope", "api1");
        });
    });


    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers().RequireAuthorization("ApiScope");

    app.Run();
}
catch (Exception ex)
{
    if (ex.GetType().Name != "StopTheHostException")
    {
        Log.Fatal(ex, "Unhandled exception");
    }
}
finally
{
    await Services.UnregisterService(registration);

    Log.Information("Shutting down");
    Log.CloseAndFlush();
}
