-- Seed: admin / Password123!
-- Hash is BCrypt (cost 11). Verify with BCrypt.Net-Next on the .NET side.

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'Admin')
    INSERT INTO dbo.Roles (Name, Description) VALUES (N'Admin', N'Full administrative access');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'User')
    INSERT INTO dbo.Roles (Name, Description) VALUES (N'User', N'Standard user');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'admin')
BEGIN
    INSERT INTO dbo.Users (Username, Email, PasswordHash, IsActive)
    VALUES (
        N'admin',
        N'admin@triggers.local',
        N'$2b$11$kZwo/MtRRstHu57helZB.uKMZyWR.3v5xbej.kmsswGU2xP8k6SJ2',
        1
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM dbo.UserRoles ur
    INNER JOIN dbo.Users u ON u.Id = ur.UserId
    INNER JOIN dbo.Roles r ON r.Id = ur.RoleId
    WHERE u.Username = N'admin' AND r.Name = N'Admin'
)
BEGIN
    INSERT INTO dbo.UserRoles (UserId, RoleId)
    SELECT u.Id, r.Id
    FROM dbo.Users u
    CROSS JOIN dbo.Roles r
    WHERE u.Username = N'admin' AND r.Name = N'Admin';
END;
GO
