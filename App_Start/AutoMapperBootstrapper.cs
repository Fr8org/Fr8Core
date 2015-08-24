using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json.Linq;
using Data.Entities;
using Web.ViewModels;
using Web.ViewModels.JsonConverters;
using Utilities.AutoMapper;
using Data.Interfaces.DataTransferObjects;

namespace Web.App_Start
{
    public class AutoMapperBootStrapper
    {

        public static void ConfigureAutoMapper()
        {
            //Mapper.CreateMap<EventDO, EventVM>()
            //    .ForMember(ev => ev.Attendees, opts => opts.ResolveUsing(ev => String.Join(",", ev.Attendees.Select(eea => eea.EmailAddress.Address).Distinct())))
            //    .ForMember(ev => ev.CreatedByAddress, opts => opts.ResolveUsing(evdo => evdo.CreatedBy.EmailAddress.Address))
            //    .ForMember(ev => ev.BookingRequestTimezoneOffsetInMinutes, opts => opts.ResolveUsing(evdo => evdo.StartDate.Offset.TotalMinutes * - 1));

            //Mapper.CreateMap<EventVM, EventDO>()
            //    .ForMember(eventDO => eventDO.Attendees, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.ActivityStatus, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.CreatedBy, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.CreatedByID, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.EventStatus, opts => opts.Ignore());

            //Mapper.CreateMap<EventDO, EventDO>()
            //    .ForMember(eventDO => eventDO.ActivityStatus, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.Attendees, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.BookingRequest, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.BookingRequestID, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.Calendar, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.CalendarID, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.CreateType, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.CreateTypeTemplate, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.CreatedBy, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.CreatedByID, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.CreateDate, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.Emails, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.EventStatus, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.EventStatusTemplate, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.Id, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.SyncStatus, opts => opts.Ignore())
            //    .ForMember(eventDO => eventDO.SyncStatusTemplate, opts => opts.Ignore());

            Mapper.CreateMap<Tuple<DockyardAccountDO, IEnumerable<AuthorizationTokenDO>>, ManageUserVM>()
                .ForMember(mu => mu.HasLocalPassword, opts => opts.ResolveUsing(tuple => !string.IsNullOrEmpty(tuple.Item1.PasswordHash)))
                .ForMember(mu => mu.HasToken, opts => opts.ResolveUsing(tuple => tuple.Item2.Any()));

            Mapper.CreateMap<ActionNameDTO, ActionRegistrationDO>()
                  .ForMember(actionRegistrationDO => actionRegistrationDO.ActionType, opts => opts.ResolveUsing(e => e.ActionType))
                  .ForMember(actionRegistrationDO => actionRegistrationDO.Version, opts => opts.ResolveUsing(e => e.Version));
               // .ForMember(mu => mu.HasLocalPassword, opts => opts.ResolveUsing(tuple => !string.IsNullOrEmpty(tuple.Item1.PasswordHash)))
              //  .ForMember(mu => mu.HasToken, opts => opts.ResolveUsing(tuple => tuple.Item2.Any()));

            //Mapper.CreateMap<EventDO, RelatedItemShowVM>()
            //    .ForMember(ri => ri.id, opts => opts.ResolveUsing(e => e.Id))
            //    .ForMember(ri => ri.Type, opts => opts.UseValue("Event"))
            //    .ForMember(ri => ri.Date, opts => opts.ResolveUsing(e => e.StartDate.ToString("M-d-yy hh:mm tt")));

            //Mapper.CreateMap<InvitationResponseDO, RelatedItemShowVM>()
            //    .ForMember(ri => ri.id, opts => opts.ResolveUsing(e => e.Id))
            //    .ForMember(ri => ri.Type, opts => opts.UseValue("Invitation Response"))
            //    .ForMember(ri => ri.Date, opts => opts.ResolveUsing(e => e.DateReceived.ToString("M-d-yy hh:mm tt")));

            //Mapper.CreateMap<NegotiationVM, NegotiationDO>();
            //Mapper.CreateMap<NegotiationQuestionVM, QuestionDO>()
            //    .ForMember(n => n.AnswerType, opts => opts.ResolveUsing(n => n.AnswerType));
            //Mapper.CreateMap<NegotiationAnswerVM, AnswerDO>()
            //    .ForMember(n => n.AnswerStatus, opts => opts.ResolveUsing(a => a.AnswerState));

            //Mapper.CreateMap<BookingRequestDO, BookingRequestVM>()
            //    .ForMember(br => br.Id, opts => opts.ResolveUsing(e => e.Id))
            //    .ForMember(br => br.BookerName, opts => opts.ResolveUsing(e => e.Booker != null ? e.Booker.EmailAddress.Address : ""))
            //    .ForMember(br => br.Subject, opts => opts.ResolveUsing(e => e.Subject))
            //    .ForMember(br => br.EmailAddress, opts => opts.ResolveUsing(e => e.From.Address))
            //    .ForMember(br => br.DateReceived, opts => opts.ResolveUsing(e => e.DateReceived))
            //    .ForMember(br => br.HTMLText, opts => opts.ResolveUsing(e => e.HTMLText));
            Mapper.CreateMap<ProcessTemplateDTO, ProcessTemplateDO>();
            Mapper.CreateMap<ProcessTemplateDO, ProcessTemplateDTO>();
            Mapper.CreateMap<UserVM, EmailAddressDO>()
                .ForMember(userDO => userDO.Address, opts => opts.ResolveUsing(e => e.EmailAddress));
            Mapper.CreateMap<UserVM, DockyardAccountDO>()
                .ForMember(userDO => userDO.Id, opts => opts.ResolveUsing(e => e.Id))
                .ForMember(userDO => userDO.FirstName, opts => opts.ResolveUsing(e => e.FirstName))
                .ForMember(userDO => userDO.LastName, opts => opts.ResolveUsing(e => e.LastName))
                .ForMember(userDO => userDO.UserName, opts => opts.ResolveUsing(e => e.UserName))
                .ForMember(userDO => userDO.EmailAddress, opts => opts.ResolveUsing(e => new EmailAddressDO {Address = e.EmailAddress}))
                .ForMember(userDO => userDO.Roles, opts => opts.Ignore());
                //.ForMember(userDO => userDO.Calendars, opts => opts.Ignore());
          //Mapper.CreateMap< ActionDO, ActionVM >()
          //    .ForMember( a => a.Id, opts => opts.ResolveUsing( ad => ad.Id ) )
          //    .ForMember( a => a.Name, opts => opts.ResolveUsing( ad => ad.Name ) );
        }
    }
}
//Missing type map configuration or unsupported mapping.

//Mapping types:
//UserVM -> EmailAddressDO
//Web.ViewModels.UserVM -> Data.Entities.EmailAddressDO

//Destination path:
//DockyardAccountDO

//Source value:
//Web.ViewModels.UserVM