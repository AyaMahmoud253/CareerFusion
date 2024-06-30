using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Web_API.helpers;
using Web_API.Models;
using Web_API.services;
using Web_API.Services;
using OfficeOpenXml;
using Web_API.Settings;
using Web_API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Accessing configuration
var configuration = builder.Configuration;
var jwtSettings = new JWT();
configuration.GetSection("JWT").Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000") // Include your AppUrl here
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});


// Add DbContext and Identity
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSignalR();

// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = configuration["JWT:Issuer"] ?? throw new ArgumentNullException("JWT:Issuer"),
        ValidAudience = configuration["JWT:Audience"] ?? throw new ArgumentNullException("JWT:Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"] ?? throw new ArgumentNullException("JWT:Key")))
    };
});

// Add other scoped services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHiringTimelineService, HiringTimelineService>();
builder.Services.AddScoped<IJobFormService, JobFormService>();
builder.Services.AddScoped<IJobSearchService, JobSearchService>();
builder.Services.AddScoped<IJobFormCVService, JobFormCVService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IPictureUploadService, PictureUploadService>();
builder.Services.AddScoped<ICVUploadService, CVUploadService>();
builder.Services.AddScoped<ITelephoneQuestionsService,TelephoneQuestionsService>();
// Add services to the container
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();





builder.Services.AddTransient<IMailService, SendGridMailService>();



builder.Services.AddRazorPages();
var app = builder.Build();

app.MapHub<NotificationHub>("/notificationHub");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add CORS middleware
app.UseCors("AllowSpecificOrigins");
app.UseCors("AllowLocalhost");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
