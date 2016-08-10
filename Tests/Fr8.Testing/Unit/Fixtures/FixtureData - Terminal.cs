using System;
using Data.Entities;
using Fr8.Infrastructure.Data.States;
using Data.States;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static TerminalDO TerminalOne()
        {
            return new TerminalDO
            {
                Name = "terminalAzureSqlServer",
                Label = "AzureSqlServer",
                Endpoint = "terminalAzureSqlServer",
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                Secret = Guid.NewGuid().ToString(),
                OperationalState = OperationalState.Active,
                ParticipationState = ParticipationState.Approved
            };
        }

        public static TerminalDO TerminalTwo()
        {
            return new TerminalDO
            {
                Name = "AzureSqlServer",
                Label = "AzureSqlServer",
                Endpoint = "AzureSqlServer",
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                Secret = Guid.NewGuid().ToString(),
                OperationalState = OperationalState.Active,
                ParticipationState = ParticipationState.Approved
            };
        }

        public static TerminalDO TerminalThree()
        {
            return new TerminalDO
            {
                Name = "http://localhost:46281/",
                Label = "http://localhost:46281/",
                Endpoint = "http://localhost:46281/",
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                Secret = Guid.NewGuid().ToString(),
                OperationalState = OperationalState.Active,
                ParticipationState = ParticipationState.Approved
            };
        }

        public static TerminalDO TerminalFour()
        {
            return new TerminalDO
            {
                Name = "AzureSqlServer",
                Label = "AzureSqlServer",
                Endpoint = "AzureSqlServer",
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                Secret = Guid.NewGuid().ToString(),
                OperationalState = OperationalState.Active,
                ParticipationState = ParticipationState.Approved
            };
        }

        public static TerminalDO TerminalFive()
        {
            return new TerminalDO
            {
                Name = "DocuSign",
                Label = "DocuSign",
                Endpoint = "localhost",
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                Secret = Guid.NewGuid().ToString(),
                OperationalState = OperationalState.Active,
                ParticipationState = ParticipationState.Approved
            };
        }

        public static TerminalDO TerminalSix()
        {
            return new TerminalDO
            {
                Id = FixtureData.GetTestGuidById(1),
                Name = "DocuSign",
                Label = "DocuSign",
                Endpoint = "http://localhost",
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                AuthenticationType = AuthenticationType.External,
                Secret = Guid.NewGuid().ToString(),
                OperationalState = OperationalState.Active,
                ParticipationState = ParticipationState.Approved
            };
        }
        public static TerminalDO TerminalSeven()
        {
            return new TerminalDO
            {
                Id = FixtureData.GetTestGuidById(1),
                Name = "terminalDocuSign",
                Label = "DocuSign",
                Endpoint = "localhost",
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                Secret = Guid.NewGuid().ToString(),
                OperationalState = OperationalState.Active,
                ParticipationState = ParticipationState.Approved
            };
        }
    }
}
