namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Rename_The_Main_Bucket_Categories : DbMigration
    {
        private const string UpdateCategoryNamesQuery = @"UPDATE ActivityCategory
                                                        SET Name = 'Ship Data'
                                                        WHERE (Name= 'Forward')

                                                        UPDATE ActivityCategory
                                                        SET Name = 'Get Data'
                                                        WHERE (Name= 'Get')

                                                        UPDATE ActivityCategory
                                                        SET Name = 'Triggers'
                                                        WHERE (Name= 'Monitor')";

        private const string RevertCategoryNamesQuery = @"UPDATE ActivityCategory
                                                        SET Name = 'Forward'
                                                        WHERE (Name= 'Ship Data')

                                                        UPDATE ActivityCategory
                                                        SET Name = 'Get'
                                                        WHERE (Name= 'Get Data')

                                                        UPDATE ActivityCategory
                                                        SET Name = 'Monitor'
                                                        WHERE (Name= 'Triggers')";
        public override void Up()
        {
            Sql(UpdateCategoryNamesQuery);
        }

        public override void Down()
        {
            Sql(RevertCategoryNamesQuery);
        }
    }
}
