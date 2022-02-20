CREATE TABLE [dbo].[User]
(
	[UserId] NVARCHAR(128) NOT NULL PRIMARY KEY,
    [UserFirstName] NVARCHAR(50) NOT NULL, 
    [UserLastName] NVARCHAR(50) NOT NULL,
    [UserEmail] NVARCHAR(256) NOT NULL, 
    [CreateDate] DATETIME2 NOT NULL DEFAULT getutcdate()
)
