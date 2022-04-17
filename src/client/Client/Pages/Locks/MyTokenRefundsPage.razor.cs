namespace Client.Pages.Locks
{
    public partial class MyTokenRefundsPage
    {
        public bool IsLoaded { get; set; }
        public bool IsAssetLoaded { get; set; }


        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    IsAssetLoaded = true;
                    StateHasChanged();
                });
            }
        }
    }
}