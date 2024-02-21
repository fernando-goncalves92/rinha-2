IF (SELECT COUNT(*) FROM SYS.DATABASES WHERE NAME = 'Rinha') > 0 
BEGIN
    DROP DATABASE Rinha    
END
GO
CREATE DATABASE Rinha	
GO 
USE Rinha
GO	    
CREATE TABLE Balance
(
	Id         INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	CustomerId INT NOT NULL,
	Amount     INT,
	UpdatedAt  DATETIME
)
GO
CREATE TABLE [Transaction]
(
	Id              INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	CustomerId      INT NOT NULL,
	Amount          INT NOT NULL,
	TransactionType CHAR(1) NOT NULL,
	[Description]   VARCHAR(10) NOT NULL,
	CreatedAt       DATETIME NOT NULL
)
GO
DECLARE @GETDATE DATETIME = GETDATE()
INSERT INTO Balance (CustomerId, Amount, UpdatedAt) VALUES (1, 0, @GETDATE)
INSERT INTO Balance (CustomerId, Amount, UpdatedAt) VALUES (2, 0, @GETDATE)
INSERT INTO Balance (CustomerId, Amount, UpdatedAt) VALUES (3, 0, @GETDATE)
INSERT INTO Balance (CustomerId, Amount, UpdatedAt) VALUES (4, 0, @GETDATE)
INSERT INTO Balance (CustomerId, Amount, UpdatedAt) VALUES (5, 0, @GETDATE)