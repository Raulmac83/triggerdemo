IF OBJECT_ID(N'dbo.Triggers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Triggers
    (
        Id          INT             IDENTITY(1,1) NOT NULL,
        Name        NVARCHAR(200)   NOT NULL,
        Description NVARCHAR(1000)  NULL,
        IsEnabled   BIT             NOT NULL CONSTRAINT DF_Triggers_IsEnabled DEFAULT (0),
        CreatedAt   DATETIME2(7)    NOT NULL CONSTRAINT DF_Triggers_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Triggers PRIMARY KEY CLUSTERED (Id)
    );

    CREATE INDEX IX_Triggers_Name ON dbo.Triggers (Name);
END;
