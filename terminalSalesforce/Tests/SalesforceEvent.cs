﻿using System;
using UtilitiesTesting;
using NUnit.Framework;
using PluginBase.BaseClasses;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;
using pluginSalesforce;
using pluginSalesforce.Infrastructure;


namespace pluginTests.PluginSalesforceTests
{
    /// <summary>
    /// Summary description for Event
    /// </summary>
    [TestFixture]
    public class SalesforceEvent : BaseTest
    {
        BasePluginController _basePluginController;
        pluginSalesforce.Services.Event _event;
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _event = new pluginSalesforce.Services.Event();
            _basePluginController = new BasePluginController();
            PluginSalesforceStructureMapBootstrapper.ConfigureDependencies(PluginSalesforceStructureMapBootstrapper.DependencyType.LIVE);
        }

        [Test]
        public void ParseXMLPayLoadTest()
        {
            string xmlPayLoad = @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
             <soapenv:Body>
              <notifications xmlns=""http://soap.sforce.com/2005/09/outbound"">
               <OrganizationId>00D610000007nIoEAI</OrganizationId>
               <ActionId>04k610000008PQjAAM</ActionId>
               <SessionId>00D610000007nIo!AQ8AQBnP8vQ96dPAo4PQTrE6PC2fTYsx.jcnurk2jE4iiI_g96aVRATIdbxQCJGU6guRIhyt.CrqlkK7lnDxkzvLP1qBMBxR</SessionId>
               <EnterpriseUrl>https://na34.salesforce.com/services/Soap/c/34.0/00D610000007nIo</EnterpriseUrl>
               <PartnerUrl>https://na34.salesforce.com/services/Soap/u/34.0/00D610000007nIo</PartnerUrl>
               <Notification>
                <Id>04l61000000GwHwAAK</Id>
                <sObject xsi:type=""sf:Lead"" xmlns:sf=""urn:sobject.enterprise.soap.sforce.com"">
                 <sf:Id>00Q61000002VyBZEA0</sf:Id>
                 <sf:CreatedById>00561000000JECsAAO</sf:CreatedById>
                 <sf:LastName>LastName</sf:LastName>
                 <sf:OwnerId>00561000000JECsAAO</sf:OwnerId>
                </sObject>
               </Notification>
              </notifications>
             </soapenv:Body>
            </soapenv:Envelope>";

            string leadId = string.Empty;
            string accountId = string.Empty;

            _event.Parse(xmlPayLoad, out leadId, out accountId);
            Assert.AreEqual(leadId, "00Q61000002VyBZEA0");
            Assert.AreEqual(accountId, "00561000000JECsAAO");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public void ParseXMLPayLoad_ArguementExpection()
        {
            string xmlPayLoad = "";
            string leadId = string.Empty;
            string accountId = string.Empty;
            _event.Parse(xmlPayLoad, out leadId, out accountId);
        }
    }
}