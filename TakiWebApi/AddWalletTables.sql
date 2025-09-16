-- ===============================================
-- WALLETS TABLE
-- ===============================================
CREATE TABLE Wallets (
    WalletID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NOT NULL UNIQUE,
    Balance decimal(18,2) NOT NULL DEFAULT 0,
    TotalIn decimal(18,2) NOT NULL DEFAULT 0,
    TotalOut decimal(18,2) NOT NULL DEFAULT 0,
    LastUpdated datetime2 NOT NULL DEFAULT GETDATE(),
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- ===============================================
-- WALLET TRANSACTIONS TABLE
-- ===============================================
CREATE TABLE WalletTransactions (
    TransactionID int IDENTITY(1,1) PRIMARY KEY,
    WalletID int NOT NULL,
    UserID int NOT NULL,
    TransactionType nvarchar(50) NOT NULL, -- Deposit, Withdraw, Refund, Payment
    Amount decimal(18,2) NOT NULL,
    Description nvarchar(500) NULL,
    TransactionDate datetime2 NOT NULL DEFAULT GETDATE(),
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (WalletID) REFERENCES Wallets(WalletID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Sample Data
-- INSERT INTO Wallets (UserID, Balance, TotalIn, TotalOut) VALUES (1, 100, 200, 100);
-- INSERT INTO WalletTransactions (WalletID, UserID, TransactionType, Amount, Description) VALUES (1, 1, 'Deposit', 200, N'İlk yükleme');
-- INSERT INTO WalletTransactions (WalletID, UserID, TransactionType, Amount, Description) VALUES (1, 1, 'Payment', -100, N'Yolculuk ücreti');
