CREATE DATABASE TeamDB;
GO

USE TeamDB;
GO

CREATE TABLE Teams (
                       Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                       Name NVARCHAR(100) NOT NULL,
                       CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                       UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE TABLE TeamMembers (
                             TeamId UNIQUEIDENTIFIER NOT NULL,
                             UserId UNIQUEIDENTIFIER NOT NULL,
                             CONSTRAINT PK_TeamMembers PRIMARY KEY (TeamId, UserId),
                             CONSTRAINT FK_TeamMembers_Teams FOREIGN KEY (TeamId) REFERENCES Teams(Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_TeamMembers_UserId ON TeamMembers(UserId);
GO

CREATE PROCEDURE sp_GetTeamMembers
    @TeamId UNIQUEIDENTIFIER
AS
BEGIN
SELECT TeamId, UserId
FROM TeamMembers
WHERE TeamId = @TeamId;
END;
GO