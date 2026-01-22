using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.DigitalCertificates.Web.Helpers;
using System.Threading.Tasks;
using MediatR;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class BaseOrchestrator
    {
        private readonly IMediator _mediator;

        public BaseOrchestrator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public IMediator Mediator => _mediator;

        protected static async Task<bool> ValidateViewModel<T>(IValidator<T> validator, T viewModel, ModelStateDictionary modelState)
        {
            var result = await validator.ValidateAndAddModelErrorsAsync(viewModel, modelState);
            return modelState.IsValid;
        }
    }
}
