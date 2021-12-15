using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Projects.Dto;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Projects.Commands
{
    public class UpdateProjectErrorMessagesCommand : IRequest<IReadOnlyList<ProjectErrorMessageDto>>
    {
        public UpdateProjectErrorMessagesCommand(int projectId, Dictionary<string, string> body)
        {
            ProjectId = projectId;
            Body = body;
        }

        public int ProjectId { get; }

        public Dictionary<string, string> Body { get; }

        public class Handler : IRequestHandler<UpdateProjectErrorMessagesCommand, IReadOnlyList<ProjectErrorMessageDto>>
        {
            private readonly INyssContext _context;

            private readonly IDateTimeProvider _dateTimeProvider;

            private readonly IAuthorizationService _authorization;

            public Handler(
                INyssContext context,
                IDateTimeProvider dateTimeProvider,
                IAuthorizationService authorization)
            {
                _context = context;
                _dateTimeProvider = dateTimeProvider;
                _authorization = authorization;
            }

            public async Task<IReadOnlyList<ProjectErrorMessageDto>> Handle(UpdateProjectErrorMessagesCommand request, CancellationToken cancellationToken)
            {
                var errorMessages = await _context.ProjectErrorMessages
                    .Where(x => x.ProjectId == request.ProjectId)
                    .ToListAsync(cancellationToken);

                var result = new List<ProjectErrorMessageDto>();

                foreach (var (key, value) in request.Body)
                {
                    result.Add(new ProjectErrorMessageDto
                    {
                        Key = key,
                        Message = value,
                    });

                    var item = errorMessages.SingleOrDefault(x => x.MessageKey == key);
                    if (item == null)
                    {
                        _context.ProjectErrorMessages.Add(new ProjectErrorMessage
                        {
                            ProjectId = request.ProjectId,
                            MessageKey = key,
                            Message = value,
                            CreatedAtUtc = _dateTimeProvider.UtcNow,
                            CreatedBy = _authorization.GetCurrentUserName(),
                        });

                        continue;
                    }

                    if (item.Message == value)
                    {
                        continue;
                    }

                    item.Message = value;
                    item.UpdatedAtUtc = _dateTimeProvider.UtcNow;
                    item.UpdatedBy = _authorization.GetCurrentUserName();
                }

                await _context.SaveChangesAsync(cancellationToken);

                return result;
            }
        }
    }
}
