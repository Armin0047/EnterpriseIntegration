using EnterpriseIntegration.Core.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using EnterpriseIntegration.Infrastructure.Middleware;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
// ==========================================
// 1. Controller Services & JSON Options
// ==========================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // جلوگیری از تغییر حروف یا فیلدهای Null در صورت نیاز
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // برای خروجی با همان نام‌های دقیق فیلدها یا PascalCase
    });

// ==========================================
// 2. JWT Authentication Configuration
// ==========================================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Secret Key is missing in appsettings.json");


builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // در محیط لوکال/توسعه
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // جلوگیری از تاخیر زمانی پیش‌فرض ۵ دقیقه‌ای انقضا
    };
});

builder.Services.AddAuthorization();

// ==========================================
// 3. Swagger with JWT Support (Authorize Lock)
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Invoice Synchronization API",
        Version = "v1",
        Description = "Enterprise Web API for Invoicing System (High-Performance Dapper Stack)"
    });

    // اضافه کردن قابلیت ورودی Bearer Token به Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "مقدار توکن JWT را اینجا وارد کنید (بدون نیاز به نوشتن کلمه Bearer):"
    });

    // 🟢 بارگذاری فایل کامنت‌های XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ==========================================
// 4. Database Connection Factory (Dapper Context)
// ==========================================
// برای ایجاد کانکشن‌های تمیز و بدون Leak
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured.");

builder.Services.AddScoped<System.Data.IDbConnection>(_ =>
    new Microsoft.Data.SqlClient.SqlConnection(connectionString));

var app = builder.Build();


app.UseMiddleware<AuditLoggingMiddleware>();


app.UseHttpsRedirection();
// ==========================================
// 5. Middleware Pipeline
// ==========================================
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "InvoiceSync API v1");
//        // 🟢 این خط بخش Schemas / Models را کاملاً مخفی می‌کند:
//        c.DefaultModelsExpandDepth(-1);
//    });
//}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EnterpriseIntegration API v1");
    // اگر می‌خواهی صفحه اصلی سایت (root) مستقیماً سواگر باز شود:
    // c.RoutePrefix = string.Empty; 
    c.DefaultModelsExpandDepth(-1);

});

// در صورت داشتن HTTPS در سرور اصلی


// ترتیب میدل‌ویرها حیاتی است: ابتدا احراز هویت، سپس مجوزها
app.UseAuthentication();
app.UseAuthorization();

//app.UseMiddleware<IdempotencyMiddleware>();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger", permanent: false))
   .ExcludeFromDescription();


app.Run();
