using System.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class BankInfoRepository : IBankInfoRepository
{
    private readonly string _connectionString;
    public BankInfoRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<IEnumerable<BankInfo>> GetAllAsync()
    {
        var list = new List<BankInfo>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM BankInfo", connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BankInfo
            {
                BankInfoID = reader.GetInt32(reader.GetOrdinal("BankInfoID")),
                DriverID = reader.GetInt32(reader.GetOrdinal("DriverID")),
                BankName = reader.GetString(reader.GetOrdinal("BankName")),
                IBAN = reader.GetString(reader.GetOrdinal("IBAN")),
                AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            });
        }
        return list;
    }

    public async Task<BankInfo?> GetByIdAsync(int bankInfoId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM BankInfo WHERE BankInfoID = @BankInfoID", connection);
        command.Parameters.AddWithValue("@BankInfoID", bankInfoId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new BankInfo
            {
                BankInfoID = reader.GetInt32(reader.GetOrdinal("BankInfoID")),
                DriverID = reader.GetInt32(reader.GetOrdinal("DriverID")),
                BankName = reader.GetString(reader.GetOrdinal("BankName")),
                IBAN = reader.GetString(reader.GetOrdinal("IBAN")),
                AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            };
        }
        return null;
    }

    public async Task<IEnumerable<BankInfo>> GetByDriverIdAsync(int driverId)
    {
        var list = new List<BankInfo>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM BankInfo WHERE DriverID = @DriverID", connection);
        command.Parameters.AddWithValue("@DriverID", driverId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BankInfo
            {
                BankInfoID = reader.GetInt32(reader.GetOrdinal("BankInfoID")),
                DriverID = reader.GetInt32(reader.GetOrdinal("DriverID")),
                BankName = reader.GetString(reader.GetOrdinal("BankName")),
                IBAN = reader.GetString(reader.GetOrdinal("IBAN")),
                AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            });
        }
        return list;
    }

    public async Task<int> CreateAsync(BankInfo bankInfo)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            @"INSERT INTO BankInfo (DriverID, BankName, IBAN, AccountNumber, CreatedDate)
              VALUES (@DriverID, @BankName, @IBAN, @AccountNumber, @CreatedDate);
              SELECT SCOPE_IDENTITY();", connection);
        command.Parameters.AddWithValue("@DriverID", bankInfo.DriverID);
        command.Parameters.AddWithValue("@BankName", bankInfo.BankName);
        command.Parameters.AddWithValue("@IBAN", bankInfo.IBAN);
        command.Parameters.AddWithValue("@AccountNumber", bankInfo.AccountNumber);
        command.Parameters.AddWithValue("@CreatedDate", bankInfo.CreatedDate);
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(BankInfo bankInfo)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            @"UPDATE BankInfo SET
                DriverID = @DriverID,
                BankName = @BankName,
                IBAN = @IBAN,
                AccountNumber = @AccountNumber
              WHERE BankInfoID = @BankInfoID", connection);
        command.Parameters.AddWithValue("@BankInfoID", bankInfo.BankInfoID);
        command.Parameters.AddWithValue("@DriverID", bankInfo.DriverID);
        command.Parameters.AddWithValue("@BankName", bankInfo.BankName);
        command.Parameters.AddWithValue("@IBAN", bankInfo.IBAN);
        command.Parameters.AddWithValue("@AccountNumber", bankInfo.AccountNumber);
        await connection.OpenAsync();
        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int bankInfoId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("DELETE FROM BankInfo WHERE BankInfoID = @BankInfoID", connection);
        command.Parameters.AddWithValue("@BankInfoID", bankInfoId);
        await connection.OpenAsync();
        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
