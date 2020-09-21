-- Create a new table called '[Data]' in schema '[dbo]'
-- Drop the table if it already exists
IF OBJECT_ID('[dbo].[Data]', 'U') IS NOT NULL
DROP TABLE [dbo].[Data]
GO
-- Create the table in the specified schema
CREATE TABLE [dbo].[Data]
(
    [Number] INT NOT NULL PRIMARY KEY, -- Primary Key column
    [Text] NVARCHAR(255) NULL
);
GO