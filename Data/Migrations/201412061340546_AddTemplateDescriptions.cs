using System.Collections.Generic;

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTemplateDescriptions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Envelopes", "TemplateDescription", c => c.String());
            const string sqlFormat = @"UPDATE dbo.Envelopes SET TemplateDescription = '{0}' WHERE TemplateName = '{1}'";

            var templateDescriptionMapping = new Dictionary<string, string>
            {
                {"2e411208-7a0d-4a72-a005-e39ae018d708", "Welcome to Kwasant"},
                {"09a7919f-e5d3-4c98-b6b8-d8ac6171401d", "Negotiation request"},
                {"6a59b7f4-9f12-47ea-8fa9-be1b96733b3d", "Negotiation request"},
                {"760f0be0-6ccc-4d31-aeb1-297f86267475", "Forgot Password"},
                {"User_Settings_Notification", "User Settings Notification"},
                {"e4da63fd-2459-4caf-8e4f-b4d6f457e95a", "User Credentials"},
                {"1956e20e-3224-4139-93e2-7a7cacbd2b34", "Event Invitation"},
                {"a2e745ee-5c37-406e-984e-1df8f48bc56e", "Event Invitation Update"},
                {"7063998f-0560-4a3e-9fbe-88432892286b", "Simple Email"},
            };
            foreach (var kvp in templateDescriptionMapping)
            {
                Sql(String.Format(sqlFormat, kvp.Value, kvp.Key));
            }
        }
        
        public override void Down()
        {
            DropColumn("dbo.Envelopes", "TemplateDescription");
        }
    }
}
