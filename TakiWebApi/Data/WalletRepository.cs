using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data
{
    public class WalletRepository : IWalletRepository
    {
        private readonly string _connectionString;
        public WalletRepository(Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SqlServerConnection")
                ?? throw new ArgumentNullException(nameof(config), "Connection string cannot be null");
        }


        public async Task<Wallet?> GetWalletByUserIdAsync(int userId)
        {
            const string sql = "SELECT * FROM Wallets WHERE UserID = @UserID AND IsDeleted = 0";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UserID", userId);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapWallet(reader);
            }
            return null;
        }


        public async Task<List<WalletTransaction>> GetLastTransactionsAsync(int userId, int count = 10)
        {
            var list = new List<WalletTransaction>();
            const string sql = "SELECT TOP (@Count) * FROM WalletTransactions WHERE UserID = @UserID ORDER BY TransactionDate DESC";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UserID", userId);
            command.Parameters.AddWithValue("@Count", count);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapTransaction(reader));
            }
            return list;
        }


        public async Task AddTransactionAsync(WalletTransaction transaction)
        {
            const string sql = "INSERT INTO WalletTransactions (WalletID, UserID, TransactionType, Amount, Description, TransactionDate) VALUES (@WalletID, @UserID, @TransactionType, @Amount, @Description, @TransactionDate)";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@WalletID", transaction.WalletID);
            command.Parameters.AddWithValue("@UserID", transaction.UserID);
            command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
            command.Parameters.AddWithValue("@Amount", transaction.Amount);
            command.Parameters.AddWithValue("@Description", (object?)transaction.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }


        public async Task UpdateWalletBalanceAsync(int walletId, decimal newBalance, decimal totalIn, decimal totalOut)
        {
            const string sql = "UPDATE Wallets SET Balance = @Balance, TotalIn = @TotalIn, TotalOut = @TotalOut, LastUpdated = GETDATE() WHERE WalletID = @WalletID";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@WalletID", walletId);
            command.Parameters.AddWithValue("@Balance", newBalance);
            command.Parameters.AddWithValue("@TotalIn", totalIn);
            command.Parameters.AddWithValue("@TotalOut", totalOut);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }


        public async Task<Wallet> CreateWalletAsync(int userId)
        {
            const string sql = "INSERT INTO Wallets (UserID, Balance, TotalIn, TotalOut) OUTPUT INSERTED.WalletID VALUES (@UserID, 0, 0, 0)";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UserID", userId);
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
                throw new Exception("WalletID could not be retrieved after insert.");
            int walletId = Convert.ToInt32(result);
            return new Wallet { WalletID = walletId, UserID = userId, Balance = 0, TotalIn = 0, TotalOut = 0, LastUpdated = DateTime.Now };
        }

        private Wallet MapWallet(SqlDataReader reader)
        {
            return new Wallet
            {
                WalletID = (int)reader["WalletID"],
                UserID = (int)reader["UserID"],
                Balance = (decimal)reader["Balance"],
                TotalIn = (decimal)reader["TotalIn"],
                TotalOut = (decimal)reader["TotalOut"],
                LastUpdated = (DateTime)reader["LastUpdated"]
            };
        }

        private WalletTransaction MapTransaction(SqlDataReader reader)
        {
            return new WalletTransaction
            {
                TransactionID = (int)reader["TransactionID"],
                WalletID = (int)reader["WalletID"],
                UserID = (int)reader["UserID"],
                TransactionType = reader["TransactionType"].ToString()!,
                Amount = (decimal)reader["Amount"],
                Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                TransactionDate = (DateTime)reader["TransactionDate"]
            };
        }
    }
}
