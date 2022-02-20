CREATE PROCEDURE [dbo].[spUserLookup]
	@Id nvarchar(128)
AS
begin
	set nocount on;

	SELECT UserId, UserFirstName, UserLastName, UserEmail, CreateDate
	from [dbo].[User]
	where UserId = @Id
end
