using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using SimpleBankAPI.Infrastructure.Providers;
using SimpleBankAPI.Infrastructure.Repositories;
using SimpleBankAPI.Infrastructure.Repositories.SqlDataAccess;
using SimpleBankAPI.Core.Usecases;
using System.Data;
using System.Text;
using SimpleBankAPI.WebApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<registerRequest>());
builder.Services.AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<loginRequest>());
builder.Services.AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<createAccountRequest>());
builder.Services.AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<transferRequest>());
builder.Services.AddSingleton<IAuthenticationProvider, AuthenticationProvider>();

//builder.Services.AddSingleton<IConfiguration>(Configuration);
//builder.Services.AddSingleton<NpgsqlConnection>();
//builder.Services.AddSingleton<IDbConnection, DbConnection>();
//builder.Services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDbConnection>((s) => new NpgsqlConnection(builder.Configuration.GetConnectionString("BankDB")));
builder.Services.AddScoped<IDbTransaction>(s =>
{
    //NpgsqlConnection connection = new(builder.Configuration.GetConnectionString("BankDB"));
    NpgsqlConnection connection = (NpgsqlConnection)s.GetRequiredService<IDbConnection>();
    connection.Open();
    return connection.BeginTransaction();
});
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IMovementRepository, MovementRepository>();
builder.Services.AddScoped<IUserUseCase, UserUseCase>();
builder.Services.AddScoped<IAccountUseCase, AccountUseCase>();
builder.Services.AddScoped<ITransferUseCase, TransferUseCase>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddAuthorization();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Cache:Redis"];
    options.InstanceName = "SimpleBankAPI";
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});
builder.Services.AddEndpointsApiExplorer();

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

app.MapControllers();

app.Run();

