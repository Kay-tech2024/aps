-- Added New Table DegreeTypes
IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[DegreeTypes]') AND type in (N'U'))

BEGIN
		CREATE TABLE [dbo].[DegreeTypes](
			[Id] [bigint] IDENTITY(1,1) NOT NULL,
			[NameArabic] [nvarchar](255) NOT NULL,
			[NameEnglish] [nvarchar](255) NOT NULL,
		 CONSTRAINT [PK_DegreeType] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]

		INSERT [dbo].[DegreeTypes] ([NameArabic], [NameEnglish]) VALUES (N'بكالوريوس', 'Bachelor')
		INSERT [dbo].[DegreeTypes] ([NameArabic], [NameEnglish]) VALUES (N'ماجستير', 'Master')
		INSERT [dbo].[DegreeTypes] ([NameArabic], [NameEnglish]) VALUES (N'دكتوراه', 'Phd')

END

GO


-- Date: 01/14/2018
-- Added new columns to Users Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Users]') 
         AND name = 'ScientificDepartmentOther'
)
ALTER TABLE Users ADD [ScientificDepartmentOther] [NVARCHAR](255) NULL;
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Users]') 
         AND name = 'CurrentWorkPlace'
)
ALTER TABLE Users ADD [CurrentWorkPlace] [NVARCHAR](255) NULL;
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Users]') 
         AND name = 'DegreeTypeId'
)
ALTER TABLE dbo.Users ADD DegreeTypeId BIGINT NULL;
GO

IF NOT EXISTS 
(
    SELECT * FROM sys.foreign_key_columns fk 
    INNER JOIN sys.columns pc ON pc.object_id = fk.parent_object_id AND pc.column_id = fk.parent_column_id 
    INNER JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fk.referenced_column_id
    WHERE fk.parent_object_id = object_id('Users') AND pc.name = 'DegreeTypeId'
    AND fk.referenced_object_id = object_id('DegreeTypes') AND rc.NAME = 'Id'
)
ALTER TABLE Users 
ADD CONSTRAINT FK_Users_DegreeTypes FOREIGN KEY (DegreeTypeId)
REFERENCES DegreeTypes(Id)



IF NOT EXISTS 
(
    SELECT * FROM sys.foreign_key_columns fk 
    INNER JOIN sys.columns pc ON pc.object_id = fk.parent_object_id AND pc.column_id = fk.parent_column_id 
    INNER JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fk.referenced_column_id
    WHERE fk.parent_object_id = object_id('Users') AND pc.name = 'DegreeTypeId'
    AND fk.referenced_object_id = object_id('DegreeTypes') AND rc.NAME = 'Id'
)
ALTER TABLE Users 
ADD CONSTRAINT FK_Users_DegreeTypes FOREIGN KEY (DegreeTypeId)
REFERENCES DegreeTypes(Id)

-- Date: 01/15/2018
-- Added new columns to Requests Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Requests]') 
         AND name = 'IsArabic'
)
ALTER TABLE Requests ADD [IsArabic] BIT DEFAULT((1));
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Requests]') 
         AND name = 'TranslatedToId'
)
ALTER TABLE Requests ADD [TranslatedToId] BIGINT NULL;
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Requests]') 
         AND name = 'TranslatedTo'
)
ALTER TABLE Requests ADD [TranslatedTo] NVARCHAR(255) NULL;
GO
--DROP TABLE TranslatedToLanguage

-- Added New Table TranslatedToLanguage
IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[TranslatedToLanguage]') AND type in (N'U'))

BEGIN
		CREATE TABLE [dbo].[TranslatedToLanguage](
			[Id] [bigint] IDENTITY(1,1) NOT NULL,
			[NameArabic] [nvarchar](255) NOT NULL,
			[NameEnglish] [nvarchar](255) NOT NULL,
		 CONSTRAINT [PK_TranslatedToLanguage] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]

		INSERT [dbo].[TranslatedToLanguage] ([NameArabic], [NameEnglish]) VALUES (N'الإنجليزية', 'English')
		INSERT [dbo].[TranslatedToLanguage] ([NameArabic], [NameEnglish]) VALUES (N'الأسبانية', 'Spanish')
		INSERT [dbo].[TranslatedToLanguage] ([NameArabic], [NameEnglish]) VALUES (N'ألمانية', 'German')
		INSERT [dbo].[TranslatedToLanguage] ([NameArabic], [NameEnglish]) VALUES (N'لغات أخرى', 'Other')
END

GO

IF NOT EXISTS 
(
    SELECT * FROM sys.foreign_key_columns fk 
    INNER JOIN sys.columns pc ON pc.object_id = fk.parent_object_id AND pc.column_id = fk.parent_column_id 
    INNER JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fk.referenced_column_id
    WHERE fk.parent_object_id = object_id('Requests') AND pc.name = 'TranslatedToId'
    AND fk.referenced_object_id = object_id('TranslatedToLanguage') AND rc.NAME = 'Id'
)
ALTER TABLE Requests 
ADD CONSTRAINT FK_Requests_TranslatedToLanguage FOREIGN KEY (TranslatedToId)
REFERENCES TranslatedToLanguage(Id)

-- Date: 01/17/2018
-- Modified Column Type for IsScientificProduction in Requests Table.
IF EXISTS(SELECT 1 
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Requests' AND COLUMN_NAME = 'IsScientificProduction' AND DATA_TYPE = 'bit')
BEGIN
	ALTER TABLE Requests 
	ALTER COLUMN IsScientificProduction INT NULL
END

-- Added new column to ThesisRequests Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[ThesisRequests]') 
         AND name = 'AssistantSupervisor'
)
ALTER TABLE ThesisRequests ADD [AssistantSupervisor] [NVARCHAR](350) NULL;
GO

-- Date: 01/18/2018
-- Added New Table ContactedUs
IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[ContactedUs]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[ContactedUs](
		[Id] [BIGINT] IDENTITY(1,1) NOT NULL,
		[FullName] [NVARCHAR](255) NOT NULL,
		[EmailAddress] [NVARCHAR](255) NOT NULL,
		[Subject] [NVARCHAR](255) NULL,
		[Message] [NVARCHAR](MAX) NOT NULL,
		[Created] [DATETIME] NOT NULL,
	 CONSTRAINT [PK_ContactedUs] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

END
GO

-- Date: 01/29/2018
-- Added new column to ContactedUs Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[ContactedUs]') 
         AND name = 'IsDeleted'
)
ALTER TABLE ContactedUs ADD [IsDeleted]  BIT DEFAULT((0)) NOT NULL;
GO

-- Date: 01/31/2018
-- Added new column to RequestReferees Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[RequestReferees]') 
         AND name = 'Grade'
)
ALTER TABLE RequestReferees ADD [Grade]  INT DEFAULT((0)) NULL;
GO

-- Date: 02/07/2018
-- Added New Table ResearcherRewards
IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[ResearcherRewards]') AND type in (N'U'))

BEGIN
		CREATE TABLE [dbo].[ResearcherRewards](
			[Id] [bigint] IDENTITY(1,1) NOT NULL,
			[Master] [BIGINT] NOT NULL DEFAULT ((8000)),
			[Phd] [BIGINT] NOT NULL DEFAULT ((15000))
		 CONSTRAINT [PK_ResearcherRewards] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]
		INSERT [dbo].[ResearcherRewards] ([Master], [Phd]) VALUES (8000,15000)
END

GO

-- Added New Table ReviewerRewards
IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[ReviewerRewards]') AND type in (N'U'))

BEGIN
		CREATE TABLE [dbo].[ReviewerRewards](
			[Id] [bigint] IDENTITY(1,1) NOT NULL,
			[Minpages] [BIGINT] NOT NULL,
			[MaxPages] [BIGINT] NOT NULL ,
			[RewardValue] [BIGINT] NOT NULL
		 CONSTRAINT [PK_ReviewerRewards] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]

		INSERT [dbo].[ReviewerRewards] ([Minpages], [MaxPages],[RewardValue]) VALUES (1,100,600)
		INSERT [dbo].[ReviewerRewards] ([Minpages], [MaxPages],[RewardValue]) VALUES (101,200,800)
		INSERT [dbo].[ReviewerRewards] ([Minpages], [MaxPages],[RewardValue]) VALUES (201,300,1000)
		INSERT [dbo].[ReviewerRewards] ([Minpages], [MaxPages],[RewardValue]) VALUES (301,400,1200)
		INSERT [dbo].[ReviewerRewards] ([Minpages], [MaxPages],[RewardValue]) VALUES (401,500,1400)
END

GO
-- Date: 02/11/2018
-- Added new column to Requests Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Requests]') 
         AND name = 'ISBN'
)
ALTER TABLE Requests ADD [ISBN] NVARCHAR(100) NULL;
GO
-- Date: 02/13/2018
-- Added New Table DistributionEntities
IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[DistributionEntities]') AND type in (N'U'))

BEGIN
		CREATE TABLE [dbo].[DistributionEntities](
			[Id] [BIGINT] IDENTITY(1,1) NOT NULL,
			[Name] [NVARCHAR](255) NOT NULL,
			[NoOfCopies] [BIGINT] NOT NULL ,
			[IsDeleted] [BIT] NOT NULL DEFAULT ((0))
		 CONSTRAINT [PK_DistributionEntities] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]
END
GO

-- Date: 02/13/2018
-- Added New Table UserDelegation
IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[UserDelegation]') AND type in (N'U'))

BEGIN
		CREATE TABLE [dbo].[UserDelegation](
			[Id] [BIGINT] IDENTITY(1,1) NOT NULL,
			[DelegatorId] [NVARCHAR](128) NOT NULL,
			[DelegateeId] [NVARCHAR](128) NOT NULL,
			[Email] [NVARCHAR](256) NOT NULL,
			[EffectiveFrom] DateTime NOT NULL,
			[EffectiveTo] DateTime NULL,
			[IsAlways] [BIT] NOT NULL DEFAULT ((0))
		 CONSTRAINT [PK_UserDelegation] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]

		ALTER TABLE [dbo].[UserDelegation]  WITH CHECK ADD FOREIGN KEY([DelegatorId])
		REFERENCES [dbo].[Users] ([Id])

		ALTER TABLE [dbo].[UserDelegation]  WITH CHECK ADD FOREIGN KEY([DelegateeId])
		REFERENCES [dbo].[Users] ([Id])
END
GO

-- Date: 02/15/2018
-- Added new column to DistributionEntities Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[DistributionEntities]') 
         AND name = 'Created'
)
ALTER TABLE DistributionEntities ADD [Created] [DATETIME] NOT NULL
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[DistributionEntities]') 
         AND name = 'CreatedBy'
)
BEGIN
	ALTER TABLE DistributionEntities 
	ADD [CreatedBy] [NVARCHAR](128) NULL,
	CONSTRAINT [FK_DistributionEntities_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) 
    REFERENCES [Users] ([Id]);
END
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[DistributionEntities]') 
         AND name = 'Updated'
)
ALTER TABLE DistributionEntities ADD [Updated] [DATETIME] NOT NULL;
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[DistributionEntities]') 
         AND name = 'UpdatedBy'
)
BEGIN
	ALTER TABLE DistributionEntities 
	ADD [UpdatedBy] [NVARCHAR](128) NULL,
	CONSTRAINT [FK_DistributionEntities_Users_UpdatedBy] FOREIGN KEY ([UpdatedBy]) 
    REFERENCES [Users] ([Id]);
END
GO

-- Added new column to Requests Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Requests]') 
         AND name = 'NoOfCopies'
)
ALTER TABLE Requests ADD [NoOfCopies] [BIGINT] NULL
GO

-- Date: 02/18/2018
-- Added new column to usersessions Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[usersessions]') 
         AND name = 'Id'
)
BEGIN
		DECLARE @table NVARCHAR(512), @sql NVARCHAR(MAX);
		SELECT @table = N'dbo.usersessions';

		SELECT @sql = 'ALTER TABLE ' + @table 
		+ ' DROP CONSTRAINT ' + name + ';'
		FROM sys.key_constraints 
		WHERE [type] = 'PK'
			AND [parent_object_id] = OBJECT_ID(@table);
		EXEC sp_executeSQL @sql;

		ALTER TABLE usersessions ADD Id BIGINT IDENTITY(1,1);
		ALTER TABLE usersessions add primary key (ID)
END


-- Date: 02/19/2018
-- Updated Views for Reports.
DROP VIEW [dbo].[ViewRequestsByTypeReport];
PRINT 'ViewRequestsByTypeReport  is Created.'
GO
CREATE VIEW [dbo].[ViewRequestsByTypeReport] 
AS SELECT * FROM 
(
	select 
			1,
			'Book',  
			N'كتاب', 
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 1),
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 1 and status = 63),
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 1 and status = 51),
			(select count(*) from Requests r1 left join Requests r2 on r1.PublicationRequestId = r2.Id where (r1.type = 6) and r2.PublicationType = 1 and r1.status = 63)	
			
		UNION SELECT
			2,
			'Handbook',
			N'كتيب',
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 2),
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 2 and status = 63),
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 2 and status = 51),
			(select count(*) from Requests r1 left join Requests r2 on r1.PublicationRequestId = r2.Id where (r1.type = 6) and r2.PublicationType = 2 and r1.status = 63)	
		UNION SELECT
			3,
			'Thesis',
			N'الرسائل العلمية',
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 3),
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 3 and status = 63),
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 3 and status = 51),
			(select count(*) from Requests r1 left join Requests r2 on r1.PublicationRequestId = r2.Id where (r1.type = 6) and r2.PublicationType = 3 and r1.status = 63)	

		UNION SELECT
			4,
			'Dictionary',
			N'المعجم',
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 4),
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 4 and status = 63),
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 4 and status = 51),
			(select count(*) from Requests r1 left join Requests r2 on r1.PublicationRequestId = r2.Id where (r1.type = 6) and r2.PublicationType = 4 and r1.status = 63)	

		UNION SELECT
			5,
			'Encyclopedia',
			N'الموسوعة',
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 5),
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 5 and status = 63),
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 5 and status = 51),
			(select count(*) from Requests r1 left join Requests r2 on r1.PublicationRequestId = r2.Id where (r1.type = 6) and r2.PublicationType = 5 and r1.status = 63)	

		UNION SELECT
			6,
			'Probe',
			N'التحقيق',
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 6),
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 6 and status = 63),
			(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 6 and status = 51),
			(select count(*) from Requests r1 left join Requests r2 on r1.PublicationRequestId = r2.Id where (r1.type = 6) and r2.PublicationType = 6 and r1.status = 63)	

) E(PublicationTypeId, PublicationTypeEn,PublicationTypeAr, SubmittedRequests, ApprovedRequests, RejectedRequests, PrintedRequests)
GO

DROP VIEW [dbo].[ViewRequestsByFinancialTypesReport];
PRINT 'ViewRequestsByFinancialTypesReport is Created.'
GO
CREATE VIEW [dbo].[ViewRequestsByFinancialTypesReport] 
AS SELECT * FROM 
	(
		SELECT 
				1,
				'Researcher',  
				N'باحث',  
				(SELECT COUNT(*) FROM Requests WHERE Type = 3),
				(SELECT COUNT(*) FROM Requests WHERE Type = 3 AND Status = 63),
				(SELECT COUNT(*) FROM Requests WHERE Type = 3 AND Status = 51),
				(SELECT ISNULL(SUM(r1.amount),0) FROM RewardRequests r1 LEFT JOIN Requests r2 ON r1.RequestId = r2.Id WHERE r2.Type = 3 AND r2.Status = 63)	
			UNION SELECT
				3,
				'Proofreader',
				N'مصحح',
				(SELECT COUNT(*) FROM Requests WHERE type = 5),
				(SELECT COUNT(*) FROM Requests WHERE type = 5 AND status = 63),
				(SELECT COUNT(*) FROM Requests WHERE type = 5 AND status = 51),
				(SELECT ISNULL(SUM(r1.amount),0) FROM RewardRequests r1 LEFT JOIN Requests r2 ON r1.RequestId = r2.Id WHERE r2.Type = 5 AND r2.Status = 63)	

			UNION SELECT
				4,
				'Referee',
				N'محكم',
				(select count(*) from Requests where type = 4),
				(select count(*) from Requests where type = 4 and status = 63),
				(select count(*) from Requests where type = 4 and status = 51),
				(select ISNULL(SUM(r1.amount),0) from RewardRequests r1 left join Requests r2 on r1.RequestId = r2.Id where r2.type = 4 and r2.status = 63)	

	) E(UserRoleId, UserRoleEn,UserRoleAr, SubmittedRequests, ApprovedRequests, RejectedRequests,  TotalAmount)
GO


---- Added on 2018-02-19 usp_GetRequestsByTypeDetailed
IF EXISTS ( SELECT * 
            FROM   sysobjects 
            WHERE  id = object_id(N'[dbo].[usp_GetRequestsByTypeDetailed]') 
                   and OBJECTPROPERTY(id, N'IsProcedure') = 1 )
BEGIN
    DROP PROCEDURE [dbo].[usp_GetRequestsByTypeDetailed]
END
GO

-- usp_GetRequestsByTypeDetailed 3,1

CREATE PROCEDURE dbo.usp_GetRequestsByTypeDetailed 
	 @PublicationType SMALLINT
	,@RequestStatus SMALLINT
AS
BEGIN
		
	SELECT * FROM ViewRequestsDetails vr
	WHERE 
		(@PublicationType = 0 AND @RequestStatus = 1 AND (vr.Type = 1 OR vr.Type = 2)) OR
		(@PublicationType = 0 AND @RequestStatus = 2 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63) OR
		(@PublicationType = 0 AND @RequestStatus = 3 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 51) OR
		(@PublicationType = 0 AND @RequestStatus = 4 AND (vr.Type = 1 OR vr.Type = 2) AND vr.IsPrinted = 1) OR
		
		
		
		(@PublicationType = 1 AND vr.PublicationType = 1 AND @RequestStatus = 1 AND (vr.Type = 1 OR vr.Type = 2)) OR
		(@PublicationType = 1 AND vr.PublicationType = 1 AND @RequestStatus = 2 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63) OR
		(@PublicationType = 1 AND vr.PublicationType = 1 AND @RequestStatus = 3 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 51) OR
		(@PublicationType = 1 AND vr.PublicationType = 1 AND @RequestStatus = 4 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63 AND vr.IsPrinted = 1) OR
		(@PublicationType = 2 AND vr.PublicationType = 2 AND @RequestStatus = 1 AND (vr.Type = 1 OR vr.Type = 2)) OR
		(@PublicationType = 2 AND vr.PublicationType = 2 AND @RequestStatus = 2 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63) OR
		(@PublicationType = 2 AND vr.PublicationType = 2 AND @RequestStatus = 3 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 51) OR
		(@PublicationType = 2 AND vr.PublicationType = 2 AND @RequestStatus = 4 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63 AND vr.IsPrinted = 1) OR
		(@PublicationType = 3 AND vr.PublicationType = 3 AND @RequestStatus = 1 AND (vr.Type = 1 OR vr.Type = 2)) OR
		(@PublicationType = 3 AND vr.PublicationType = 3 AND @RequestStatus = 2 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63) OR
		(@PublicationType = 3 AND vr.PublicationType = 3 AND @RequestStatus = 3 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 51) OR
		(@PublicationType = 3 AND vr.PublicationType = 3 AND @RequestStatus = 4 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63 AND vr.IsPrinted = 1) OR
		(@PublicationType = 4 AND vr.PublicationType = 4 AND @RequestStatus = 1 AND (vr.Type = 1 OR vr.Type = 2)) OR
		(@PublicationType = 4 AND vr.PublicationType = 4 AND @RequestStatus = 2 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63) OR
		(@PublicationType = 4 AND vr.PublicationType = 4 AND @RequestStatus = 3 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 51) OR
		(@PublicationType = 4 AND vr.PublicationType = 4 AND @RequestStatus = 4 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63 AND vr.IsPrinted = 1) OR
		(@PublicationType = 5 AND vr.PublicationType = 5 AND @RequestStatus = 1 AND (vr.Type = 1 OR vr.Type = 2)) OR
		(@PublicationType = 5 AND vr.PublicationType = 5 AND @RequestStatus = 2 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63) OR
		(@PublicationType = 5 AND vr.PublicationType = 5 AND @RequestStatus = 3 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 51) OR
		(@PublicationType = 5 AND vr.PublicationType = 5 AND @RequestStatus = 4 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63 AND vr.IsPrinted = 1) OR
		(@PublicationType = 6 AND vr.PublicationType = 6 AND @RequestStatus = 1 AND (vr.Type = 1 OR vr.Type = 2)) OR
		(@PublicationType = 6 AND vr.PublicationType = 6 AND @RequestStatus = 2 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63) OR
		(@PublicationType = 6 AND vr.PublicationType = 6 AND @RequestStatus = 3 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 51) OR
		(@PublicationType = 6 AND vr.PublicationType = 6 AND @RequestStatus = 4 AND (vr.Type = 1 OR vr.Type = 2) AND vr.Status = 63 AND vr.IsPrinted = 1)
END

---- Added on 2018-02-19 usp_GetRequestsByFinancialDetailed
IF EXISTS ( SELECT * 
            FROM   sysobjects 
            WHERE  id = object_id(N'[dbo].[usp_GetRequestsByFinancialDetailed]') 
                   and OBJECTPROPERTY(id, N'IsProcedure') = 1 )
BEGIN
    DROP PROCEDURE [dbo].[usp_GetRequestsByTypeDetailed]
END
GO
-- usp_GetRequestsByFinancialDetailed 3,1
CREATE PROCEDURE dbo.usp_GetRequestsByFinancialDetailed 
	 @UserRole SMALLINT
	,@RequestStatus SMALLINT
AS
BEGIN
		
	SELECT * FROM ViewRewardRequestsDetails vr
	WHERE 
		(@UserRole = 0 AND @RequestStatus = 1) OR
		(@UserRole = 0 AND @RequestStatus = 2 AND vr.Status = 63) OR
		(@UserRole = 0 AND @RequestStatus = 3 AND vr.Status = 51) OR
		
		(@UserRole = 1 AND @RequestStatus = 1 AND vr.Type = 3) OR
		(@UserRole = 1 AND @RequestStatus = 2 AND vr.Type = 3 AND vr.Status = 63) OR
		(@UserRole = 1 AND @RequestStatus = 3 AND vr.Type = 3 AND vr.Status = 51) OR
		
		(@UserRole = 3 AND @RequestStatus = 1 AND vr.Type = 4) OR
		(@UserRole = 3 AND @RequestStatus = 2 AND vr.Type = 4 AND vr.Status = 63) OR
		(@UserRole = 3 AND @RequestStatus = 3 AND vr.Type = 4 AND vr.Status = 51) OR
		
		(@UserRole = 4 AND @RequestStatus = 1 AND vr.Type = 5) OR
		(@UserRole = 4 AND @RequestStatus = 2 AND vr.Type = 5 AND vr.Status = 63) OR
		(@UserRole = 4 AND @RequestStatus = 3 AND vr.Type = 5 AND vr.Status = 51)
END
-- Date: 02/26/2018
-- Added new columns to Template Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Templates]') 
         AND name = 'FilePath'
)
ALTER TABLE Templates ADD [FilePath] [NVARCHAR](1000) NULL;
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Templates]') 
         AND name = 'FileName'
)
ALTER TABLE Templates ADD [FileName] [NVARCHAR](205) NULL;
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Templates]') 
         AND name = 'Extension'
)
ALTER TABLE Templates ADD [Extension] [NVARCHAR](5) NULL;
GO

-- Date: 04/04/2018
-- Added New Table UserSessionData
IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[UserSessionData]') AND type in (N'U'))

BEGIN
		CREATE TABLE [dbo].[UserSessionData](
			[Id] [bigint] IDENTITY(1,1) NOT NULL,
			[SessionId] [bigint] NOT NULL,
			[Key] [nvarchar](255) NOT NULL,
			[Type] [nvarchar](255) NOT NULL,
			[Value] [ntext] NULL,
		PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
		) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

-- Added new column to UserSessions Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[UserSessions]') 
         AND name = 'EmailVerified'
)
ALTER TABLE UserSessions ADD [EmailVerified] [NVARCHAR](255) NULL
GO

-- Date: 04/05/2018
-- Added New Table UserSessionFiles

IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[UserSessionFiles]') AND type in (N'U'))
BEGIN
		CREATE TABLE [dbo].[UserSessionFiles](
			[Id] [uniqueidentifier] NOT NULL,
			[SessionId] [bigint] NOT NULL,
			[FileName] [nvarchar](255) NOT NULL,
			[FilePath] [nvarchar](1000) NULL,
			[Type] [tinyint] NOT NULL,
			[Description] [nvarchar](500) NULL,
			[Extension] [nvarchar](5) NOT NULL,
			[Created] [datetime] NOT NULL,
			[CreatedBy] [nvarchar](128) NOT NULL
		) ON [PRIMARY]

		ALTER TABLE [dbo].[UserSessionFiles]  WITH CHECK ADD FOREIGN KEY([SessionId])
		REFERENCES [dbo].[UserSessions] ([Id])
END
GO

-- Added new columns to Researchers Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Researchers]') 
         AND name = 'NationalId'
)
ALTER TABLE Researchers ADD [NationalId] [NVARCHAR](50) NULL
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Researchers]') 
         AND name = 'Qualification'
)
ALTER TABLE Researchers ADD [Qualification] [TINYINT] NULL
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Researchers]') 
         AND name = 'UniversityOther'
)
ALTER TABLE Researchers ADD [UniversityOther] [NVARCHAR](255) NULL
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Researchers]') 
         AND name = 'CollegeOther'
)
ALTER TABLE Researchers ADD [CollegeOther] [NVARCHAR](255) NULL
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Researchers]') 
         AND name = 'DepartmentOther'
)
ALTER TABLE Researchers ADD [DepartmentOther] [NVARCHAR](255) NULL
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Researchers]') 
         AND name = 'Specialization'
)
ALTER TABLE Researchers ADD [Specialization] [NVARCHAR](255) NULL
GO

-- Added New Table UserExtraRoles

IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[UserExtraRoles]') AND type in (N'U'))
BEGIN
		CREATE TABLE [dbo].[UserExtraRoles](
			[Id] [int] IDENTITY(1,1) NOT NULL,
			[UserId] [nvarchar](128) NULL,
			[RoleId] [nvarchar](128) NULL,
			[CollegeId] [int] NULL,
			[IsActive] [bit] NOT NULL,
			[EffectiveFrom] [datetime] NULL,
			[EffectiveTo] [datetime] NULL,
			[Created] [datetime] NOT NULL,
			[Updated] [datetime] NOT NULL,
			[CreatedBy] [nvarchar](128) NOT NULL,
			[UpdatedBy] [nvarchar](128) NOT NULL,
		PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
		) ON [PRIMARY]
END
GO

-- Added new columns to UserFiles Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[UserFiles]') 
         AND name = 'Updated'
)
ALTER TABLE UserFiles ADD [Updated] [DATETIME] NOT NULL
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[UserFiles]') 
         AND name = 'UpdatedBy'
)
ALTER TABLE UserFiles ADD [UpdatedBy] [NVARCHAR](128) NOT NULL
GO

IF NOT EXISTS 
(
    SELECT * FROM sys.foreign_key_columns fk 
    INNER JOIN sys.columns pc ON pc.object_id = fk.parent_object_id AND pc.column_id = fk.parent_column_id 
    INNER JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fk.referenced_column_id
    WHERE fk.parent_object_id = object_id('Departments') AND pc.name = 'CollegeId'
    AND fk.referenced_object_id = object_id('Colleges') AND rc.NAME = 'Id'
)
ALTER TABLE [dbo].[Departments] 
ADD CONSTRAINT [FK_Departments_Colleges] FOREIGN KEY ([CollegeId])
REFERENCES [dbo].[Colleges] ([Id])
GO

-- Added new columns to Departments Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Departments]') 
         AND name = 'Field'
)
ALTER TABLE Departments ADD [Field] [TINYINT] NULL
GO

-- Added new column to UserSessions Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[UserSessions]') 
         AND name = 'LastSmsCodeGenerated'
)
ALTER TABLE UserSessions ADD [LastSmsCodeGenerated] [DATETIME] NULL
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[UserSessions]') 
         AND name = 'LastEmailCodeGenerated'
)
ALTER TABLE UserSessions ADD [LastEmailCodeGenerated] [DATETIME] NULL
GO

-- Added new column to UserTemporaryUrls Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[UserTemporaryUrls]') 
         AND name = 'SessionId'
)
ALTER TABLE UserTemporaryUrls ADD [SessionId] [BIGINT] NULL
GO

-- Date: 04/15/2018
-- Added foreign keys to UserExtraRoles Table 
IF NOT EXISTS 
(
    SELECT * FROM sys.foreign_key_columns fk 
    INNER JOIN sys.columns pc ON pc.object_id = fk.parent_object_id AND pc.column_id = fk.parent_column_id 
    INNER JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fk.referenced_column_id
    WHERE fk.parent_object_id = object_id('UserExtraRoles') AND pc.name = 'CollegeId'
    AND fk.referenced_object_id = object_id('Colleges') AND rc.NAME = 'Id'
)
ALTER TABLE [dbo].[UserExtraRoles] 
ADD CONSTRAINT FK_UserExtraRoles_Colleges FOREIGN KEY ([CollegeId])
REFERENCES [dbo].[Colleges](Id)

IF NOT EXISTS 
(
    SELECT * FROM sys.foreign_key_columns fk 
    INNER JOIN sys.columns pc ON pc.object_id = fk.parent_object_id AND pc.column_id = fk.parent_column_id 
    INNER JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fk.referenced_column_id
    WHERE fk.parent_object_id = object_id('UserExtraRoles') AND pc.name = 'CreatedBy'
    AND fk.referenced_object_id = object_id('Users') AND rc.NAME = 'Id'
)
ALTER TABLE [dbo].[UserExtraRoles] 
ADD CONSTRAINT FK_UserExtraRoles_Users_CreatedBy FOREIGN KEY ([CreatedBy])
REFERENCES [dbo].[Users](Id)

IF NOT EXISTS 
(
    SELECT * FROM sys.foreign_key_columns fk 
    INNER JOIN sys.columns pc ON pc.object_id = fk.parent_object_id AND pc.column_id = fk.parent_column_id 
    INNER JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fk.referenced_column_id
    WHERE fk.parent_object_id = object_id('UserExtraRoles') AND pc.name = 'RoleId'
    AND fk.referenced_object_id = object_id('Roles') AND rc.NAME = 'Id'
)
ALTER TABLE [dbo].[UserExtraRoles] 
ADD CONSTRAINT FK_UserExtraRoles_Roles FOREIGN KEY ([RoleId])
REFERENCES [dbo].[Roles](Id)

IF NOT EXISTS 
(
    SELECT * FROM sys.foreign_key_columns fk 
    INNER JOIN sys.columns pc ON pc.object_id = fk.parent_object_id AND pc.column_id = fk.parent_column_id 
    INNER JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fk.referenced_column_id
    WHERE fk.parent_object_id = object_id('UserExtraRoles') AND pc.name = 'UpdatedBy'
    AND fk.referenced_object_id = object_id('Users') AND rc.NAME = 'Id'
)
ALTER TABLE [dbo].[UserExtraRoles] 
ADD CONSTRAINT FK_UserExtraRoles_Users_UpdatedBy FOREIGN KEY ([UpdatedBy])
REFERENCES [dbo].[Users](Id)

IF NOT EXISTS 
(
    SELECT * FROM sys.foreign_key_columns fk 
    INNER JOIN sys.columns pc ON pc.object_id = fk.parent_object_id AND pc.column_id = fk.parent_column_id 
    INNER JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fk.referenced_column_id
    WHERE fk.parent_object_id = object_id('UserExtraRoles') AND pc.name = 'UserId'
    AND fk.referenced_object_id = object_id('Users') AND rc.NAME = 'Id'
)
ALTER TABLE [dbo].[UserExtraRoles] 
ADD CONSTRAINT FK_UserExtraRoles_Users_UserId FOREIGN KEY ([UserId])
REFERENCES [dbo].[Users](Id)


-- Update Colleges, departments, universities data
update researchers set CollegeId=null, DepartmentId=null, universityid = null
update requests set CollegeId=null
update Referees set CollegeId=null,UniversityId =null
update users set CollegeId = null, departmentid = null
update researchers set collegeid =null, departmentid = null
update Majors set collegeid =null
update Colleges set UniversityId =null
update Users set UniversityId =null

delete from  Departments
delete from  colleges
delete from  universities
go

SET IDENTITY_INSERT universities ON
insert into universities (id, NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (1, N'جامعة الإمام محمد بن سعود الإسلامية', 'Al-Imam Mohammad Ibn Saud Islamic University', getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
SET IDENTITY_INSERT universities OFF
go

INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'المعهد العالي للقضاء', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 1, 1);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'المعهد العالي للاحتساب', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 1, 15);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'الشريعة', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 1, 2);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'أصول الدين', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 1, 3);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'الشريعة بالأحساء', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, NULL, 8);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'علوم الحاسب', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 4, 9);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'الطب', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 4, 13);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'العلوم', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 4, 12);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'الهندسة', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 4, 14);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'اللغة العربية', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 2, 4);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'معهد تعليم اللغة العربية', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 2, 10);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'الاعلام والاتصال', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 3, 5);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'العلوم الاجتماعية', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 3, 6);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'اللغات والترجمة', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 3, 7);
INSERT INTO Colleges (NameArabic, NameEnglish, UniversityId, Created, Updated, CreatedBy, UpdatedBy, IsDeleted, Field, ResearchUnitCode) VALUES	(N'الاقتصاد والعلوم الإدارية', NULL, 1, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0, 3, 11);
go

insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم القران الكريم وعلومه', NULL, (select id from colleges where ResearchUnitCode = 3), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم السنة النبوية', NULL, (select id from colleges where ResearchUnitCode = 3), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم العقيدة والمذاهب المعاصرة', NULL, (select id from colleges where ResearchUnitCode = 3), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم القضاء العمالي', NULL, (select id from colleges where ResearchUnitCode = 1), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الفقه المقارن', NULL, (select id from colleges where ResearchUnitCode = 1), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم السياسة الشرعية', NULL, (select id from colleges where ResearchUnitCode = 1), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم القضاء التجاري', NULL, (select id from colleges where ResearchUnitCode = 1), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم القضاء الجزائي', NULL, (select id from colleges where ResearchUnitCode = 1), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم قضاء الأحوال الشخصية', NULL, (select id from colleges where ResearchUnitCode = 1), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم القضاء الإداري', NULL, (select id from colleges where ResearchUnitCode = 1), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'الدراسات الإسلامية المعاصرة', NULL, (select id from colleges where ResearchUnitCode = 15), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الدعوة', NULL, (select id from colleges where ResearchUnitCode = 15), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الحسبة', NULL, (select id from colleges where ResearchUnitCode = 15), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الفقه', NULL, (select id from colleges where ResearchUnitCode = 2), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم أصول الفقه', NULL, (select id from colleges where ResearchUnitCode = 2), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الثقافة الإسلامية', NULL, (select id from colleges where ResearchUnitCode = 2), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الأنظمة', NULL, (select id from colleges where ResearchUnitCode = 2), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم نظم المعلومات', NULL, (select id from colleges where ResearchUnitCode = 9), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم علوم الحاسب', NULL, (select id from colleges where ResearchUnitCode = 9), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم تقنية المعلومات', NULL, (select id from colleges where ResearchUnitCode = 9), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم إدارة المعلومات', NULL, (select id from colleges where ResearchUnitCode = 9), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم طب الأسرة', NULL, (select id from colleges where ResearchUnitCode = 13), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الصحة العامة', NULL, (select id from colleges where ResearchUnitCode = 13), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم العيون', NULL, (select id from colleges where ResearchUnitCode = 13), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم التشريح', NULL, (select id from colleges where ResearchUnitCode = 13), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم علوم الامراض', NULL, (select id from colleges where ResearchUnitCode = 13), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الكيمياء الحيوية', NULL, (select id from colleges where ResearchUnitCode = 13), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الأدوية', NULL, (select id from colleges where ResearchUnitCode = 13), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم وظائف الأعضاء', NULL, (select id from colleges where ResearchUnitCode = 13), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم التعليم الطبي', NULL, (select id from colleges where ResearchUnitCode = 13), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الجلدية', NULL, (select id from colleges where ResearchUnitCode = 13), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الكيمياء', NULL, (select id from colleges where ResearchUnitCode = 12), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الفيزياء', NULL, (select id from colleges where ResearchUnitCode = 12), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الرياضيات والاحصاء', NULL, (select id from colleges where ResearchUnitCode = 12), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الأحياء', NULL, (select id from colleges where ResearchUnitCode = 12), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الهندسة الميكانيكية', NULL, (select id from colleges where ResearchUnitCode = 14), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الهندسة المدنية', NULL, (select id from colleges where ResearchUnitCode = 14), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الهندسة الكهربائية', NULL, (select id from colleges where ResearchUnitCode = 14), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الهندسة الكيميائية', NULL, (select id from colleges where ResearchUnitCode = 14), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الهندسة المعمارية', NULL, (select id from colleges where ResearchUnitCode = 14), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الادب', NULL, (select id from colleges where ResearchUnitCode = 4), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم البلاغة والنقد ومنهج الادب الإسلامي', NULL, (select id from colleges where ResearchUnitCode = 4), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم النحو والصرف وفقه اللغة', NULL, (select id from colleges where ResearchUnitCode = 4), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم اللغة العربية والعلوم الإسلامية', NULL, (select id from colleges where ResearchUnitCode = 10), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم علم اللغة التطبيقي', NULL, (select id from colleges where ResearchUnitCode = 10), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الإعداد اللغوي', NULL, (select id from colleges where ResearchUnitCode = 10), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الصحافة والنشر  الإكتروني', NULL, (select id from colleges where ResearchUnitCode = 5), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الإذاعة والتليفزيون والفيلم', NULL, (select id from colleges where ResearchUnitCode = 5), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم العلاقات العامة', NULL, (select id from colleges where ResearchUnitCode = 5), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الإعلان والإتصال التسويقي', NULL, (select id from colleges where ResearchUnitCode = 5), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الجرفيكس والوسائط المتعددة', NULL, (select id from colleges where ResearchUnitCode = 5), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الإعلام المتخصص', NULL, (select id from colleges where ResearchUnitCode = 5), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الجغرافيا', NULL, (select id from colleges where ResearchUnitCode = 6), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الاجتماع والخدمة الاجتماعية', NULL, (select id from colleges where ResearchUnitCode = 6), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم علم النفس', NULL, (select id from colleges where ResearchUnitCode = 6), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الإدارة والتخطيط التربوي', NULL, (select id from colleges where ResearchUnitCode = 6), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم أصول التربية', NULL, (select id from colleges where ResearchUnitCode = 6), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم المناهج وطرق التدريس', NULL, (select id from colleges where ResearchUnitCode = 6), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم التربية الخاصة', NULL, (select id from colleges where ResearchUnitCode = 6), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم التاريخ والحضارة', NULL, (select id from colleges where ResearchUnitCode = 6), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم اللغات والترجمة', NULL, (select id from colleges where ResearchUnitCode = 7), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الاقتصاد', NULL, (select id from colleges where ResearchUnitCode = 11), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم المحاسبة', NULL, (select id from colleges where ResearchUnitCode = 11), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم إدارة الاعمال', NULL, (select id from colleges where ResearchUnitCode = 11), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم التمويل والاستثمار', NULL, (select id from colleges where ResearchUnitCode = 11), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الإعمال المصرفية', NULL, (select id from colleges where ResearchUnitCode = 11), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم التأمين وإدارة المخاطر', NULL, (select id from colleges where ResearchUnitCode = 11), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الشريعة', NULL, (select id from colleges where ResearchUnitCode = 8), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم أصول الدين', NULL, (select id from colleges where ResearchUnitCode = 8), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم اللغة العربية', NULL, (select id from colleges where ResearchUnitCode = 8), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الإدارة', NULL, (select id from colleges where ResearchUnitCode = 8), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الأنظمة', NULL, (select id from colleges where ResearchUnitCode = 8), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم الحاسب الآلي', NULL, (select id from colleges where ResearchUnitCode = 8), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
insert into departments ( NameArabic, NameEnglish, CollegeId, CreatedBy, UpdatedBy, Created, Updated, IsDeleted) values (N'قسم اللغة الإنجليزية', NULL, (select id from colleges where ResearchUnitCode = 8), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', getdate(), getdate(), 0);
go

insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة أم القرى', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'الجامعة الإسلامية', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الملك سعود', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الملك فهد للبترول والمعادن', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الملك فيصل', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الملك خالد', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة القصيم', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة طيبة', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الطائف', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة حائل', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة جازان', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الجوف', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الباحة', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة تبوك', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة نجران', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الحدود الشمالية', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الأميرة نورة بنت عبد الرحمن', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الملك سعود بن عبد العزيز للعلوم الصحية', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الامام عبد الرحمن الفيصل', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الأمير سطام بن عبد العزيز', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة شقراء', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة المجمعة', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'الجامعة السعودية الإلكترونية', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة جدة', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة بيشة', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الأمير سلطان الأهلية', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة عفت الأهلية', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'الجامعة العربية المفتوحة', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة اليمامة الأهلية', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الاعمال والتكنولوجيا الأهلية', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الأمير محمد بن فهد الأهلية', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الفيصل الأهلية', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة دار العلوم الأهلية', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة دار الحكمة الأهلية', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
insert into universities (NameArabic, NameEnglish, Created, Updated, CreatedBy, UpdatedBy, IsDeleted) values (N'جامعة الأمير مقرن بن عبد العزيز', NULL, getdate(), getdate(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 0);
go

-- Date: 04/18/2018
-- Updated data in Banks table
IF EXISTS (SELECT 1 FROM BANKS)
BEGIN
	DELETE  BANKS
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'مجموعة سامبا المالية', N'SAMBA Financial Group', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'بنك ساب', N'SABB Bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'مصرف الراجحي', N'Al Rajhi Bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'البنك الأول', N'First Bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'البنك العربي الوطني', N'Arab national Bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'بنك الجزيرة', N'Al Jazeera Bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'البنك السعودي الفرنسي', N'Saudi French Bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'البنك الأهلي التجاري', N'National Commercial Bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'بنك الرياض', N'Riyad Bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'البنك السعودي للاستثمار', N'The Saudi Investment Bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'شركة البلاد المالية', N'Al Bilad Finance Company', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'بنك مسقط', N'Muscat bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'مصرف الانماء', N'Alinma Bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'بنك الكويت', N'Kuwait Bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'بنك الامارات دبي الوطني    ', N'Emirates NBD Bank', 1)
	 
	INSERT [dbo].[Banks] ([NameArabic], [NameEnglish], [CountryId]) VALUES (N'بنك الخليج الدولي', N'Gulf International Bank', 1)
	 
END

-- Added new columns to Referees Table
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Referees]') 
         AND name = 'Qualification'
)
ALTER TABLE Referees ADD [Qualification] TINYINT NULL;
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Referees]') 
         AND name = 'NationalityId'
)
ALTER TABLE Referees ADD [NationalityId] INT NULL;
GO

-- Added on 6th May:


IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefereeRequestFeedbackFormQuestions]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[RefereeRequestFeedbackFormQuestions](
		[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
		[TextEnglish] [nvarchar](2048) NULL,
		[TextArabic] [nvarchar](2048) NOT NULL,
		[OrderNumber] [int] NOT NULL,
		[MaxMarks] [int] NOT NULL,
		[IslamicFieldEn] [bit] NOT NULL,
		[ArabicLanguageFieldEn] [bit] NOT NULL,
		[SocialFieldEn] [bit] NOT NULL,
		[AppliedScienceFieldEn] [bit] NOT NULL,
		[Created] [datetime] NOT NULL,
		[UpdatedBy] [nvarchar](128) NOT NULL FOREIGN KEY REFERENCES Users (Id),
		[Updated] [datetime] NOT NULL,
		[CreatedBy] [nvarchar](128) NOT NULL FOREIGN KEY REFERENCES Users (Id),
		[IsDeleted] [bit] NOT NULL
	 )

END

GO

IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefereeRequestFeedback]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[RefereeRequestFeedback](
		[Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
		[FeedbackFormQuestionId] [int] NOT NULL FOREIGN KEY REFERENCES RefereeRequestFeedbackFormQuestions (Id),
		[RefereeId] [int] NOT NULL  FOREIGN KEY REFERENCES Referees (Id),
		[RequestId] [bigint] NOT NULL  FOREIGN KEY REFERENCES Requests (Id),
		[Comments] nvarchar(MAX) NULL,
		[GivenMarks] [int] NOT NULL,		
		[Created] [datetime] NOT NULL,
		[UpdatedBy] [nvarchar](128) NOT NULL FOREIGN KEY REFERENCES Users (Id),
		[Updated] [datetime] NOT NULL,
		[CreatedBy] [nvarchar](128) NOT NULL FOREIGN KEY REFERENCES Users (Id),
		[IsDeleted] [bit] NOT NULL
	 )

END

GO


IF NOT EXISTS (SELECT * FROM Settings WHERE [Key] = 'aps.referee.request_feedback.reject_cutoff')
BEGIN
	INSERT INTO Settings VALUES('aps.referee.request_feedback.reject_cutoff', '75', GETDATE(),'00000000-0000-0000-0000-000000000000', GETDATE(), '00000000-0000-0000-0000-000000000000');
END


GO

IF NOT EXISTS (SELECT * FROM Settings WHERE [Key] = 'aps.referee.external.acceptance.days')
BEGIN
	INSERT INTO Settings VALUES('aps.referee.external.acceptance.days', '5', GETDATE(),'00000000-0000-0000-0000-000000000000', GETDATE(), '00000000-0000-0000-0000-000000000000');
END

GO

IF NOT EXISTS (SELECT * FROM Settings WHERE [Key] = 'aps.referee.response.days')
BEGIN
	INSERT INTO Settings VALUES('aps.referee.response.days', '14', GETDATE(),'00000000-0000-0000-0000-000000000000', GETDATE(), '00000000-0000-0000-0000-000000000000');
END

GO

IF NOT EXISTS 
(
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE table_name = 'RequestReferees'
    AND column_name = 'RequestReviewOrderNumber'
)
BEGIN
	ALTER TABLE RequestReferees ADD RequestReviewOrderNumber INT NULL;
END

GO

IF NOT EXISTS 
(
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE table_name = 'RequestReferees'
    AND column_name = 'RequestReviewSentOn'
)
BEGIN
	ALTER TABLE RequestReferees ADD RequestReviewSentOn DATETIME NULL;
END

GO

IF NOT EXISTS 
(
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE table_name = 'RequestReferees'
    AND column_name = 'RequestReviewAcceptOn'
)
BEGIN
	ALTER TABLE RequestReferees ADD RequestReviewAcceptOn DATETIME NULL;
END

GO

IF NOT EXISTS 
(
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE table_name = 'RequestReferees'
    AND column_name = 'IsDeleted'
)
BEGIN
	ALTER TABLE RequestReferees ADD IsDeleted BIT NOT NULL DEFAULT(0);
END

GO  

IF NOT EXISTS 
(
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE table_name = 'Requests'
    AND column_name = 'RequestRevisionId'
)
BEGIN
	ALTER TABLE Requests ADD RequestRevisionId bigint NULL;
END

GO

