using System;
using System.Collections.Generic;
using System.IO;

using Data.Entities;

using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
	{
		public static EnvelopeDO TestEnvelope1()
		{
			return new EnvelopeDO { DocusignEnvelopeId = "21", Status = EnvelopeDO.EnvelopeState.Any};
		}

        public static Envelope TestEnvelope1WithGivenAccount(Account account)
        {
            // create envelope object and assign login info
            return new Envelope
                   {
                       // assign account info from above
                       Login = account,
                       // "sent" to send immediately, "created" to save envelope as draft
                       Status = "created",
                       Created = DateTime.UtcNow,
                       Recipients = new Recipients
                                    {
                                        recipientCount = "1",
                                        signers = new[]
                                                  {
                                                      new Signer
                                                      {
                                                          recipientId = Guid.NewGuid().ToString(),
                                                          name = "Orkan ARI",
                                                          email = "hello@orkan.com",
                                                      }
                                                  }
                                    }
                   };
        }

	    public static string TestRealPdfFile1()
	    {
            return Path.Combine(Environment.CurrentDirectory, "Tools\\TestFiles", "small_pdf_file.pdf");
	    }

	    public static TabCollection TestTabCollection1()
	    {
	        return new TabCollection
	               {
	                   textTabs = new List<TextTab>
	                              {
	                                  new TextTab
	                                  {
	                                      required = false,
	                                      height = 200,
	                                      width = 200,
	                                      xPosition = 200,
	                                      yPosition = 200,
	                                      name = "Amount",
	                                      value = "40"
	                                  }
	                              }
	               };
	    }

        /// <summary>
        /// This is test RestSettins for unit tests.
        /// </summary>
        /// <returns></returns>
	    public static RestSettings TestRestSettings1()
	    {
            // configure application's integrator key and webservice url
            RestSettings.Instance.IntegratorKey = "TEST-34d0ac9c-89e7-4acc-bc1d-24d6cfb867f2";
            RestSettings.Instance.DocuSignAddress = "http://demo.docusign.net";
            RestSettings.Instance.WebServiceUrl = RestSettings.Instance.DocuSignAddress + "/restapi/v2";

	        return RestSettings.Instance;
	    }
	}
}