using Microsoft.JSInterop;

namespace BlazorApp.Pages.OpticianMap.Interop;

public class MapInterop : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private IJSObjectReference _module;

    public MapInterop(IJSRuntime js) => _js = js;

    public async Task InitializeAsync(DotNetObjectReference<OpticianMap> dotNetRef, string elementId, object markers)
    {
        _module = await _js.InvokeAsync<IJSObjectReference>("import", "./js/map/main.js");
        await _module.InvokeVoidAsync("naverMapUtils.initializeNaverMap", dotNetRef, elementId, markers);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null) await _module.DisposeAsync();
    }
}
