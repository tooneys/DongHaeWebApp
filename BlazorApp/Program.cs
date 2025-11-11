using BlazorApp;
using BlazorApp.Services;
using BlazorApp.Services.Auth;
using BlazorApp.Services.Common;
using BlazorApp.Services.Development;
using BlazorApp.Services.OpticianMap;
using BlazorApp.Services.PartnerCard;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using System.Net.Http.Headers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 루트 컴포넌트 등록
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 로깅 설정
builder.Logging.SetMinimumLevel(LogLevel.Information);
if (builder.HostEnvironment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}

// HTTPS 강제 사용을 위한 환경별 API URL 설정
var apiBaseUrl = GetSecureApiBaseUrl(builder);

// HttpClient 설정 최적화 (HTTPS 강제)
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(apiBaseUrl),
        Timeout = TimeSpan.FromSeconds(300)
    };

    // 기본 헤더 설정
    httpClient.DefaultRequestHeaders.Accept.Clear();
    httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

    // HTTPS 보안 헤더 추가
    httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");

    // 개발 환경에서 SSL 인증서 검증 우회 (자체 서명 인증서용)
    if (builder.HostEnvironment.IsDevelopment())
    {
        // 개발 환경에서는 자체 서명 인증서 허용을 위한 설정
        // 실제 HttpClientHandler 설정은 서버 측에서 처리
        httpClient.DefaultRequestHeaders.Add("X-Development-Mode", "true");
    }

    return httpClient;
});

// LocalStorage 서비스
builder.Services.AddBlazoredLocalStorage(config =>
{
    config.JsonSerializerOptions.PropertyNamingPolicy = null; // PascalCase 유지
    config.JsonSerializerOptions.WriteIndented = builder.HostEnvironment.IsDevelopment();
});

// 인증 관련 서비스들
builder.Services.AddScoped<IAuthClientService, AuthClientService>();
builder.Services.AddScoped<ITokenManager, TokenManager>();
builder.Services.AddScoped<IApiResponseHandler, ApiResponseHandler>();

// 인증 및 권한 부여
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

// 커스텀 인증 상태 제공자
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// API 클라이언트 서비스들 (표준 구조)
builder.Services.AddScoped<ICommonClientService, CommonClientService>();
builder.Services.AddScoped<IPartnerCardClientService, PartnerCardClientService>();
builder.Services.AddScoped<IOpticianMapClientService, OpticianMapClientService>();
builder.Services.AddScoped<DashboardService>();     //DashBoard 서비스
builder.Services.AddScoped<IVehicleClientService, VehicleClientService>();

// MudBlazor 설정 최적화
builder.Services.AddMudServices(config =>
{
    // Snackbar 설정
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = true; // 중복 방지
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000; // 5초로 단축
    config.SnackbarConfiguration.HideTransitionDuration = 300;
    config.SnackbarConfiguration.ShowTransitionDuration = 300;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
    config.SnackbarConfiguration.MaxDisplayedSnackbars = 5;

    // 테마 설정
    config.ResizeOptions = new ResizeOptions
    {
        ReportRate = 300,
        EnableLogging = builder.HostEnvironment.IsDevelopment()
    };
});

// 개발 환경 전용 서비스
if (builder.HostEnvironment.IsDevelopment())
{
    // 개발용 로깅 향상
    builder.Services.AddScoped<IDevToolsService, DevToolsService>();
}

// 성능 최적화 서비스
builder.Services.AddScoped<ICacheService, LocalStorageCacheService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

// 앱 시작 전 초기화
await InitializeAppAsync(app);

await app.RunAsync();

// HTTPS 강제 사용을 위한 환경별 API URL 결정 메서드
static string GetSecureApiBaseUrl(WebAssemblyHostBuilder builder)
{
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

    try
    {
        // 개발 환경인지 확인
        if (builder.HostEnvironment.IsDevelopment())
        {
            // 개발 환경: localhost HTTPS 사용
            var devApiUrl = builder.Configuration["WebApiAddress"] ?? "https://localhost:5000";

            // HTTP URL이 설정된 경우 HTTPS로 변환
            if (devApiUrl.StartsWith("http://"))
            {
                devApiUrl = devApiUrl.Replace("http://", "https://");
                logger.LogInformation($"개발 환경 HTTP를 HTTPS로 변환: {devApiUrl}");
            }

            logger.LogInformation($"개발 환경 API URL: {devApiUrl}");
            return devApiUrl;
        }
        else
        {
            // 프로덕션 환경: HTTPS 강제 사용
            var prodApiUrl = builder.Configuration["WebApiAddress"];

            if (string.IsNullOrEmpty(prodApiUrl))
            {
                prodApiUrl = "https://api.donghaelens.com";
                logger.LogWarning($"WebApiAddress가 설정되지 않아 기본값 사용: {prodApiUrl}");
            }
            else if (prodApiUrl.StartsWith("http://"))
            {
                // HTTP를 HTTPS로 자동 변환
                prodApiUrl = prodApiUrl.Replace("http://", "https://");
                logger.LogInformation($"프로덕션 환경 HTTP를 HTTPS로 변환: {prodApiUrl}");
            }

            logger.LogInformation($"프로덕션 환경 API URL: {prodApiUrl}");
            return prodApiUrl;
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "API URL 설정 중 오류 발생");

        // 기본값으로 폴백 (HTTPS 사용)
        var fallbackUrl = builder.HostEnvironment.IsDevelopment()
            ? "https://localhost:5000"
            : "https://api.donghaelens.com";

        logger.LogWarning($"기본 HTTPS API URL 사용: {fallbackUrl}");
        return fallbackUrl;
    }
}

// 앱 초기화 메서드 (HTTPS 연결 체크 추가)
static async Task InitializeAppAsync(WebAssemblyHost app)
{
    try
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Blazor WebAssembly 앱 초기화 시작");

        // HTTPS 연결 상태 체크
        await CheckHttpsConnectionAsync(app, logger);

        // 인증 상태 초기화
        var authStateProvider = app.Services.GetRequiredService<AuthenticationStateProvider>();
        await authStateProvider.GetAuthenticationStateAsync();

        // 캐시 초기화
        var cacheService = app.Services.GetRequiredService<ICacheService>();
        await cacheService.InitializeAsync();

        logger.LogInformation("Blazor WebAssembly 앱 초기화 완료");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "앱 초기화 중 오류 발생");
    }
}

// HTTPS 연결 체크 메서드
static async Task CheckHttpsConnectionAsync(WebAssemblyHost app, ILogger logger)
{
    try
    {
        var httpClient = app.Services.GetRequiredService<HttpClient>();
        var baseAddress = httpClient.BaseAddress?.ToString() ?? "";

        // 현재 페이지와 API 모두 HTTPS인지 확인
        var jsRuntime = app.Services.GetRequiredService<IJSRuntime>();
        var currentProtocol = await jsRuntime.InvokeAsync<string>("eval", "window.location.protocol");
        var currentHost = await jsRuntime.InvokeAsync<string>("eval", "window.location.host");

        logger.LogInformation($"현재 페이지: {currentProtocol}//{currentHost}");
        logger.LogInformation($"API 서버: {baseAddress}");

        if (currentProtocol == "https:" && baseAddress.StartsWith("https://"))
        {
            logger.LogInformation("✅ 완전한 HTTPS 연결 구성됨");

            // HTTPS 연결 테스트
            try
            {
                var response = await httpClient.GetAsync("health");
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("✅ HTTPS API 연결 테스트 성공");
                }
                else
                {
                    logger.LogWarning($"⚠️ HTTPS API 연결 응답 코드: {response.StatusCode}");
                }
            }
            catch (Exception apiEx)
            {
                logger.LogWarning($"⚠️ HTTPS API 연결 테스트 실패: {apiEx.Message}");
            }
        }
        else if (currentProtocol == "http:")
        {
            logger.LogWarning("⚠️ 페이지가 HTTP로 로드됨 - HTTPS 사용을 권장합니다");
        }
        else if (baseAddress.StartsWith("http://"))
        {
            logger.LogWarning("⚠️ API 서버가 HTTP로 설정됨 - HTTPS 사용을 권장합니다");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "HTTPS 연결 체크 중 오류 발생");
    }
}
