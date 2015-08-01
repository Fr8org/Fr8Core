using System;
using System.Collections.Generic;
using System.IO;

using Data.Entities;

using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
		public static EnvelopeDO CreateEnvelope()
		{
			return new EnvelopeDO { DocusignEnvelopeId = "21", Status = EnvelopeDO.EnvelopeState.Any};
		}

        public static Envelope CreateEnvelope(Account account)
        {
            // create envelope object and assign login info
            return new Envelope
                   {
                       // assign account info from above
                       Login = account,
                       // "sent" to send immediately, "created" to save envelope as draft
                       Status = "created",
                       Created = DateTime.UtcNow
                   };
        }

	    public static string FullFilePathToDocument()
	    {
            //note: renamed pdf file to xml for checking pdf to github.
            string tempPdf = Path.Combine(Environment.CurrentDirectory, "Items", "small_pdf_file.png");
            //string actualPdf = Path.Combine(Environment.CurrentDirectory, "App_Data", "small_pdf_file.png");
	        
            //File.Copy(tempPdf, actualPdf);
	        return tempPdf;
	    }

	    public static TabCollection GetTabCollection()
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
	}
}