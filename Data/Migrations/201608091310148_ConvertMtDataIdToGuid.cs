namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ConvertMtDataIdToGuid : System.Data.Entity.Migrations.DbMigration
    {
        private const string CreateMTDataTableQuery = @"CREATE TABLE [dbo].[MtData](
	[Id] [uniqueidentifier] NOT NULL DEFAULT(newid()),
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
    [OldId] [nvarchar](max) NULL,
	 CONSTRAINT [PK_dbo.MTData_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

        private const string CopyDataFromOldMtDataQuery = @"INSERT INTO [dbo].[MTData] (
                    [Id],
	                [Type],
	                [CreatedAt],
	                [UpdatedAt],
	                [fr8AccountId],
	                [IsDeleted],
	                [Value1],
	                [Value2],
	                [Value3],
	                [Value4],
	                [Value5],
	                [Value6],
	                [Value7],
	                [Value8],
	                [Value9],
	                [Value10],
	                [Value11],
	                [Value12],
	                [Value13],
	                [Value14],
	                [Value15],
	                [Value16],
	                [Value17],
	                [Value18],
	                [Value19],
	                [Value20],
	                [Value21],
	                [Value22],
	                [Value23],
	                [Value24],
	                [Value25],
	                [Value26],
	                [Value27],
	                [Value28],
	                [Value29],
	                [Value30],
	                [Value31],
	                [Value32],
	                [Value33],
	                [Value34],
	                [Value35],
	                [Value36],
	                [Value37],
	                [Value38],
	                [Value39],
	                [Value40],
	                [Value41],
	                [Value42],
	                [Value43],
	                [Value44],
	                [Value45],
	                [Value46],
	                [Value47],
	                [Value48],
	                [Value49],
	                [Value50],
                    [OldId])
                SELECT
                    newid() as [Id],
	                [mt].[Type],
	                [mt].[CreatedAt],
	                [mt].[UpdatedAt],
	                [mt].[fr8AccountId],
	                [mt].[IsDeleted],
	                [mt].[Value1],
	                [mt].[Value2],
	                [mt].[Value3],
	                [mt].[Value4],
	                [mt].[Value5],
	                [mt].[Value6],
	                [mt].[Value7],
	                [mt].[Value8],
	                [mt].[Value9],
	                [mt].[Value10],
	                [mt].[Value11],
	                [mt].[Value12],
	                [mt].[Value13],
	                [mt].[Value14],
	                [mt].[Value15],
	                [mt].[Value16],
	                [mt].[Value17],
	                [mt].[Value18],
	                [mt].[Value19],
	                [mt].[Value20],
	                [mt].[Value21],
	                [mt].[Value22],
	                [mt].[Value23],
	                [mt].[Value24],
	                [mt].[Value25],
	                [mt].[Value26],
	                [mt].[Value27],
	                [mt].[Value28],
	                [mt].[Value29],
	                [mt].[Value30],
	                [mt].[Value31],
	                [mt].[Value32],
	                [mt].[Value33],
	                [mt].[Value34],
	                [mt].[Value35],
	                [mt].[Value36],
	                [mt].[Value37],
	                [mt].[Value38],
	                [mt].[Value39],
	                [mt].[Value40],
	                [mt].[Value41],
	                [mt].[Value42],
	                [mt].[Value43],
	                [mt].[Value44],
	                [mt].[Value45],
	                [mt].[Value46],
	                [mt].[Value47],
	                [mt].[Value48],
	                [mt].[Value49],
	                [mt].[Value50],
                    [mt].[Id] as [OldId]
                FROM [dbo].[OldMTData] AS [mt]";
        public override void Up()
        {
            RenameTable("dbo.MTData", "OldMTData");
            Sql(CreateMTDataTableQuery);
            Sql(CopyDataFromOldMtDataQuery);

            //Copy ids of PlanTemplates to ObjectRolePermissions
            Sql(@"UPDATE o SET o.ObjectId = t.Id
                    FROM ObjectRolePermissions o
                    INNER JOIN MtData t ON
                    o.ObjectId = t.OldId
		            WHERE LEN(ObjectId) != 36 AND o.Type = 'Plan Template'");

            //Remove leftovers
            DropColumn("dbo.MtData", "OldId");
            DropTable("dbo.OldMTData");
        }

        public override void Down()
        {
        }
    }
}
