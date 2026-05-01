using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Models.Certificates
{
    public class CheckAndSubmitViewModelTests
    {
        [Test]
        public void Properties_AreSettable_And_FullName_Composed()
        {
            var id = Guid.NewGuid();
            var vm = new CheckAndSubmitViewModel
            {
                CertificateId = id,
                GivenNames = "John",
                FamilyName = "Smith",
                CourseName = "Course",
                Organisation = "Org",
                AddressLine1 = "Line1",
                AddressLine2 = "Line2",
                TownOrCity = "Town",
                County = "County",
                Postcode = "PC1 1PC",
                BackRoute = "back"
            };

            vm.CertificateId.Should().Be(id);
            vm.FullName.Should().Be("John Smith");
            vm.Organisation.Should().Be("Org");
            vm.AddressLine1.Should().Be("Line1");
            vm.TownOrCity.Should().Be("Town");
            vm.Postcode.Should().Be("PC1 1PC");
        }

        [Test]
        public void FullName_Handles_Nulls_And_Trims()
        {
            var vm = new CheckAndSubmitViewModel { GivenNames = null, FamilyName = "Smith" };
            vm.FullName.Should().Be("Smith");

            vm = new CheckAndSubmitViewModel { GivenNames = "John", FamilyName = null };
            vm.FullName.Should().Be("John");

            vm = new CheckAndSubmitViewModel { GivenNames = " ", FamilyName = " " };
            vm.FullName.Should().Be(string.Empty);
        }
    }
}
