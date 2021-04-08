using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients.Access;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients.Dto
{
    public class ProjectAlertNotHandledRecipientRequestDto
    {
        public int UserId { get; set; }
        public int OrganizationId { get; set; }

        public class Validator : AbstractValidator<ProjectAlertNotHandledRecipientRequestDto>
        {
            public Validator(IProjectAlertNotHandledRecipientAccessService _projectAlertNotHandledRecipientAccessService)
            {
                RuleFor(x => x.OrganizationId)
                    .MustAsync(async (model, organizationId, t) => await _projectAlertNotHandledRecipientAccessService.HasAccessToCreateForOrganization(organizationId))
                    .WithMessageKey(ResultKey.AlertNotHandledNotificationRecipient.UserMustBeInSameOrg);

                RuleFor(x => x.UserId)
                    .MustAsync(async (model, userId, t) => await _projectAlertNotHandledRecipientAccessService.UserIsInOrganization(userId, model.OrganizationId))
                    .WithMessageKey(ResultKey.AlertNotHandledNotificationRecipient.UserMustBeInSameOrg);
            }
        }
    }
}
