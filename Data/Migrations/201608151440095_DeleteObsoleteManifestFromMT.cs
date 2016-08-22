namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class DeleteObsoleteManifestFromMT : DbMigration
    {
        public override void Up()
        {
            Sql(
                     @"declare @type nvarchar(max), @ObsoleteProperty nvarchar(max);

                        select @type = Id
                        FROM [dbo].[MtTypes]
                        where Alias = 'Docusign Recipient'

                        select @ObsoleteProperty = Name
                        FROM [dbo].[MtProperties]
                        where DeclaringType=@type and Name = 'Object'

                        if (@ObsoleteProperty is not null)

                        Begin 

                            Delete
                              FROM [dbo].[MtProperties]
                              where DeclaringType=@type

                            Delete
                            from [dbo].[MtData]
                            where type = @type

                            Delete FROM [dbo].[MtTypes]
                            where Alias = 'Docusign Recipient'

                        end");
        }


        public override void Down()
        {
            //no need for that
        }
    }
}
