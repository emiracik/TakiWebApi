using System.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class WalletTransactionRepository : IWalletTransactionRepository
{
    private readonly string _connectionString;
    public WalletTransactionRepository(IConfiguration config)
    {
       _connectionString = config.GetConnectionString("SqlServerConnection")
                ?? throw new ArgumentNullException(nameof(config), "Connection string cannot be null");
    }

    public async Task<IEnumerable<WalletTransaction>> GetAllAsync()
    {
        var list = new List<WalletTransaction>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM WalletTransactions", connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new WalletTransaction
            {
                TransactionID = reader.GetInt32(reader.GetOrdinal("TransactionID")),
                WalletID = reader.GetInt32(reader.GetOrdinal("WalletID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                TransactionType = reader.GetString(reader.GetOrdinal("TransactionType")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate"))
            });
        }
        return list;
    }

    public async Task<WalletTransaction?> GetByIdAsync(int transactionId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM WalletTransactions WHERE TransactionID = @TransactionID", connection);
        command.Parameters.AddWithValue("@TransactionID", transactionId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new WalletTransaction
            {
                TransactionID = reader.GetInt32(reader.GetOrdinal("TransactionID")),
                WalletID = reader.GetInt32(reader.GetOrdinal("WalletID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                TransactionType = reader.GetString(reader.GetOrdinal("TransactionType")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate"))
            };
        }
        return null;
    }

    public async Task<IEnumerable<WalletTransaction>> GetByUserIdAsync(int userId)
    {
        var list = new List<WalletTransaction>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM WalletTransactions WHERE USERID = @UserID", connection);
        command.Parameters.AddWithValue("@UserID", userId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new WalletTransaction
            {
                TransactionID = reader.GetInt32(reader.GetOrdinal("TransactionID")),
                WalletID = reader.GetInt32(reader.GetOrdinal("WalletID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                TransactionType = reader.GetString(reader.GetOrdinal("TransactionType")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate"))
            });
        }
        return list;
    }

    public async Task<int> CreateAsync(WalletTransaction transaction)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            @"INSERT INTO WalletTransactions (WalletID, UserID, Amount, TransactionType, Description, TransactionDate)
              VALUES (@WalletID, @UserID, @Amount, @TransactionType, @Description, @TransactionDate);
              SELECT SCOPE_IDENTITY();", connection);
        command.Parameters.AddWithValue("@WalletID", transaction.WalletID);
        command.Parameters.AddWithValue("@UserID", transaction.UserID);
        command.Parameters.AddWithValue("@Amount", transaction.Amount);
        command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
        command.Parameters.AddWithValue("@Description", (object?)transaction.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
