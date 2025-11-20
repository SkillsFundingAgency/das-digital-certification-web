## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# Digital Certificates Web

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status%2Fdas-digital-certification-web?repoName=SkillsFundingAgency%2Fdas-digital-certification-web&branchName=main)
[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=SkillsFundingAgency_das-digital-certification-web)](https://sonarcloud.io/summary/new_code?id=SkillsFundingAgency_das-digital-certification-web)
[![Confluence Page](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/4921819148/Digital+Certification+Technical+Architecture)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

This web solution is part of Digital Certification project. Here apprentices can view details of their certificates, request prints and share links to digital versions.

## How It Works
Apprentices using GOV.UK One Login, must verify their login credentials and then be authorised to view their certificates by associating themselves with a ULN
When running this locally, with stub sign-in enabled, the launch url should be `https://localhost:5003/`

## 🚀 Installation

### Pre-Requisites
* A clone of this repository
* Optionally an Azure Active Directory account with the appropriate roles.
* The Outer API [das-apim-endpoints](https://github.com/SkillsFundingAgency/das-apim-endpoints/tree/master/src/DigitalCertificates) should be available either running locally or accessible in an Azure tenancy.

### Config
You can find the latest config file in [das-employer-config repository](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-digital-certificates-web/SFA.DAS.DigitalCertifcates.Web.json)

In the web project, if not exist already, add `AppSettings.Development.json` file with following content:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true;",
  "SFA.DAS.DigitalCertificates.Web,SFA.DAS.DigitalCertificates.GovSignIn",
  "EnvironmentName": "LOCAL",
  "ResourceEnvironmentName": "LOCAL",
  "cdn": {
    "url": "https://das-test-frnt-end.azureedge.net"
  },
  "StubEmail": "someemail",
  "StubId": "someid",
  "StubAuth": true
} 
```

## Technologies
* .NetCore 8.0
* NUnit
* Moq
* FluentAssertions
* RestEase
* MediatR
