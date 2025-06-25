using Blazored.LocalStorage;
using MudBlazor;
using System.Text.Json;

namespace BlazorApp.Services.Development
{
    public interface IDevToolsService
    {
        Task LogComponentLifecycleAsync(string componentName, string lifecycle, object? data = null);
        Task LogApiCallAsync(string endpoint, string method, object? request = null, object? response = null);
        Task LogPerformanceAsync(string operation, TimeSpan duration);
        Task<Dictionary<string, object>> GetDebugInfoAsync();
        Task ClearLogsAsync();
        Task<List<LogEntry>> GetLogsAsync(LogLevel? level = null);
        Task SimulateNetworkDelayAsync(int milliseconds = 1000);
        Task<string> ExportLogsAsync();
        void ShowToast(string message, string type = "info");
    }
    public class DevToolsService : IDevToolsService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly ILogger<DevToolsService> _logger;
        private readonly ISnackbar _snackbar;
        private readonly List<LogEntry> _inMemoryLogs = new();
        private const string LOGS_KEY = "dev_logs";

        public DevToolsService(
            ILocalStorageService localStorage,
            ILogger<DevToolsService> logger,
            ISnackbar snackbar)
        {
            _localStorage = localStorage;
            _logger = logger;
            _snackbar = snackbar;
        }

        public async Task LogComponentLifecycleAsync(string componentName, string lifecycle, object? data = null)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = LogLevel.Debug,
                Category = "Component",
                Message = $"{componentName} - {lifecycle}",
                Data = data
            };

            await AddLogEntryAsync(logEntry);
            _logger.LogDebug("Component Lifecycle: {ComponentName} - {Lifecycle}", componentName, lifecycle);
        }

        public async Task LogApiCallAsync(string endpoint, string method, object? request = null, object? response = null)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = LogLevel.Information,
                Category = "API",
                Message = $"{method} {endpoint}",
                Data = new { Request = request, Response = response }
            };

            await AddLogEntryAsync(logEntry);
            _logger.LogInformation("API Call: {Method} {Endpoint}", method, endpoint);
        }

        public async Task LogPerformanceAsync(string operation, TimeSpan duration)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = duration.TotalMilliseconds > 1000 ? LogLevel.Warning : LogLevel.Information,
                Category = "Performance",
                Message = $"{operation} took {duration.TotalMilliseconds:F2}ms",
                Data = new { Operation = operation, DurationMs = duration.TotalMilliseconds }
            };

            await AddLogEntryAsync(logEntry);

            if (duration.TotalMilliseconds > 1000)
            {
                ShowToast($"⚠️ Slow operation: {operation} ({duration.TotalMilliseconds:F0}ms)", "warning");
            }
        }

        public async Task<Dictionary<string, object>> GetDebugInfoAsync()
        {
            var debugInfo = new Dictionary<string, object>
            {
                ["Timestamp"] = DateTime.Now,
                ["UserAgent"] = await GetUserAgentAsync(),
                ["LocalStorage"] = await GetLocalStorageInfoAsync(),
                ["Memory"] = GetMemoryInfo(),
                ["Logs"] = _inMemoryLogs.Count,
                ["Environment"] = "Development"
            };

            return debugInfo;
        }

        public async Task ClearLogsAsync()
        {
            _inMemoryLogs.Clear();
            await _localStorage.RemoveItemAsync(LOGS_KEY);
            ShowToast("🗑️ Logs cleared", "info");
        }

        public async Task<List<LogEntry>> GetLogsAsync(LogLevel? level = null)
        {
            // 로컬 스토리지에서 로그 불러오기
            var storedLogs = await _localStorage.GetItemAsync<List<LogEntry>>(LOGS_KEY) ?? new List<LogEntry>();

            // 메모리 로그와 병합
            var allLogs = storedLogs.Concat(_inMemoryLogs).ToList();

            if (level.HasValue)
            {
                allLogs = allLogs.Where(log => log.Level == level.Value).ToList();
            }

            return allLogs.OrderByDescending(log => log.Timestamp).Take(100).ToList();
        }

        public async Task SimulateNetworkDelayAsync(int milliseconds = 1000)
        {
            ShowToast($"🐌 Simulating network delay: {milliseconds}ms", "info");
            await Task.Delay(milliseconds);
        }

        public async Task<string> ExportLogsAsync()
        {
            var logs = await GetLogsAsync();
            var json = JsonSerializer.Serialize(logs, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            ShowToast("📄 Logs exported to console", "success");
            Console.WriteLine("=== EXPORTED LOGS ===");
            Console.WriteLine(json);
            Console.WriteLine("=== END LOGS ===");

            return json;
        }

        public void ShowToast(string message, string type = "info")
        {
            var severity = type.ToLower() switch
            {
                "success" => Severity.Success,
                "warning" => Severity.Warning,
                "error" => Severity.Error,
                _ => Severity.Info
            };

            _snackbar.Add($"[DEV] {message}", severity);
        }

        private async Task AddLogEntryAsync(LogEntry logEntry)
        {
            _inMemoryLogs.Add(logEntry);

            // 메모리 로그가 너무 많으면 정리
            if (_inMemoryLogs.Count > 50)
            {
                var oldLogs = _inMemoryLogs.Take(25).ToList();
                var existingLogs = await _localStorage.GetItemAsync<List<LogEntry>>(LOGS_KEY) ?? new List<LogEntry>();
                existingLogs.AddRange(oldLogs);

                // 최대 200개까지만 저장
                if (existingLogs.Count > 200)
                {
                    existingLogs = existingLogs.TakeLast(200).ToList();
                }

                await _localStorage.SetItemAsync(LOGS_KEY, existingLogs);
                _inMemoryLogs.RemoveRange(0, 25);
            }
        }

        private async Task<string> GetUserAgentAsync()
        {
            // JavaScript interop을 통해 User Agent 가져오기
            return "Browser Info"; // 실제로는 IJSRuntime 사용
        }

        private async Task<object> GetLocalStorageInfoAsync()
        {
            try
            {
                var keys = await _localStorage.KeysAsync();
                return new { KeyCount = keys.Count(), Keys = keys.Take(10) };
            }
            catch
            {
                return new { Error = "Unable to access localStorage" };
            }
        }

        private object GetMemoryInfo()
        {
            var gc = GC.GetTotalMemory(false);
            return new
            {
                TotalMemoryBytes = gc,
                TotalMemoryMB = gc / (1024 * 1024),
                Generation0Collections = GC.CollectionCount(0),
                Generation1Collections = GC.CollectionCount(1),
                Generation2Collections = GC.CollectionCount(2)
            };
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
