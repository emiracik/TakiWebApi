-- Kredi Kartları Tablosu
CREATE TABLE CreditCards (
    CardID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NOT NULL,
    CardNumber nvarchar(20) NOT NULL,
    CardHolder nvarchar(100) NOT NULL,
    ExpiryMonth int NOT NULL,
    ExpiryYear int NOT NULL,
    CVC nvarchar(4) NOT NULL,
    IsDefault bit NOT NULL DEFAULT 0,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Cüzdan Hareketleri Tablosu
CREATE TABLE WalletTransactions (
    TransactionID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NOT NULL,
    Amount decimal(10,2) NOT NULL,
    TransactionType nvarchar(20) NOT NULL, -- Deposit, Withdraw, Payment
    Description nvarchar(200) NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Faturalar Tablosu
CREATE TABLE Invoices (
    InvoiceID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NOT NULL,
    TripID int NOT NULL,
    Amount decimal(10,2) NOT NULL,
    InvoiceDate datetime2 NOT NULL DEFAULT GETDATE(),
    Status nvarchar(20) NOT NULL DEFAULT 'Paid',
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (TripID) REFERENCES Trips(TripID)
);

-- Geri Bildirimler Tablosu
CREATE TABLE Feedbacks (
    FeedbackID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NULL,
    DriverID int NULL,
    TripID int NULL,
    Rating decimal(2,1) NOT NULL,
    Comment nvarchar(1000) NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
    FOREIGN KEY (TripID) REFERENCES Trips(TripID)
);

-- Paylaşımlı Yolculuk Tablosu
CREATE TABLE SharedRides (
    SharedRideID int IDENTITY(1,1) PRIMARY KEY,
    TripID int NOT NULL,
    PassengerCount int NOT NULL,
    Status nvarchar(20) NOT NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (TripID) REFERENCES Trips(TripID)
);

-- Mesajlaşma Tablosu
CREATE TABLE Messages (
    MessageID int IDENTITY(1,1) PRIMARY KEY,
    SenderID int NOT NULL,
    ReceiverID int NOT NULL,
    Content nvarchar(2000) NOT NULL,
    SentAt datetime2 NOT NULL DEFAULT GETDATE(),
    IsRead bit NOT NULL DEFAULT 0,
    FOREIGN KEY (SenderID) REFERENCES Users(UserID),
    FOREIGN KEY (ReceiverID) REFERENCES Users(UserID)
);

-- Sürücü Belgeleri Tablosu
CREATE TABLE DriverDocuments (
    DocumentID int IDENTITY(1,1) PRIMARY KEY,
    DriverID int NOT NULL,
    DocumentType nvarchar(50) NOT NULL,
    DocumentUrl nvarchar(500) NOT NULL,
    UploadedDate datetime2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
);

-- Sürücü Banka Bilgileri Tablosu
CREATE TABLE BankInfo (
    BankInfoID int IDENTITY(1,1) PRIMARY KEY,
    DriverID int NOT NULL,
    BankName nvarchar(100) NOT NULL,
    IBAN nvarchar(34) NOT NULL,
    AccountNumber nvarchar(30) NOT NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
);