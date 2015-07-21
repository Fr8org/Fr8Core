using System.Linq;
using Data.Interfaces;
using DockyardTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace DockyardTest.Entities
{
	[ TestFixture ]
	public class ActionTests: BaseTest
	{
		[ Test ]
		[ Category( "Action" ) ]
		public void Action_Add_CanCreateAction()
		{
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var fixture = new FixtureData( uow );
				//SETUP
				//create a customer from fixture data
				var curActionDO = fixture.TestAction1();

				//EXECUTE
				uow.ActionRepository.Add( curActionDO );
				uow.SaveChanges();

				//VERIFY
				//check that it was saved to the db
				var savedActionDO = uow.ActionRepository.GetQuery().FirstOrDefault( u => u.Id == curActionDO.Id );
				Assert.NotNull( savedActionDO );
				Assert.AreEqual( curActionDO.Name, savedActionDO.Name );

				var curActionDO2 = fixture.TestAction2();

				//EXECUTE
				uow.ActionRepository.Add( curActionDO2 );
				uow.SaveChanges();

				//VERIFY
				//check that it was saved to the db
				var savedActionDO2 = uow.ActionRepository.GetQuery().FirstOrDefault( u => u.Id == curActionDO2.Id );
				Assert.NotNull( savedActionDO2 );
				Assert.AreEqual( curActionDO2.Name, savedActionDO2.Name );
			}
		}

		[ Test ]
		[ Category( "ActionList" ) ]
		public void ActionList_Add_CanCreateActionList()
		{
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var fixture = new FixtureData( uow );
				//SETUP
				//create a customer from fixture data
				var curActionListDO = fixture.TestEmptyActionList();

				//EXECUTE
				uow.ActionListRepository.Add( curActionListDO );
				uow.SaveChanges();

				//VERIFY
				//check that it was saved to the db
				var savedActionListDO = uow.ActionListRepository.GetQuery().FirstOrDefault( u => u.Id == curActionListDO.Id );
				Assert.NotNull( savedActionListDO );
				Assert.AreEqual( curActionListDO.Name, savedActionListDO.Name );

				var curActionListDO2 = fixture.TestActionList();

				//EXECUTE
				uow.ActionListRepository.Add( curActionListDO2 );
				uow.SaveChanges();

				//VERIFY
				//check that it was saved to the db
				var savedActionListDO2 = uow.ActionListRepository.GetQuery().FirstOrDefault( u => u.Id == curActionListDO2.Id );

				Assert.NotNull( curActionListDO2 );
				Assert.NotNull( savedActionListDO2 );
				Assert.AreEqual( curActionListDO2.Name, savedActionListDO2.Name );
				Assert.AreEqual( curActionListDO2.ActionOrdering.FirstOrDefault().Id, savedActionListDO2.ActionOrdering.FirstOrDefault().Id );
			}
		}
	}
}