-- Reset admin password to: x
-- BCrypt cost 11. Generated with BCrypt.Net-Next.
UPDATE dbo.Users
SET PasswordHash = N'$2a$11$Ji9LwYBHnp9fDNceyy.S/eiPtwZ1noQBJ85b91oXC8yDufxzFt.x6'
WHERE Username = N'admin';
GO
