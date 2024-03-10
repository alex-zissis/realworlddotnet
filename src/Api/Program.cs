var builder = WebApplication.CreateBuilder(args);

// add logging
builder.Host.UseSerilog((hostBuilderContext, services, loggerConfiguration) =>
{
    loggerConfiguration.ConfigureBaseLogging("realworldDotnet");
    loggerConfiguration.AddApplicationInsightsLogging(services, hostBuilderContext.Configuration);
});

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SupportNonNullableReferenceTypes();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "realworlddotnet", Version = "v1" });
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressInferBindingSourcesForParameters = true;
});

builder.Services.AddScoped<IConduitRepository, ConduitRepository>();
builder.Services.AddScoped<IUserHandler, UserHandler>();
builder.Services.AddScoped<IArticlesHandler, ArticlesHandler>();
builder.Services.AddScoped<IProfilesHandler, ProfilesHandler>();
builder.Services.AddSingleton<CertificateProvider>();

builder.Services.AddSingleton<ITokenGenerator>(container =>
{
    var logger = container.GetRequiredService<ILogger<CertificateProvider>>();
    var certificateProvider = new CertificateProvider(logger);
    var cert = certificateProvider.LoadFromFile("certificate.pfx", "password");

    return new TokenGenerator(cert);
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<ILogger<CertificateProvider>>((o, logger) =>
    {
        var certificateProvider = new CertificateProvider(logger);
        var cert = certificateProvider.LoadFromFile("certificate.pfx", "password");

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            IssuerSigningKey = new RsaSecurityKey(cert.GetRSAPublicKey())
        };
        o.Events = new JwtBearerEvents { OnMessageReceived = CustomOnMessageReceivedHandler.OnMessageReceived };
    });

builder.Services.AddDbContext<ConduitContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConduitContext"), b => b.MigrationsAssembly("Data")));

ProblemDetailsExtensions.AddProblemDetails(builder.Services);
builder.Services.ConfigureOptions<ProblemDetailsLogging>();

var app = builder.Build();

// Configure the HTTP request pipeline.
Log.Information("Start configuring http request pipeline");

// when using in memory SQLite ensure the tables are created
using (var scope = app.Services.CreateScope())
{
    using var context = scope.ServiceProvider.GetService<ConduitContext>();
    context?.Database.EnsureCreated();
}

app.UseSerilogRequestLogging();
app.UseProblemDetails();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "realworlddotnet v1"));

try
{
    Log.Information("Starting web host");
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
    Thread.Sleep(2000);
}
