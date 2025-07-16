namespace BlazorApp.Pages.Components
{
    public interface IDashboardSection
    {
        Task RefreshAsync(CancellationToken cancellationToken = default);
        bool IsLoading { get; }
    }

}
