using System.Text;
using System.Text.Json.Serialization;
using Clinic.Api.Auth;
using Clinic.Api.Middlleware;
using Clinic.Application.Abstractions;
using Clinic.Application.Services;
using Clinic.Application.Validators;
using Clinic.Infrastructure.Identity;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Persistence.Seed;
using Clinic.Infrastructure.Persistence.Repositories;
using Clinic.Infrastructure.Time;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Infrastructure
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddHttpContextAccessor();

// Application
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IQueueService, QueueService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthStore, AuthStore>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICsvExportService, CsvExportService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePatientValidator>();

// Identity (password rules match the staff-account validator: 10+ chars, upper, lower, digit)
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 10;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddRoles<IdentityRole<string>>()
    .AddEntityFrameworkStores<ClinicDbContext>();

// Authentication (JWT) + authorization
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<JwtTokenService>();

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be set and at least 32 characters long.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization(Policies.Register);

// CORS: the Angular dev server (calls also go through the dev proxy, which makes them same-origin).
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                     ?? new[] { "http://localhost:4200" })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("Content-Disposition")));

// API
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Send and accept enums as strings, e.g. "Male" instead of 1
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

var app = builder.Build();

// Demo data (doctors, schedules, patients, staff accounts, appointments) - development only.
if (app.Environment.IsDevelopment())
{
    await DataSeeder.SeedAsync(app.Services);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                 // /openapi/v1.json
    app.MapScalarApiReference();      // /scalar/v1
}

app.UseRequestLogging();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();        // in development the Angular proxy calls plain http
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
