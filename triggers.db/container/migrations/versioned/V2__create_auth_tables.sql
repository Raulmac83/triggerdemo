IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id              INT             IDENTITY(1,1) NOT NULL,
        Username        NVARCHAR(100)   NOT NULL,
        Email           NVARCHAR(256)   NOT NULL,
        PasswordHash    NVARCHAR(256)   NOT NULL,
        IsActive        BIT             NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreatedAt       DATETIME2(7)    NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2(7)    NULL,
        LastLoginAt     DATETIME2(7)    NULL,
        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id)
    );

    CREATE UNIQUE INDEX UX_Users_Username ON dbo.Users (Username);
    CREATE UNIQUE INDEX UX_Users_Email    ON dbo.Users (Email);
END;
GO

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        Id          INT             IDENTITY(1,1) NOT NULL,
        Name        NVARCHAR(100)   NOT NULL,
        Description NVARCHAR(500)   NULL,
        CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (Id)
    );

    CREATE UNIQUE INDEX UX_Roles_Name ON dbo.Roles (Name);
END;
GO

IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles
    (
        UserId    INT NOT NULL,
        RoleId    INT NOT NULL,
        AssignedAt DATETIME2(7) NOT NULL CONSTRAINT DF_UserRoles_AssignedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_UserRoles PRIMARY KEY CLUSTERED (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshTokens
    (
        Id          INT             IDENTITY(1,1) NOT NULL,
        UserId      INT             NOT NULL,
        TokenHash   NVARCHAR(256)   NOT NULL,
        ExpiresAt   DATETIME2(7)    NOT NULL,
        CreatedAt   DATETIME2(7)    NOT NULL CONSTRAINT DF_RefreshTokens_CreatedAt DEFAULT (SYSUTCDATETIME()),
        RevokedAt   DATETIME2(7)    NULL,
        CONSTRAINT PK_RefreshTokens PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_RefreshTokens_UserId    ON dbo.RefreshTokens (UserId);
    CREATE INDEX IX_RefreshTokens_TokenHash ON dbo.RefreshTokens (TokenHash);
END;
GO
