
-- Create database (if needed)
--CREATE DATABASE AuthDemoDB;
--GO

--USE AuthDemoDB;
--GO

-- Users table to store user credentials
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARBINARY(MAX) NOT NULL,
    PasswordSalt VARBINARY(MAX) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    LastLogin DATETIME2 NULL,
    IsActive BIT DEFAULT 1
);

-- Refresh tokens table for token rotation
CREATE TABLE RefreshTokens (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Token NVARCHAR(MAX) NOT NULL,
    Expires DATETIME2 NOT NULL,
    Created DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedByIp NVARCHAR(50) NULL,
    Revoked DATETIME2 NULL,
    RevokedByIp NVARCHAR(50) NULL,
    ReplacedByToken NVARCHAR(MAX) NULL,
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

Select * from Users
Select * from RefreshTokens
