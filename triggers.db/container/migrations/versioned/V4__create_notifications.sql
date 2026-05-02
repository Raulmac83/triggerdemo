IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications
    (
        Id          BIGINT          IDENTITY(1,1) NOT NULL,
        Type        NVARCHAR(50)    NOT NULL,
        Severity    NVARCHAR(20)    NOT NULL CONSTRAINT DF_Notifications_Severity DEFAULT (N'Info'),
        EntityType  NVARCHAR(100)   NULL,
        EntityId    INT             NULL,
        Title       NVARCHAR(200)   NOT NULL,
        Message     NVARCHAR(2000)  NULL,
        Payload     NVARCHAR(MAX)   NULL,
        UserId      INT             NULL,
        IsRead      BIT             NOT NULL CONSTRAINT DF_Notifications_IsRead    DEFAULT (0),
        ReadAt      DATETIME2(7)    NULL,
        CreatedAt   DATETIME2(7)    NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Notifications PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE SET NULL,
        CONSTRAINT CK_Notifications_Severity CHECK (Severity IN (N'Info', N'Success', N'Warning', N'Error')),
        CONSTRAINT CK_Notifications_ReadAt   CHECK (
            (IsRead = 0 AND ReadAt IS NULL) OR
            (IsRead = 1 AND ReadAt IS NOT NULL)
        )
    );

    CREATE INDEX IX_Notifications_CreatedAt        ON dbo.Notifications (CreatedAt DESC);
    CREATE INDEX IX_Notifications_UserId_IsRead    ON dbo.Notifications (UserId, IsRead) INCLUDE (CreatedAt);
    CREATE INDEX IX_Notifications_Entity           ON dbo.Notifications (EntityType, EntityId);
END;
GO
