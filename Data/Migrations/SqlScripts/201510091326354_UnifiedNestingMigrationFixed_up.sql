-- Remove auto incremented Id from ProcessNodeTemplates
GO
ALTER TABLE dbo.ProcessNodeTemplates
	DROP CONSTRAINT [FK_dbo.ProcessNodeTemplates_dbo.ActivityDOes_Id]
GO
ALTER TABLE dbo.ActivityDOes SET (LOCK_ESCALATION = TABLE)
GO
 
--select Has_Perms_By_Name(N'dbo.ActivityDOes', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.ActivityDOes', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.ActivityDOes', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
--GO
ALTER TABLE dbo.ProcessNodeTemplates
	DROP CONSTRAINT DF__ProcessNo__Start__76619304
GO
CREATE TABLE dbo.Tmp_ProcessNodeTemplates
	(
	Id int NOT NULL,
	Name nvarchar(MAX) NULL,
	NodeTransitions nvarchar(MAX) NULL,
	StartingProcessNodeTemplate bit NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_ProcessNodeTemplates SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_ProcessNodeTemplates ADD CONSTRAINT
	DF__ProcessNo__Start__76619304 DEFAULT ((0)) FOR StartingProcessNodeTemplate
GO
IF EXISTS(SELECT * FROM dbo.ProcessNodeTemplates)
	 EXEC('INSERT INTO dbo.Tmp_ProcessNodeTemplates (Id, Name, NodeTransitions, StartingProcessNodeTemplate)
		SELECT Id, Name, NodeTransitions, StartingProcessNodeTemplate FROM dbo.ProcessNodeTemplates WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE dbo.Criteria
	DROP CONSTRAINT [FK_dbo.Criteria_dbo.ProcessNodeTemplates_ProcessNodeTemplateId]
GO
ALTER TABLE dbo.ProcessNodes
	DROP CONSTRAINT [FK_dbo.ProcessNodes_dbo.ProcessNodeTemplates_ProcessNodeTemplateId]
GO
ALTER TABLE dbo.Actions
	DROP CONSTRAINT [FK_dbo.Actions_dbo.ProcessNodeTemplates_ProcessNodeTemplateDO_Id]
GO
DROP TABLE dbo.ProcessNodeTemplates
GO
EXECUTE sp_rename N'dbo.Tmp_ProcessNodeTemplates', N'ProcessNodeTemplates', 'OBJECT' 
GO
ALTER TABLE dbo.ProcessNodeTemplates ADD CONSTRAINT
	[PK_dbo.ProcessNodeTemplates] PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_Id ON dbo.ProcessNodeTemplates
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.ProcessNodeTemplates ADD CONSTRAINT
	[FK_dbo.ProcessNodeTemplates_dbo.ActivityDOes_Id] FOREIGN KEY
	(
	Id
	) REFERENCES dbo.ActivityDOes
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
 
--select Has_Perms_By_Name(N'dbo.ProcessNodeTemplates', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.ProcessNodeTemplates', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.ProcessNodeTemplates', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION

ALTER TABLE dbo.Actions ADD CONSTRAINT
	[FK_dbo.Actions_dbo.ProcessNodeTemplates_ProcessNodeTemplateDO_Id] FOREIGN KEY
	(
	ProcessNodeTemplateDO_Id
	) REFERENCES dbo.ProcessNodeTemplates
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Actions SET (LOCK_ESCALATION = TABLE)
GO
 
--select Has_Perms_By_Name(N'dbo.Actions', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Actions', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Actions', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION

ALTER TABLE dbo.ProcessNodes ADD CONSTRAINT
	[FK_dbo.ProcessNodes_dbo.ProcessNodeTemplates_ProcessNodeTemplateId] FOREIGN KEY
	(
	ProcessNodeTemplateId
	) REFERENCES dbo.ProcessNodeTemplates
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProcessNodes SET (LOCK_ESCALATION = TABLE)
GO
 
--select Has_Perms_By_Name(N'dbo.ProcessNodes', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.ProcessNodes', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.ProcessNodes', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION

ALTER TABLE dbo.Criteria ADD CONSTRAINT
	[FK_dbo.Criteria_dbo.ProcessNodeTemplates_ProcessNodeTemplateId] FOREIGN KEY
	(
	ProcessNodeTemplateId
	) REFERENCES dbo.ProcessNodeTemplates
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Criteria SET (LOCK_ESCALATION = TABLE)
GO
 
-- Remove auto incremented ID from ProcessTemplates

GO
ALTER TABLE dbo.ProcessTemplates
	DROP CONSTRAINT [FK_dbo.ProcessTemplates_dbo._ProcessTemplateStateTemplate_ProcessTemplateState]
GO
ALTER TABLE dbo._ProcessTemplateStateTemplate SET (LOCK_ESCALATION = TABLE)
GO
 
--select Has_Perms_By_Name(N'dbo._ProcessTemplateStateTemplate', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo._ProcessTemplateStateTemplate', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo._ProcessTemplateStateTemplate', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
--GO
ALTER TABLE dbo.ProcessTemplates
	DROP CONSTRAINT [FK_dbo.ProcessTemplates_dbo.Users_DockyardAccount_Id]
GO
ALTER TABLE dbo.Users SET (LOCK_ESCALATION = TABLE)
GO
 
--select Has_Perms_By_Name(N'dbo.Users', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Users', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Users', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
--GO
ALTER TABLE dbo.ProcessTemplates
	DROP CONSTRAINT [FK_dbo.ProcessTemplates_dbo.ActivityDOes_Id]
GO
ALTER TABLE dbo.ActivityDOes SET (LOCK_ESCALATION = TABLE)
GO
 
--select Has_Perms_By_Name(N'dbo.ActivityDOes', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.ActivityDOes', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.ActivityDOes', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
--GO
CREATE TABLE dbo.Tmp_ProcessTemplates
	(
	Id int NOT NULL,
	Name nvarchar(MAX) NOT NULL,
	Description nvarchar(MAX) NULL,
	ProcessTemplateState int NOT NULL,
	DockyardAccount_Id nvarchar(128) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_ProcessTemplates SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM dbo.ProcessTemplates)
	 EXEC('INSERT INTO dbo.Tmp_ProcessTemplates (Id, Name, Description, ProcessTemplateState, DockyardAccount_Id)
		SELECT Id, Name, Description, ProcessTemplateState, DockyardAccount_Id FROM dbo.ProcessTemplates WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE dbo.Processes
	DROP CONSTRAINT [FK_dbo.Processes_dbo.ProcessTemplates_ProcessTemplateId]
GO
DROP TABLE dbo.ProcessTemplates
GO
EXECUTE sp_rename N'dbo.Tmp_ProcessTemplates', N'ProcessTemplates', 'OBJECT' 
GO
ALTER TABLE dbo.ProcessTemplates ADD CONSTRAINT
	[PK_dbo.ProcessTemplates] PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_ProcessTemplateState ON dbo.ProcessTemplates
	(
	ProcessTemplateState
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_DockyardAccount_Id ON dbo.ProcessTemplates
	(
	DockyardAccount_Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Id ON dbo.ProcessTemplates
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.ProcessTemplates ADD CONSTRAINT
	[FK_dbo.ProcessTemplates_dbo.ActivityDOes_Id] FOREIGN KEY
	(
	Id
	) REFERENCES dbo.ActivityDOes
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProcessTemplates ADD CONSTRAINT
	[FK_dbo.ProcessTemplates_dbo.Users_DockyardAccount_Id] FOREIGN KEY
	(
	DockyardAccount_Id
	) REFERENCES dbo.Users
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProcessTemplates ADD CONSTRAINT
	[FK_dbo.ProcessTemplates_dbo._ProcessTemplateStateTemplate_ProcessTemplateState] FOREIGN KEY
	(
	ProcessTemplateState
	) REFERENCES dbo._ProcessTemplateStateTemplate
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
 
--select Has_Perms_By_Name(N'dbo.ProcessTemplates', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.ProcessTemplates', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.ProcessTemplates', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
--GO
ALTER TABLE dbo.Processes ADD CONSTRAINT
	[FK_dbo.Processes_dbo.ProcessTemplates_ProcessTemplateId] FOREIGN KEY
	(
	ProcessTemplateId
	) REFERENCES dbo.ProcessTemplates
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Processes SET (LOCK_ESCALATION = TABLE)
GO
 
