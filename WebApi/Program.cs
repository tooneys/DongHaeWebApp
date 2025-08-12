using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApi.Infrastructure;
using WebApi.Repositories;
using WebApi.Services;
using WebApi.Services.Auth;
using WebApi.Services.Common;
using WebApi.Services.Dashboard;
using WebApi.Services.OpticianMap;
using WebApi.Services.PartnerCard;
using WebApi.Services.Report;
using WebApi.Services.Vehicle;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

    // 마케팅 데이터 서비스 및 리포지토리 등록
    builder.Services.AddScoped<IMarketingDataRepository, MarketingDataRepository>();
    builder.Services.AddScoped<IFileUploadService, FileUploadService>();

    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ICommonService, CommonService>();
    builder.Services.AddScoped<IImageService, ImageService>();
    builder.Services.AddScoped<IPartnerCardService, PartnerCardService>();
    builder.Services.AddScoped<IOpticianMapService, OpticianMapService>();
    builder.Services.AddScoped<IDashboardSalesService, DashboardSalesService>();
    builder.Services.AddScoped<IVehicleService, VehicleService>();
    builder.Services.AddScoped<IReportService, ReportService>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
        };
    });

    builder.Services.AddAuthorization();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentCors", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });

        options.AddPolicy("ProductionCors", policy =>
        {
            policy.WithOrigins(
                    "https://order.donghaelens.com",
                    "https://sales.donghaelens.com",
                    "https://api.donghaelens.com",
                    "https://localhost:7240"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    //builder.Services.AddCors(options =>
    //{
    //    options.AddDefaultPolicy(policy =>
    //    {
    //        if (builder.Environment.IsDevelopment())
    //        {
    //            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    //        }
    //        else
    //        {
    //            policy.WithOrigins(
    //                        "https://order.donghaelens.com",
    //                        "https://sales.donghaelens.com",
    //                        "https://api.donghaelens.com",
    //                        "https://localhost:7240"
    //                  )
    //                  .AllowAnyHeader()
    //                  .AllowAnyMethod()
    //                  .AllowCredentials();
    //        }
    //    });
    //});

    // 파일 업로드 크기 제한 설정
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 100 * 1024 * 1024;
    });

    var app = builder.Build();

    // 환경별 설정
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseCors("DevelopmentCors");
    }
    else if (app.Environment.IsProduction())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
        app.UseCors("ProductionCors");
    }

    // 업로드 디렉토리 생성 확인
    var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads");
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
    }

    // 미들웨어 순서 중요!
    app.UseStaticFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads"
    });

    app.UseHttpsRedirection();
    //app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    // 시작 오류 로깅
    Console.WriteLine($"애플리케이션 시작 실패: {ex.Message}");
    Console.WriteLine($"스택 트레이스: {ex.StackTrace}");
    throw;
}