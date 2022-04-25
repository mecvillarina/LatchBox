namespace Client.Shared
{
    public partial class NotFoundLayout
    {
        protected override void OnInitialized()
        {
            NavigationManager.NavigateTo("/");
        }
    }
}
