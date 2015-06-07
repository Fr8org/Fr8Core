using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManagers.Packagers.SendGrid;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using SendGrid;
using StructureMap;

namespace Playground
{
    public class Program
    {
        /// <summary>
        /// This is a sandbox for devs to use. Useful for directly calling some library without needing to launch the main application
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE); //set to either "test" or "dev"
            KwasantDbContext db = new KwasantDbContext();
            db.Database.Initialize(true);

            var evDO = new EventDO();
            evDO.CreatedBy = new UserDO { EmailAddress = new EmailAddressDO { Name = "Alex Edelstein" } };

            evDO.Description = @"Meeting with Paul Maeder, Campaign co-chair for the School of  Engineering and Applied Sciences.";
            evDO.Attendees.Add(new AttendeeDO { Name = "Alex Edelstein", EmailAddress = new EmailAddressDO("alex@edelstein.org") });
            evDO.Attendees.Add(new AttendeeDO { Name = "Dieterich, Joshua Ethan", EmailAddress = new EmailAddressDO("joshua_dieterich@harvard.edu") });
            evDO.Attendees.Add(new AttendeeDO { Name = "Outbound Archive", EmailAddress = new EmailAddressDO("kwasantoutbound@gmail.com") });
            evDO.Attendees.Add(new AttendeeDO { Name = "'Alexed15@gmail.com'", EmailAddress = new EmailAddressDO("alexed15@gmail.com") });
            evDO.StartDate = new DateTimeOffset(2014, 12, 09, 16, 0, 0, 0, TimeSpan.FromHours(-8));
            evDO.EndDate = evDO.StartDate.AddHours(1);
            evDO.Location = "Harvard";
            evDO.Summary = "Harvard Meeting with Paul Maeder";

            var cal = Event.GenerateICSCalendarStructure(evDO);

            iCalendarSerializer serializer = new iCalendarSerializer(cal);
            string fileToAttach = serializer.Serialize(cal);

        }
    }
}
