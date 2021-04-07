USE master
GO
IF NOT EXISTS (
    SELECT [name]
        FROM sys.databases
        WHERE [name] = N'TestDb'
)
CREATE DATABASE TestDb
GO
USE TestDb
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data](
	[Number] [int] NULL,
	[Text] [nvarchar](255) NULL
) ON [PRIMARY]
GO
INSERT INTO [dbo].[Data] VALUES (1, 'This is from SQL!')
GO
