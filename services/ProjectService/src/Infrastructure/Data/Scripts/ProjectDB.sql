CREATE DATABASE ProjectDB;
GO

USE ProjectDB;
GO

CREATE TABLE Projects (
                          Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                          Name NVARCHAR(100) NOT NULL,
                          Description NVARCHAR(500),
                          ProjectManagerId UNIQUEIDENTIFIER NOT NULL,
                          Status NVARCHAR(50) NOT NULL CHECK (Status IN ('Planning', 'InProgress', 'Completed')),
                          CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                          UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO
CREATE TABLE ProjectTeams (
                              ProjectId UNIQUEIDENTIFIER NOT NULL,
                              TeamId UNIQUEIDENTIFIER NOT NULL,
                              CONSTRAINT PK_ProjectTeams PRIMARY KEY (ProjectId, TeamId),
                              CONSTRAINT FK_ProjectTeams_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
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