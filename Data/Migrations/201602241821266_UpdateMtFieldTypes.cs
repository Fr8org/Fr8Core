namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateMtFieldTypes : System.Data.Entity.Migrations.DbMigration
    {
        const string SqlToRun = @"
            declare @typeId int 
            select @typeId = Id from MT_FieldType where TypeName = '{0}' and AssemblyName = '{1}';

            if @typeId is null
            begin
            DECLARE @tempVar table (Id int);
             insert MT_FieldType output inserted.Id into @tempVar values ('{0}', '{1}')
             select @typeId = Id from @tempVar
            end 
  
            update MT_Fields
             set
             MT_Fields.MT_FieldType_Id = @typeId
              from MT_Fields
             inner join MT_Objects 
             on MT_Fields.MT_ObjectId = MT_Objects.Id
             inner join MT_FieldType
             on MT_Objects.MT_FieldType_Id = MT_FieldType.Id
             where MT_FieldType.TypeName = 'Data.Interfaces.Manifests.DocuSignEnvelopeCM' 
             and (MT_Fields.Name = 'CompletedDate' or 
	              MT_Fields.Name = 'CreateDate'  or 
	              MT_Fields.Name = 'DeliveredDate' or 
	              MT_Fields.Name = 'SentDate' or 
	              MT_Fields.Name = 'StatusChangedDateTime')
 
             update MT_Fields
             set
             MT_Fields.MT_FieldType_Id = @typeId
              from MT_Fields
             inner join MT_Objects 
             on MT_Fields.MT_ObjectId = MT_Objects.Id
             inner join MT_FieldType
             on MT_Objects.MT_FieldType_Id = MT_FieldType.Id
             where MT_FieldType.TypeName = 'Data.Interfaces.Manifests.DocuSignTemplateCM' 
             and (MT_Fields.Name = 'CreateDate')";

        public override void Up()
        {
            AlterDateTimeColumns(typeof(DateTime?));
        }
        
        public override void Down()
        {
            AlterDateTimeColumns(typeof(string));
        }

        private void AlterDateTimeColumns(Type targetType)
        {
            var typeName = targetType.FullName;
            var assemblyName = targetType.Assembly.FullName;

            Sql(string.Format(SqlToRun, typeName, assemblyName));
        }
    }
}
