using Microsoft.IdentityModel.Tokens;
using Serilog;
using Sphere.Shared;

// Setting this allows us to get some benefits all over the place.
Services.Current = Services.Administration;

Log.Logger = SphericalLogger.StartupLogger(Services.Current);

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog(SphericalLogger.SetupLogger);
    builder.Services.AddInjectableOrleansClient();
    builder.Services.AddHealthChecks();

    // Add services to the container.

    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            // TODO: get this from right spot.
            //options.Authority = Services.Auth.Address;

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
    app.MapHealthChecks(Constants.HealthCheckEndpoint);

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
    Log.Information("Shutting down");
    Log.CloseAndFlush();
}
