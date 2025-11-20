using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class VerifyException : Exception
    {
        public VerifyException() :
            base()
        {
        }

        public VerifyException(string message) :
            base(message)
        {
        }

        public VerifyException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}