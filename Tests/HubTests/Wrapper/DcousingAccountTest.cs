//using Data.Wrappers;
//
//using Newtonsoft.Json.Linq;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Fr8.Testing.Unit;
//using Fr8.Testing.Unit.Fixtures;

//namespace HubTests.Wrapper
//{
//	 [TestFixture]
//	 [Category("DcousingAccount")]
//	 public class DcouSingAccountTest : BaseTest
//	 {
//		  private DocuSignAccount _docuSignAccount;
//		  [SetUp]
//		  public override void SetUp()
//		  {
//				base.SetUp();
//				_docuSignAccount = new DocuSignAccount();
//		  }

//		  [Test]
//		  public void CanGetAllConnectProfile()
//		  {
//				Configuration curConfiguration = _docuSignAccount.CreateDocuSignConnectProfile(FixtureData.TestCreateConnectProfile1());
//				ConnectProfile curConnectProfile = _docuSignAccount.GetDocuSignConnectProfiles();
//				Assert.IsNotNull(curConnectProfile.configurations);
//				Assert.Greater(Int32.Parse(curConnectProfile.totalRecords), 0);
//				_docuSignAccount.DeleteDocuSignConnectProfile(curConfiguration.connectId);
//		  }

//		  [Test]
//		  public void CanCreateConnectProfile()
//		  {
//				int totalRecord = Int32.Parse(_docuSignAccount.GetDocuSignConnectProfiles().totalRecords);
//				Configuration curConfiguration = _docuSignAccount.CreateDocuSignConnectProfile(FixtureData.TestCreateConnectProfile1());
//				Assert.AreEqual(Int32.Parse(_docuSignAccount.GetDocuSignConnectProfiles().totalRecords), totalRecord + 1);
//				_docuSignAccount.DeleteDocuSignConnectProfile(curConfiguration.connectId);
//		  }

//		  [Test]
//		  public void FailsCreateConnectProfileWhenNull()
//		  {
//				int totalRecordsBeforeCreate = Int32.Parse(_docuSignAccount.GetDocuSignConnectProfiles().totalRecords);
//				_docuSignAccount.CreateDocuSignConnectProfile(null);
//				int totalRecordsAfterCreate = Int32.Parse(_docuSignAccount.GetDocuSignConnectProfiles().totalRecords);
//				Assert.AreNotEqual(totalRecordsAfterCreate, totalRecordsBeforeCreate + 1);
//		  }

//		  [Test]
//		  public void CanUpdateConnectProfile()
//		  {
//				Configuration curConfiguration = _docuSignAccount.CreateDocuSignConnectProfile(FixtureData.TestCreateConnectProfile1());
//				curConfiguration.name = "Dockyard_Updated";
//				Configuration updatedConfiguration = _docuSignAccount.UpdateDocuSignConnectProfile(curConfiguration);

//				Assert.AreNotEqual(curConfiguration, updatedConfiguration);
//				_docuSignAccount.DeleteDocuSignConnectProfile(curConfiguration.connectId);
//		  }

//		  [Test]
//		  public void CanDeleteConnectProfile()
//		  {
//				Configuration curConfiguration = _docuSignAccount.CreateDocuSignConnectProfile(FixtureData.TestCreateConnectProfile1());
//				int totalRecordBeforDelete = Int32.Parse(_docuSignAccount.GetDocuSignConnectProfiles().totalRecords);

//				_docuSignAccount.DeleteDocuSignConnectProfile(curConfiguration.connectId);
//				int totalRecordAfterDelete = Int32.Parse(_docuSignAccount.GetDocuSignConnectProfiles().totalRecords);

//				Assert.AreEqual(totalRecordAfterDelete, totalRecordBeforDelete - 1);
//		  }

//		  [Test]
//		  [ExpectedException(ExpectedException = typeof(NullReferenceException))]
//		  public void FailsDeleteConnectProfileIfConnectIdNull()
//		  {
//				_docuSignAccount.DeleteDocuSignConnectProfile(null);
//		  }
//	 }
//}
