using System.Text;
using System.Text.Json.Serialization;
using Api.Auth;
using Api.Json;
using Api.Middleware;
using Aplicacion.Servicios;
using Dominio.Servicios;
using Infraestructura;
using Infraestructura.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=mesasitec.db";

builder.Services.AddInfraestructura(connectionString);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = ctx =>
        {
            var errores = ctx.ModelState
                .Where(kv => kv.Value?.Errors.Count > 0)
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            var problema = ErrorFactory.Crear(
                "VALIDACION", "Los datos enviados no son válidos.", errores);

            return new ObjectResult(problema) { StatusCode = problema.Status };
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeNullableJsonConverter());
    });

builder.Services.AddSingleton<StateMachineService>();
builder.Services.AddSingleton<SlaCalculator>();
builder.Services.AddSingleton<CodigoFormateador>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<SolicitudService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SolicitudMapper>();
builder.Services.AddScoped<JwtTokenFactory>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? "MesaSitec-Dev-Secret-Para-Prueba-Tecnica-2026!";

if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        "La variable de entorno Jwt__Secret debe tener al menos 32 caracteres (HS256).");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MesaSitecApi",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MesaSitecClients",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.Response.ContentType = "application/problem+json";
                return ctx.Response.WriteAsJsonAsync(
                    ErrorFactory.Crear("NO_AUTENTICADO", "El token es inválido, está ausente o ha expirado."));
            },
            OnForbidden = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                ctx.Response.ContentType = "application/problem+json";
                return ctx.Response.WriteAsJsonAsync(
                    ErrorFactory.Crear("OPERACION_NO_PERMITIDA", "No tiene permisos para realizar esta operación."));
            },
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MesaSitec API",
        Version = "v1",
        Description = "Mesa de servicio multi-tenant (prueba técnica).",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega aquí tu token JWT (sin el prefijo 'Bearer').",
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MesaSitecDbContext>();
    db.Database.Migrate();

    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { estado = "ok" }));

app.Run();
