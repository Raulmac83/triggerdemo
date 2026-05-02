IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Notifications')
      AND name = N'TriggerMethod'
)
BEGIN
    ALTER TABLE dbo.Notifications
        ADD TriggerMethod NVARCHAR(50) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Notifications_TriggerMethod')
BEGIN
    CREATE INDEX IX_Notifications_TriggerMethod ON dbo.Notifications (TriggerMethod);
END;
GO
