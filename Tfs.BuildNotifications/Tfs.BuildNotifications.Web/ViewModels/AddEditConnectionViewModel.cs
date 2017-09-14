namespace Tfs.BuildNotifications.Web.ViewModels
{
    public class AddEditConnectionViewModel : ViewModelBase
    {
        public string TfsServerUrl { get; set; }
        public string PersonalAccessToken { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string TfsServerLocation { get; set; }

        public bool NewConfiguration { get; set; }
    }
}
