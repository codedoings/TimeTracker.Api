using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using TimeTrack.Api.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var authOptions = new AuthOptions();
var authOptionsSection = builder.Configuration.GetRequiredSection(nameof(AuthOptions));
authOptionsSection.Bind(authOptions);
if (authOptions == null) throw new ArgumentNullException(nameof(authOptions));
builder.Services.Configure<AuthOptions>(authOptionsSection);

builder.Services.AddAuthorization();
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
            IssuerSigningKey =  signingCredentials
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

app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
