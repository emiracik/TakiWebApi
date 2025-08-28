using System.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly string _connectionString;
    public InvoiceRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        var list = new List<Invoice>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM Invoices", connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Invoice
            {
                InvoiceID = reader.GetInt32(reader.GetOrdinal("InvoiceID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                TripID = reader.GetInt32(reader.GetOrdinal("TripID")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                InvoiceDate = reader.GetDateTime(reader.GetOrdinal("InvoiceDate")),
                Status = reader.GetString(reader.GetOrdinal("Status"))
            });
        }
        return list;
    }

    public async Task<Invoice?> GetByIdAsync(int invoiceId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM Invoices WHERE InvoiceID = @InvoiceID", connection);
        command.Parameters.AddWithValue("@InvoiceID", invoiceId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Invoice
            {
                InvoiceID = reader.GetInt32(reader.GetOrdinal("InvoiceID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                TripID = reader.GetInt32(reader.GetOrdinal("TripID")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                InvoiceDate = reader.GetDateTime(reader.GetOrdinal("InvoiceDate")),
                Status = reader.GetString(reader.GetOrdinal("Status"))
            };
        }
        return null;
    }

    public async Task<IEnumerable<Invoice>> GetByUserIdAsync(int userId)
    {
        var list = new List<Invoice>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM Invoices WHERE UserID = @UserID", connection);
        command.Parameters.AddWithValue("@UserID", userId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Invoice
            {
                InvoiceID = reader.GetInt32(reader.GetOrdinal("InvoiceID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                TripID = reader.GetInt32(reader.GetOrdinal("TripID")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                InvoiceDate = reader.GetDateTime(reader.GetOrdinal("InvoiceDate")),
                Status = reader.GetString(reader.GetOrdinal("Status"))
            });
        }
        return list;
    }

    public async Task<IEnumerable<Invoice>> GetByTripIdAsync(int tripId)
    {
        var list = new List<Invoice>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM Invoices WHERE TripID = @TripID", connection);
        command.Parameters.AddWithValue("@TripID", tripId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Invoice
            {
                InvoiceID = reader.GetInt32(reader.GetOrdinal("InvoiceID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                TripID = reader.GetInt32(reader.GetOrdinal("TripID")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                InvoiceDate = reader.GetDateTime(reader.GetOrdinal("InvoiceDate")),
                Status = reader.GetString(reader.GetOrdinal("Status"))
            });
        }
        return list;
    }

    public async Task<int> CreateAsync(Invoice invoice)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            @"INSERT INTO Invoices (UserID, TripID, Amount, InvoiceDate, Status)
              VALUES (@UserID, @TripID, @Amount, @InvoiceDate, @Status);
              SELECT SCOPE_IDENTITY();", connection);
        command.Parameters.AddWithValue("@UserID", invoice.UserID);
        command.Parameters.AddWithValue("@TripID", invoice.TripID);
        command.Parameters.AddWithValue("@Amount", invoice.Amount);
        command.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
        command.Parameters.AddWithValue("@Status", invoice.Status);
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Invoice invoice)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            @"UPDATE Invoices SET
                UserID = @UserID,
                TripID = @TripID,
                Amount = @Amount,
                InvoiceDate = @InvoiceDate,
                Status = @Status
              WHERE InvoiceID = @InvoiceID", connection);
        command.Parameters.AddWithValue("@InvoiceID", invoice.InvoiceID);
        command.Parameters.AddWithValue("@UserID", invoice.UserID);
        command.Parameters.AddWithValue("@TripID", invoice.TripID);
        command.Parameters.AddWithValue("@Amount", invoice.Amount);
        command.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
        command.Parameters.AddWithValue("@Status", invoice.Status);
        await connection.OpenAsync();
        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int invoiceId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("DELETE FROM Invoices WHERE InvoiceID = @InvoiceID", connection);
        command.Parameters.AddWithValue("@InvoiceID", invoiceId);
        await connection.OpenAsync();
        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
