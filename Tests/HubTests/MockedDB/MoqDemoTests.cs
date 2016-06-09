using System.Collections.Generic;
using System.Net.Mail;
using Moq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Utilities;
using Hub.Interfaces;
using Fr8.Testing.Unit;

namespace HubTests.MockedDB
{
	[ TestFixture ]
    [Category("MockedDB")]
	public class MoqDemoTests: BaseTest
	{
		[ Test ]
		public void MoqMethodExample()
		{
			var mock = new Mock< IEmailAddress >();
			mock.Setup( e => e.ExtractFromString( "<Robert Robert>rjrudman@gmail.com" ) ).Returns( new List< ParsedEmailAddress >() );
		}


		[ Test ]
		public void MoqMatchingArgumentsExample()
		{
			var mock = new Mock< IEmailAddress >();
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				mock.Setup( e => e.ConvertFromMailAddress( uow, new MailAddress( "rjrudman@gmail.com" ) ) ).Returns( new EmailAddressDO() );
			}
		}


		[ Test ]
		public void MoqPropertiesExample()
		{
			const string name = "test email name";
			var mock = new Mock< IEmailAddressDO >();
			mock.SetupProperty( e => e.Name, name );

			var emailName = mock.Object;
			Assert.AreEqual( name, emailName.Name );
		}
	}
}