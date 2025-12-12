using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.TestHelper.Extensions;
using SFA.DAS.DigitalCertificates.Web.Attributes;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Attributes
{
    [TestFixture]
    public class ValidateRequiredQueryParametersAttributeTests
    {
        private Mock<ILogger<ValidateRequiredQueryParametersAttribute>> _loggerMock;
        private ValidateRequiredQueryParametersAttribute _attribute;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<ValidateRequiredQueryParametersAttribute>>();
            _attribute = new ValidateRequiredQueryParametersAttribute(_loggerMock.Object);
        }

        [Test]
        public void OnActionExecuting_Should_ThrowException_When_RequiredQueryParameters_AreMissing()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("param1", $"{ValidateRequiredQueryParametersAttribute.MissingRequireQueryParameterMessage}param1");
            modelState.AddModelError("param2", $"{ValidateRequiredQueryParametersAttribute.MissingRequireQueryParameterMessage}param2");

            var actionContext = new ActionContext(new DefaultHttpContext(), new Microsoft.AspNetCore.Routing.RouteData(), new ControllerActionDescriptor(), modelState);
            var context = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new Mock<Controller>().Object
            );

            // Act
            Action action = () => _attribute.OnActionExecuting(context);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("Missing required query parameters: param1, param2");
            _loggerMock.VerifyLogError("Missing required query parameters: param1, param2", Times.Once);
        }

        [Test]
        public void OnActionExecuting_Should_NotThrowException_When_NoRequiredQueryParameters_AreMissing()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("param1", "Some other error message");

            var actionContext = new ActionContext(new DefaultHttpContext(), new Microsoft.AspNetCore.Routing.RouteData(), new ControllerActionDescriptor(), modelState);
            var context = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new Mock<Controller>().Object
            );

            // Act
            Action action = () => _attribute.OnActionExecuting(context);

            // Assert
            action.Should().NotThrow<ArgumentException>();
            _loggerMock.VerifyLogError(It.IsAny<string>(), Times.Never);
        }
    }
}