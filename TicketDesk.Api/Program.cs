
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicketDesk.Api.Data;
using TicketDesk.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CORS
// ========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ========================================
// BASE DE DATOS
// ========================================

builder.Services.AddDbContext<TicketDeskDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// ========================================
// SERVICIOS
// ========================================

builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<TicketStatusService>();

// ========================================
// CORREO ELECTRÓNICO (GMAIL SMTP)
// ========================================

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email:Smtp")
);

builder.Services.AddScoped<IEmailService, EmailService>();

// ========================================
// AUTENTICACIÓN JWT
// ========================================

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "La clave JWT no está configurada."
    );

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        ),

        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        ValidateAudience = true,
        ValidAudience = jwtAudience,

        ValidateLifetime = true,

        ClockSkew = TimeSpan.Zero
    };
});

// ========================================
// CONTROLLERS Y OPENAPI
// ========================================

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ========================================
// OPENAPI
// ========================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ========================================
// MIDDLEWARE
// ========================================

app.UseHttpsRedirection();

// CORS
app.UseCors("ReactPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();