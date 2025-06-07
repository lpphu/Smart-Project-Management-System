CREATE DATABASE TaskDB;
GO

USE TaskDB;
GO

CREATE TABLE Tasks (
                       Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                       ProjectId UNIQUEIDENTIFIER NOT NULL,
                       Title NVARCHAR(100) NOT NULL,
                       Description NVARCHAR(500),
                       AssigneeId UNIQUEIDENTIFIER,
                       Status NVARCHAR(20) NOT NULL CHECK (Status IN ('ToDo', 'InProgress', 'Done')),
                       CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                       UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO
CREATE TABLE TaskHistories (
                               Id UNIQUEIDENTIFIER PRIMARY KEY,
                               TaskId UNIQUEIDENTIFIER NOT NULL,
                               ModifiedBy UNIQUEIDENTIFIER NOT NULL,
                               ChangeDescription NVARCHAR(MAX) NOT NULL,
                               ModifiedAt DATETIME2 NOT NULL,
                               FOREIGN KEY (TaskId) REFERENCES Tasks(Id)
);
GO
CREATE INDEX IX_Tasks_ProjectId ON Tasks(ProjectId);
CREATE INDEX IX_Tasks_AssigneeId ON Tasks(AssigneeId);
CREATE INDEX IX_Tasks_Status ON Tasks(Status);
GO

CREATE PROCEDURE sp_GetTasksByProject
    @ProjectId UNIQUEIDENTIFIER
AS
BEGIN
SELECT Id, ProjectId, Title, Description, AssigneeId, Status, CreatedAt, UpdatedAt
FROM Tasks
WHERE ProjectId = @ProjectId;
END;
GO