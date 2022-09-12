using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using SimpleBankAPI.Application.Interfaces;
using SimpleBankAPI.Application.Usecases;
using SimpleBankAPI.Infrastructure.Adapters.Providers;
using SimpleBankAPI.Infrastructure.Ports.Providers;
using SimpleBankAPI.Infrastructure.Ports.Repositories;
using SimpleBankAPI.Infrastructure.Adapters.Repositories.Database;
using SimpleBankAPI.WebApi.Validators;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddSingleton<IAuthenticationProvider, AuthenticationProvider>();
//builder.Services.AddSingleton<IConfiguration>(Configuration);
//builder.Services.AddSingleton<NpgsqlConnection>();
//builder.Services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDbConnection>((s) => new NpgsqlConnection(builder.Configuration.GetConnectionString("BankDB")));
builder.Services.AddScoped<IDbTransaction>(s =>
{
    NpgsqlConnection connection = (NpgsqlConnection)s.GetRequiredService<IDbConnection>();
    connection.Open();
    return connection.BeginTransaction();
});
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IAccountRepository, AccountRepository>();
builder.Services.AddTransient<ISessionRepository, SessionRepository>();
builder.Services.AddTransient<IMovementRepository, MovementRepository>();

builder.Services.AddScoped<ISessionUseCase, SessionUseCase>();
builder.Services.AddScoped<IUserUseCase, UserUseCase>();
builder.Services.AddScoped<IAccountUseCase, AccountUseCase>();
builder.Services.AddScoped<ITransferUseCase, TransferUseCase>();

//builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddFluentValidationClientsideAdapters();
//builder.Services.AddScoped<IValidator<registerRequest>, RegisterValidator>();

builder.Services.AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<RegisterValidator>());
builder.Services.AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<LoginValidator>());
builder.Services.AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<RenewLoginValidator>());
builder.Services.AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<TransferValidator>());
builder.Services.AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<CreateAccountValidator>());

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

/*
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var repository = services.GetRequiredService<IUserRepository>();
        repository.ReadAll();
    }
    catch (Exception ex)
    {
        
    }
}
*/
app.Run();

