using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using TimeTrack.Api.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var authOptions = ConfigureAuthOptions();
builder.Services.AddAuthorization();
ConfigureAuthentication(authOptions);


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "CorsPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("CorsPolicy"); //Do not enable this policy outside development unless you remove localhost as a trusted origin
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

AuthOptions ConfigureAuthOptions()
{
    var authOptions = new AuthOptions();
    var authOptionsSection = builder.Configuration.GetRequiredSection(nameof(AuthOptions));
    authOptionsSection.Bind(authOptions);
    if (authOptions == null) throw new ArgumentNullException(nameof(authOptions));
    builder.Services.Configure<AuthOptions>(authOptionsSection);
    return authOptions;
}

void ConfigureAuthentication(AuthOptions authOptions)
{
    builder.Services.AddAuthentication("Bearer")
    .AddCookie(x =>
    {
        x.Cookie.Name = "token";
    })
    .AddJwtBearer(options =>
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(authOptions.PublicKey);
        var signingCredentials = new RsaSecurityKey(rsa);

        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = authOptions.JwtAudience,
            ValidIssuer = authOptions.JwtIssuer,
            IssuerSigningKey = signingCredentials
        };
        options.Events = new JwtBearerEvents()
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["token"];
                return Task.CompletedTask;
            }
        };
    });
}
