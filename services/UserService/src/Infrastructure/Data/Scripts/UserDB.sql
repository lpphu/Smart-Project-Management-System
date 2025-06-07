CREATE DATABASE ProjectDB;
GO

USE ProjectDB;
GO

CREATE TABLE Projects (
                          Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                          Name NVARCHAR(100) NOT NULL,
                          Description NVARCHAR(500),
                          ProjectManagerId UNIQUEIDENTIFIER NOT NULL,
                          CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                          UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE INDEX IX_Projects_ProjectManagerId ON Projects(ProjectManagerId);
GO

CREATE PROCEDURE sp_GetProjectsByManager
    @ProjectManagerId UNIQUEIDENTIFIER
AS
BEGIN
SELECT Id, Name, Description, ProjectManagerId, CreatedAt, UpdatedAt
FROM Projects
WHERE ProjectManagerId = @ProjectManagerId;
END;
GO