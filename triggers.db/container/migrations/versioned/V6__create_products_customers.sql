IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id          INT             IDENTITY(1,1) NOT NULL,
        Name        NVARCHAR(200)   NOT NULL,
        Description NVARCHAR(1000)  NULL,
        Sku         NVARCHAR(50)    NULL,
        Price       DECIMAL(18, 2)  NOT NULL CONSTRAINT DF_Products_Price    DEFAULT (0),
        IsActive    BIT             NOT NULL CONSTRAINT DF_Products_IsActive DEFAULT (1),
        CreatedAt   DATETIME2(7)    NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (Id)
    );

    CREATE INDEX IX_Products_Name ON dbo.Products (Name);
END;
GO

IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers
    (
        Id          INT             IDENTITY(1,1) NOT NULL,
        Name        NVARCHAR(200)   NOT NULL,
        Email       NVARCHAR(256)   NULL,
        Phone       NVARCHAR(50)    NULL,
        IsActive    BIT             NOT NULL CONSTRAINT DF_Customers_IsActive  DEFAULT (1),
        CreatedAt   DATETIME2(7)    NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Customers PRIMARY KEY CLUSTERED (Id)
    );

    CREATE INDEX IX_Customers_Name ON dbo.Customers (Name);
END;
GO
