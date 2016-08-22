namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoveToSqlMt : System.Data.Entity.Migrations.DbMigration
    {
        const string DropNewStructure = @"
IF OBJECT_ID('dbo.MtData', 'U') IS NOT NULL 
    DROP TABLE [dbo].[MtData]
IF OBJECT_ID('dbo.MtProperties', 'U') IS NOT NULL 
    DROP TABLE [dbo].[MtProperties]
IF OBJECT_ID('dbo.MtTypes', 'U') IS NOT NULL 
DROP TABLE [dbo].[MtTypes]
";

        const string CreateNewMtStructure = @"
CREATE TABLE [MtData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Type] [uniqueidentifier] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
	[fr8AccountId] [nvarchar](max) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[Value1] [nvarchar](max) NULL,
	[Value2] [nvarchar](max) NULL,
	[Value3] [nvarchar](max) NULL,
	[Value4] [nvarchar](max) NULL,
	[Value5] [nvarchar](max) NULL,
	[Value6] [nvarchar](max) NULL,
	[Value7] [nvarchar](max) NULL,
	[Value8] [nvarchar](max) NULL,
	[Value9] [nvarchar](max) NULL,
	[Value10] [nvarchar](max) NULL,
	[Value11] [nvarchar](max) NULL,
	[Value12] [nvarchar](max) NULL,
	[Value13] [nvarchar](max) NULL,
	[Value14] [nvarchar](max) NULL,
	[Value15] [nvarchar](max) NULL,
	[Value16] [nvarchar](max) NULL,
	[Value17] [nvarchar](max) NULL,
	[Value18] [nvarchar](max) NULL,
	[Value19] [nvarchar](max) NULL,
	[Value20] [nvarchar](max) NULL,
	[Value21] [nvarchar](max) NULL,
	[Value22] [nvarchar](max) NULL,
	[Value23] [nvarchar](max) NULL,
	[Value24] [nvarchar](max) NULL,
	[Value25] [nvarchar](max) NULL,
	[Value26] [nvarchar](max) NULL,
	[Value27] [nvarchar](max) NULL,
	[Value28] [nvarchar](max) NULL,
	[Value29] [nvarchar](max) NULL,
	[Value30] [nvarchar](max) NULL,
	[Value31] [nvarchar](max) NULL,
	[Value32] [nvarchar](max) NULL,
	[Value33] [nvarchar](max) NULL,
	[Value34] [nvarchar](max) NULL,
	[Value35] [nvarchar](max) NULL,
	[Value36] [nvarchar](max) NULL,
	[Value37] [nvarchar](max) NULL,
	[Value38] [nvarchar](max) NULL,
	[Value39] [nvarchar](max) NULL,
	[Value40] [nvarchar](max) NULL,
	[Value41] [nvarchar](max) NULL,
	[Value42] [nvarchar](max) NULL,
	[Value43] [nvarchar](max) NULL,
	[Value44] [nvarchar](max) NULL,
	[Value45] [nvarchar](max) NULL,
	[Value46] [nvarchar](max) NULL,
	[Value47] [nvarchar](max) NULL,
	[Value48] [nvarchar](max) NULL,
	[Value49] [nvarchar](max) NULL,
	[Value50] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.MTData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [MtProperties](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Offset] [int] NOT NULL,
	[Type] [uniqueidentifier] NOT NULL,
	[DeclaringType] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_MtProperties] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [MtTypes](
	[Id] [uniqueidentifier] NOT NULL,
	[Alias] [nvarchar](200) NULL,
	[ClrName] [nvarchar](1000) NULL,
	[IsPrimitive] [bit] NOT NULL,
	[IsComplex] [bit] NOT NULL,
	[ManifestId] [int] NULL,
 CONSTRAINT [PK_MtTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
";

        public override void Up()
        {
            Sql(CreateNewMtStructure);
            /*Sql(MigrateDataToNewStructure);

            DropForeignKey("dbo.MT_Fields", "MT_FieldType_Id", "dbo.MT_FieldType");
            DropForeignKey("dbo.MT_Objects", "MT_FieldType_Id", "dbo.MT_FieldType");
            DropForeignKey("dbo.MT_Fields", "MT_ObjectId", "dbo.MT_Objects");
            DropForeignKey("dbo.MT_Data", "MT_ObjectId", "dbo.MT_Objects");
            DropIndex("dbo.MT_Fields", "FieldColumnOffsetIndex");
            DropIndex("dbo.MT_Fields", new[] { "MT_FieldType_Id" });
            DropIndex("dbo.MT_Objects", new[] { "MT_FieldType_Id" });
            DropIndex("dbo.MT_Data", new[] { "MT_ObjectId" });
            DropTable("dbo.MT_Fields");
            DropTable("dbo.MT_FieldType");
            DropTable("dbo.MT_Objects");
            DropTable("dbo.MT_Data");*/
        }
        
        public override void Down()
        {
            Sql(DropNewStructure);

           /* CreateTable(
                "dbo.MT_Data",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GUID = c.Guid(nullable: false, identity: true),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        MT_ObjectId = c.Int(nullable: false),
                        fr8AccountId = c.String(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        Value1 = c.String(),
                        Value2 = c.String(),
                        Value3 = c.String(),
                        Value4 = c.String(),
                        Value5 = c.String(),
                        Value6 = c.String(),
                        Value7 = c.String(),
                        Value8 = c.String(),
                        Value9 = c.String(),
                        Value10 = c.String(),
                        Value11 = c.String(),
                        Value12 = c.String(),
                        Value13 = c.String(),
                        Value14 = c.String(),
                        Value15 = c.String(),
                        Value16 = c.String(),
                        Value17 = c.String(),
                        Value18 = c.String(),
                        Value19 = c.String(),
                        Value20 = c.String(),
                        Value21 = c.String(),
                        Value22 = c.String(),
                        Value23 = c.String(),
                        Value24 = c.String(),
                        Value25 = c.String(),
                        Value26 = c.String(),
                        Value27 = c.String(),
                        Value28 = c.String(),
                        Value29 = c.String(),
                        Value30 = c.String(),
                        Value31 = c.String(),
                        Value32 = c.String(),
                        Value33 = c.String(),
                        Value34 = c.String(),
                        Value35 = c.String(),
                        Value36 = c.String(),
                        Value37 = c.String(),
                        Value38 = c.String(),
                        Value39 = c.String(),
                        Value40 = c.String(),
                        Value41 = c.String(),
                        Value42 = c.String(),
                        Value43 = c.String(),
                        Value44 = c.String(),
                        Value45 = c.String(),
                        Value46 = c.String(),
                        Value47 = c.String(),
                        Value48 = c.String(),
                        Value49 = c.String(),
                        Value50 = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MT_Objects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ManifestId = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                        MT_FieldType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MT_FieldType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TypeName = c.String(nullable: false),
                        AssemblyName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MT_Fields",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        FieldColumnOffset = c.Int(nullable: false),
                        MT_ObjectId = c.Int(nullable: false),
                        MT_FieldType_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.MT_Data", "MT_ObjectId");
            CreateIndex("dbo.MT_Objects", "MT_FieldType_Id");
            CreateIndex("dbo.MT_Fields", "MT_FieldType_Id");
            CreateIndex("dbo.MT_Fields", new[] { "MT_ObjectId", "Name", "FieldColumnOffset" }, name: "FieldColumnOffsetIndex");
            AddForeignKey("dbo.MT_Data", "MT_ObjectId", "dbo.MT_Objects", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MT_Fields", "MT_ObjectId", "dbo.MT_Objects", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MT_Objects", "MT_FieldType_Id", "dbo.MT_FieldType", "Id");
            AddForeignKey("dbo.MT_Fields", "MT_FieldType_Id", "dbo.MT_FieldType", "Id", cascadeDelete: true);*/
        }
    }
}
