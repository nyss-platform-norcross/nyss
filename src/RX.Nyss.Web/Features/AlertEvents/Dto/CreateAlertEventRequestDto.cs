using System;
using FluentValidation;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.AlertEvents.Dto
{
    public class CreateAlertEventRequestDto
    {
        public DateTime Timestamp { get; set; }
        public int EventTypeId { get; set; }
        public int? EventSubtypeId { get; set; }
        public string Text { get; set; }

        public class Validator : AbstractValidator<CreateAlertEventRequestDto>
        {
            public Validator()
            {
                RuleFor(x => x.EventTypeId)
                    .NotEmpty();
                RuleFor(x => x.Timestamp)
                    .NotEmpty();
            }
        }
    }
}
