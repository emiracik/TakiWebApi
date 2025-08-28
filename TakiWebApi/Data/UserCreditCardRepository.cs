using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class UserCreditCardRepository : IUserCreditCardRepository
{
    private readonly string _connectionString;

    public UserCreditCardRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<UserCreditCard>> GetAllUserCreditCardsAsync()
    {
        var userCreditCards = new List<UserCreditCard>();
        const string sql = @"
            SELECT CardID, UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, 
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserCreditCards
            WHERE IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userCreditCards.Add(new UserCreditCard
            {
                CardID = reader.GetInt32("CardID"),
                UserID = reader.GetInt32("UserID"),
                CardHolderName = reader.GetString("CardHolderName"),
                CardNumberMasked = reader.GetString("CardNumberMasked"),
                ExpiryMonth = reader.GetInt32("ExpiryMonth"),
                ExpiryYear = reader.GetInt32("ExpiryYear"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                IsDeleted = reader.GetBoolean("IsDeleted")
            });
        }

        return userCreditCards;
    }

    public async Task<UserCreditCard?> GetUserCreditCardByIdAsync(int cardId)
    {
        const string sql = @"
            SELECT CardID, UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, 
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserCreditCards
            WHERE CardID = @CardID AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@CardID", SqlDbType.Int).Value = cardId;

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new UserCreditCard
            {
                CardID = reader.GetInt32("CardID"),
                UserID = reader.GetInt32("UserID"),
                CardHolderName = reader.GetString("CardHolderName"),
                CardNumberMasked = reader.GetString("CardNumberMasked"),
                ExpiryMonth = reader.GetInt32("ExpiryMonth"),
                ExpiryYear = reader.GetInt32("ExpiryYear"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                IsDeleted = reader.GetBoolean("IsDeleted")
            };
        }

        return null;
    }

    public async Task<IEnumerable<UserCreditCard>> GetUserCreditCardsByUserIdAsync(int userId)
    {
        var userCreditCards = new List<UserCreditCard>();
        const string sql = @"
            SELECT CardID, UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, 
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserCreditCards
            WHERE UserID = @UserID AND IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userCreditCards.Add(new UserCreditCard
            {
                CardID = reader.GetInt32("CardID"),
                UserID = reader.GetInt32("UserID"),
                CardHolderName = reader.GetString("CardHolderName"),
                CardNumberMasked = reader.GetString("CardNumberMasked"),
                ExpiryMonth = reader.GetInt32("ExpiryMonth"),
                ExpiryYear = reader.GetInt32("ExpiryYear"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                IsDeleted = reader.GetBoolean("IsDeleted")
            });
        }

        return userCreditCards;
    }

    public async Task<IEnumerable<UserCreditCard>> GetActiveUserCreditCardsAsync()
    {
        var userCreditCards = new List<UserCreditCard>();
        const string sql = @"
            SELECT CardID, UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, 
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserCreditCards
            WHERE IsDeleted = 0 AND ExpiryYear >= @CurrentYear 
            AND (ExpiryYear > @CurrentYear OR ExpiryMonth >= @CurrentMonth)
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@CurrentYear", SqlDbType.Int).Value = DateTime.Now.Year;
        command.Parameters.Add("@CurrentMonth", SqlDbType.Int).Value = DateTime.Now.Month;

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userCreditCards.Add(new UserCreditCard
            {
                CardID = reader.GetInt32("CardID"),
                UserID = reader.GetInt32("UserID"),
                CardHolderName = reader.GetString("CardHolderName"),
                CardNumberMasked = reader.GetString("CardNumberMasked"),
                ExpiryMonth = reader.GetInt32("ExpiryMonth"),
                ExpiryYear = reader.GetInt32("ExpiryYear"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                IsDeleted = reader.GetBoolean("IsDeleted")
            });
        }

        return userCreditCards;
    }

    public async Task<IEnumerable<UserCreditCard>> GetUserCreditCardsPaginatedAsync(int pageNumber, int pageSize)
    {
        var userCreditCards = new List<UserCreditCard>();
        var offset = (pageNumber - 1) * pageSize;

        const string sql = @"
            SELECT CardID, UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, 
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserCreditCards
            WHERE IsDeleted = 0
            ORDER BY CreatedDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Offset", SqlDbType.Int).Value = offset;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userCreditCards.Add(new UserCreditCard
            {
                CardID = reader.GetInt32("CardID"),
                UserID = reader.GetInt32("UserID"),
                CardHolderName = reader.GetString("CardHolderName"),
                CardNumberMasked = reader.GetString("CardNumberMasked"),
                ExpiryMonth = reader.GetInt32("ExpiryMonth"),
                ExpiryYear = reader.GetInt32("ExpiryYear"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                IsDeleted = reader.GetBoolean("IsDeleted")
            });
        }

        return userCreditCards;
    }

    public async Task<int> GetTotalUserCreditCardsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM UserCreditCards WHERE IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result != null ? (int)result : 0;
    }

    public async Task<int> GetActiveUserCreditCardsCountAsync()
    {
        const string sql = @"
            SELECT COUNT(*) FROM UserCreditCards 
            WHERE IsDeleted = 0 AND ExpiryYear >= @CurrentYear 
            AND (ExpiryYear > @CurrentYear OR ExpiryMonth >= @CurrentMonth)";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@CurrentYear", SqlDbType.Int).Value = DateTime.Now.Year;
        command.Parameters.Add("@CurrentMonth", SqlDbType.Int).Value = DateTime.Now.Month;

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result != null ? (int)result : 0;
    }

    public async Task<IEnumerable<UserCreditCard>> SearchUserCreditCardsByNameAsync(string searchTerm)
    {
        var userCreditCards = new List<UserCreditCard>();
        const string sql = @"
            SELECT CardID, UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, 
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserCreditCards
            WHERE IsDeleted = 0 AND CardHolderName LIKE @SearchTerm
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = $"%{searchTerm}%";

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userCreditCards.Add(new UserCreditCard
            {
                CardID = reader.GetInt32("CardID"),
                UserID = reader.GetInt32("UserID"),
                CardHolderName = reader.GetString("CardHolderName"),
                CardNumberMasked = reader.GetString("CardNumberMasked"),
                ExpiryMonth = reader.GetInt32("ExpiryMonth"),
                ExpiryYear = reader.GetInt32("ExpiryYear"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                IsDeleted = reader.GetBoolean("IsDeleted")
            });
        }

        return userCreditCards;
    }

    public async Task<IEnumerable<UserCreditCard>> GetUserCreditCardsByCreatedDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var userCreditCards = new List<UserCreditCard>();
        const string sql = @"
            SELECT CardID, UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, 
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserCreditCards
            WHERE IsDeleted = 0 AND CreatedDate >= @StartDate AND CreatedDate <= @EndDate
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = startDate;
        command.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = endDate;

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userCreditCards.Add(new UserCreditCard
            {
                CardID = reader.GetInt32("CardID"),
                UserID = reader.GetInt32("UserID"),
                CardHolderName = reader.GetString("CardHolderName"),
                CardNumberMasked = reader.GetString("CardNumberMasked"),
                ExpiryMonth = reader.GetInt32("ExpiryMonth"),
                ExpiryYear = reader.GetInt32("ExpiryYear"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                IsDeleted = reader.GetBoolean("IsDeleted")
            });
        }

        return userCreditCards;
    }

    public async Task<int> CreateUserCreditCardAsync(UserCreditCard userCreditCard)
    {
        const string sql = @"
            INSERT INTO UserCreditCards (UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, CreatedBy, CreatedDate)
            OUTPUT INSERTED.CardID
            VALUES (@UserID, @CardHolderName, @CardNumberMasked, @ExpiryMonth, @ExpiryYear, @CreatedBy, @CreatedDate)";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@UserID", SqlDbType.Int).Value = userCreditCard.UserID;
        command.Parameters.Add("@CardHolderName", SqlDbType.NVarChar, 100).Value = userCreditCard.CardHolderName;
        command.Parameters.Add("@CardNumberMasked", SqlDbType.NVarChar, 20).Value = userCreditCard.CardNumberMasked;
        command.Parameters.Add("@ExpiryMonth", SqlDbType.Int).Value = userCreditCard.ExpiryMonth;
        command.Parameters.Add("@ExpiryYear", SqlDbType.Int).Value = userCreditCard.ExpiryYear;
        command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = (object?)userCreditCard.CreatedBy ?? DBNull.Value;
        command.Parameters.Add("@CreatedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        var cardId = result != null ? (int)result : 0;

        return cardId;
    }

    public async Task<bool> UpdateUserCreditCardAsync(UserCreditCard userCreditCard)
    {
        const string sql = @"
            UPDATE UserCreditCards 
            SET UserID = @UserID, CardHolderName = @CardHolderName, CardNumberMasked = @CardNumberMasked, 
                ExpiryMonth = @ExpiryMonth, ExpiryYear = @ExpiryYear, UpdatedBy = @UpdatedBy, UpdatedDate = @UpdatedDate
            WHERE CardID = @CardID AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@CardID", SqlDbType.Int).Value = userCreditCard.CardID;
        command.Parameters.Add("@UserID", SqlDbType.Int).Value = userCreditCard.UserID;
        command.Parameters.Add("@CardHolderName", SqlDbType.NVarChar, 100).Value = userCreditCard.CardHolderName;
        command.Parameters.Add("@CardNumberMasked", SqlDbType.NVarChar, 20).Value = userCreditCard.CardNumberMasked;
        command.Parameters.Add("@ExpiryMonth", SqlDbType.Int).Value = userCreditCard.ExpiryMonth;
        command.Parameters.Add("@ExpiryYear", SqlDbType.Int).Value = userCreditCard.ExpiryYear;
        command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = (object?)userCreditCard.UpdatedBy ?? DBNull.Value;
        command.Parameters.Add("@UpdatedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteUserCreditCardAsync(int cardId)
    {
        const string sql = @"
            UPDATE UserCreditCards 
            SET IsDeleted = 1, DeletedDate = @DeletedDate, DeletedBy = @DeletedBy
            WHERE CardID = @CardID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@CardID", SqlDbType.Int).Value = cardId;
        command.Parameters.Add("@DeletedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;
        command.Parameters.Add("@DeletedBy", SqlDbType.Int).Value = DBNull.Value; // Should be set based on current user

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }
}
