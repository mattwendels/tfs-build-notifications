using FluentValidation;
using Tfs.BuildNotifications.Web.ViewModels;

namespace Tfs.BuildNotifications.Web.Nancy.Validators
{
    public class AddConnectionValidator : AbstractValidator<AddEditConnectionViewModel>
    {
        public AddConnectionValidator()
        {
            RuleFor(request => request.TfsServerUrl).NotEmpty().WithMessage("Please enter a TFS server/collection URL");
            RuleFor(request => request.TfsServerLocation).NotEmpty().WithMessage("Please select the TFS server location");
        }
    }
}
