using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.DataConsumers.Commands
{
    public class EditDataConsumerCommand : IRequest<Result>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string AdditionalPhoneNumber { get; set; }

        public string Organization { get; set; }

        public class Handler : IRequestHandler<EditDataConsumerCommand, Result>
        {
            private readonly INyssContext _dataContext;

            private readonly INationalSocietyUserService _nationalSocietyUserService;

            private readonly ILoggerAdapter _loggerAdapter;

            public Handler(
                INyssContext dataContext,
                INationalSocietyUserService nationalSocietyUserService,
                ILoggerAdapter loggerAdapter)
            {
                _dataContext = dataContext;
                _nationalSocietyUserService = nationalSocietyUserService;
                _loggerAdapter = loggerAdapter;
            }

            public async Task<Result> Handle(EditDataConsumerCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var user = await _nationalSocietyUserService.GetNationalSocietyUser<DataConsumerUser>(request.Id);

                    user.Name = request.Name;
                    user.PhoneNumber = request.PhoneNumber;
                    user.Organization = request.Organization;
                    user.AdditionalPhoneNumber = request.AdditionalPhoneNumber;

                    await _dataContext.SaveChangesAsync(cancellationToken);

                    return Result.Success();
                }
                catch (ResultException e)
                {
                    _loggerAdapter.Debug(e);
                    return e.Result;
                }
            }
        }

        public class Validator : AbstractValidator<EditDataConsumerCommand>
        {
            public Validator()
            {
                RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20).PhoneNumber().Unless(r => string.IsNullOrEmpty(r.AdditionalPhoneNumber));
                RuleFor(m => m.Organization).NotEmpty().MaximumLength(100);
            }
        }
    }
}
