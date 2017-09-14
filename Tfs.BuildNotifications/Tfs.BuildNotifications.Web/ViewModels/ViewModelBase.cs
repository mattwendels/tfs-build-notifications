using System.Collections.Generic;

namespace Tfs.BuildNotifications.Web.ViewModels
{
    public class ViewModelBase
    {
        public ViewModelBase()
        {
            Errors = new List<string>();
        }

        public List<string> Errors { get; set; }
    }
}
