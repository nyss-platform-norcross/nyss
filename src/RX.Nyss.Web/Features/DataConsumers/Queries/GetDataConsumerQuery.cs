using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.DataConsumers.Dto;

namespace RX.Nyss.Web.Features.DataConsumers.Queries
{
    public class GetDataConsumerQuery : IRequest<Result<GetDataConsumerResponseDto>>
    {
        public GetDataConsumerQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }

        public class Handler : IRequestHandler<GetDataConsumerQuery, Result<GetDataConsumerResponseDto>>
        {
            private readonly INyssContext _dataContext;

            private readonly ILoggerAdapter _loggerAdapter;

            public Handler(INyssContext dataContext, ILoggerAdapter loggerAdapter)
            {
                _dataContext = dataContext;
                _loggerAdapter = loggerAdapter;
            }

            public async Task<Result<GetDataConsumerResponseDto>> Handle(GetDataConsumerQuery request, CancellationToken cancellationToken)
            {
                var dataConsumer = await _dataContext.Users.FilterAvailable()
                    .OfType<DataConsumerUser>()
                    .Where(u => u.Id == request.Id)
                    .Select(u => new GetDataConsumerResponseDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Role = u.Role,
                        Email = u.EmailAddress,
                        PhoneNumber = u.PhoneNumber,
                        AdditionalPhoneNumber = u.AdditionalPhoneNumber,
                        Organization = u.Organization
                    })
                    .SingleOrDefaultAsync(cancellationToken);

                if (dataConsumer == null)
                {
                    _loggerAdapter.Debug($"Data consumer with id {request.Id} was not found");

                    return Result.Error<GetDataConsumerResponseDto>(ResultKey.User.Common.UserNotFound);
                }

                return new Result<GetDataConsumerResponseDto>(dataConsumer, true);
            }
        }
    }
}
