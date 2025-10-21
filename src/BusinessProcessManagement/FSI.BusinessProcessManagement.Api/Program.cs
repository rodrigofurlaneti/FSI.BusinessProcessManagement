using FSI.BusinessProcessManagement.Infrastructure;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.Application.Services;
using FSI.BusinessProcessManagement.Api.Filters;
using FSI.BusinessProcessManagement.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Swagger + JWT auth schema
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FSI BPM API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT no header. Ex: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id = "Bearer"}
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers(o => o.Filters.Add<ApiExceptionFilter>());

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IDepartmentAppService, DepartmentAppService>();
builder.Services.AddScoped<IUsuarioAppService, UsuarioAppService>();
builder.Services.AddScoped<IRoleAppService, RoleAppService>();
builder.Services.AddScoped<IUserRoleAppService, UserRoleAppService>();
builder.Services.AddScoped<IScreenAppService, ScreenAppService>();
builder.Services.AddScoped<IAuditLogAppService, AuditLogAppService>();
builder.Services.AddScoped<IProcessoBPMAppService, ProcessoBPMAppService>();
builder.Services.AddScoped<IProcessStepAppService, ProcessStepAppService>();
builder.Services.AddScoped<IProcessExecutionAppService, ProcessExecutionAppService>();
builder.Services.AddScoped<IRoleScreenPermissionAppService, RoleScreenPermissionAppService>();

// JWT Auth
var jwt = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = false;
        opt.SaveToken = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<ITokenService, TokenService>();

var app = builder.Build();

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
