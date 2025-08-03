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
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userCreditCards.Add(MapUserCreditCardFromReader(reader));
        }

        return userCreditCards;
    }

    public async Task<UserCreditCard?> GetUserCreditCardByIdAsync(int cardId)
    {
        const string sql = @"
            SELECT CardID, UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, 
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserCreditCards
            WHERE CardID = @CardId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CardId", cardId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapUserCreditCardFromReader(reader);
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
            WHERE UserID = @UserId AND IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userCreditCards.Add(MapUserCreditCardFromReader(reader));
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
            WHERE IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userCreditCards.Add(MapUserCreditCardFromReader(reader));
        }

        return userCreditCards;
    }

    public async Task<IEnumerable<UserCreditCard>> GetUserCreditCardsPaginatedAsync(int pageNumber, int pageSize)
    {
        var userCreditCards = new List<UserCreditCard>();
        const string sql = @"
            SELECT CardID, UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, 
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserCreditCards
            ORDER BY CreatedDate DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userCreditCards.Add(MapUserCreditCardFromReader(reader));
        }

        return userCreditCards;
    }

    public async Task<int> GetTotalUserCreditCardsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM UserCreditCards";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> GetActiveUserCreditCardsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM UserCreditCards WHERE IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<UserCreditCard>> SearchUserCreditCardsByNameAsync(string searchTerm)
    {
        var userCreditCards = new List<UserCreditCard>();
        const string sql = @"
            SELECT CardID, UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, 
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserCreditCards
            WHERE CardHolderName LIKE @SearchTerm OR CardNumberMasked LIKE @SearchTerm
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userCreditCards.Add(MapUserCreditCardFromReader(reader));
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
            WHERE CreatedDate >= @StartDate AND CreatedDate <= @EndDate
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userCreditCards.Add(MapUserCreditCardFromReader(reader));
        }

        return userCreditCards;
    }

    public async Task<int> CreateUserCreditCardAsync(UserCreditCard userCreditCard)
    {
        const string sql = @"
            INSERT INTO UserCreditCards (UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, CreatedBy, CreatedDate, IsDeleted)
            VALUES (@UserID, @CardHolderName, @CardNumberMasked, @ExpiryMonth, @ExpiryYear, @CreatedBy, @CreatedDate, @IsDeleted);
            SELECT SCOPE_IDENTITY();";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@UserID", userCreditCard.UserID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CardHolderName", userCreditCard.CardHolderName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CardNumberMasked", userCreditCard.CardNumberMasked ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ExpiryMonth", userCreditCard.ExpiryMonth ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ExpiryYear", userCreditCard.ExpiryYear ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", userCreditCard.CreatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedDate", userCreditCard.CreatedDate);
        command.Parameters.AddWithValue("@IsDeleted", userCreditCard.IsDeleted);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateUserCreditCardAsync(UserCreditCard userCreditCard)
    {
        const string sql = @"
            UPDATE UserCreditCards 
            SET UserID = @UserID, CardHolderName = @CardHolderName, CardNumberMasked = @CardNumberMasked, 
                ExpiryMonth = @ExpiryMonth, ExpiryYear = @ExpiryYear, UpdatedBy = @UpdatedBy, UpdatedDate = @UpdatedDate
            WHERE CardID = @CardID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@CardID", userCreditCard.CardID);
        command.Parameters.AddWithValue("@UserID", userCreditCard.UserID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CardHolderName", userCreditCard.CardHolderName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CardNumberMasked", userCreditCard.CardNumberMasked ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ExpiryMonth", userCreditCard.ExpiryMonth ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ExpiryYear", userCreditCard.ExpiryYear ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedBy", userCreditCard.UpdatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedDate", userCreditCard.UpdatedDate ?? (object)DBNull.Value);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteUserCreditCardAsync(int cardId)
    {
        const string sql = @"
            UPDATE UserCreditCards 
            SET IsDeleted = 1, DeletedDate = @DeletedDate 
            WHERE CardID = @CardID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CardID", cardId);
        command.Parameters.AddWithValue("@DeletedDate", DateTime.UtcNow);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static UserCreditCard MapUserCreditCardFromReader(SqlDataReader reader)
    {
        return new UserCreditCard
        {
            CardID = reader.GetInt32("CardID"),
            UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
            CardHolderName = reader.IsDBNull("CardHolderName") ? null : reader.GetString("CardHolderName"),
            CardNumberMasked = reader.IsDBNull("CardNumberMasked") ? null : reader.GetString("CardNumberMasked"),
            ExpiryMonth = reader.IsDBNull("ExpiryMonth") ? null : reader.GetInt32("ExpiryMonth"),
            ExpiryYear = reader.IsDBNull("ExpiryYear") ? null : reader.GetInt32("ExpiryYear"),
            CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
            CreatedDate = reader.GetDateTime("CreatedDate"),
            UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
            UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
            DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
            DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
            IsDeleted = reader.GetBoolean("IsDeleted")
        };
    }
}
