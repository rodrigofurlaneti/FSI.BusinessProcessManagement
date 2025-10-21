using FSI.BusinessProcessManagement.Infrastructure;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.Application.Services;
using Microsoft.OpenApi.Models;
using FSI.BusinessProcessManagement.Api.Filters;
using System.Text;
using FSI.BusinessProcessManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FSI BPM API",
        Version = "v1",
        Description = "API para Business Process Management (BPM)"
    });
    // Suporte a JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Ex: 'Bearer {seu token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// Controllers + filtros
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
});

// Infra (DbContext + Repos + UoW)
builder.Services.AddInfrastructure(builder.Configuration);

// Application (AppServices)
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

// ========= JWT Auth =========
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.RequireHttpsMetadata = false;
    opt.SaveToken = true;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSection["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

// Serviço para gerar tokens
builder.Services.AddSingleton<ITokenService, TokenService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ativar AuthN/AuthZ
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
