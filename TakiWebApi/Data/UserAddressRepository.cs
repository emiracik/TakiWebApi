using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data
{
    public class UserAddressRepository : IUserAddressRepository
    {
        private readonly string _connectionString;

        public UserAddressRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServerConnection") 
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
        }

        public async Task<IEnumerable<UserAddress>> GetAllUserAddressesAsync()
        {
            var userAddresses = new List<UserAddress>();
            const string sql = @"
                SELECT UserAddressID, UserID, AddressName, FullAddress, Latitude, Longitude, IsDefault, AddressType,
                       CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
                FROM UserAddresses
                WHERE IsDeleted = 0
                ORDER BY CreatedDate DESC";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                userAddresses.Add(new UserAddress
                {
                    UserAddressID = reader.GetInt32("UserAddressID"),
                    UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
                    AddressName = reader.IsDBNull("AddressName") ? null : reader.GetString("AddressName"),
                    FullAddress = reader.IsDBNull("FullAddress") ? null : reader.GetString("FullAddress"),
                    Latitude = reader.IsDBNull("Latitude") ? null : reader.GetDouble("Latitude"),
                    Longitude = reader.IsDBNull("Longitude") ? null : reader.GetDouble("Longitude"),
                    IsDefault = reader.GetBoolean("IsDefault"),
                    AddressType = reader.IsDBNull("AddressType") ? null : reader.GetString("AddressType"),
                    CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                    CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                    UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                    UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                    DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                    DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                    IsDeleted = reader.GetBoolean("IsDeleted")
                });
            }

            return userAddresses;
        }

        public async Task<UserAddress?> GetUserAddressByIdAsync(int addressId)
        {
            const string sql = @"
                SELECT UserAddressID, UserID, AddressName, FullAddress, Latitude, Longitude, IsDefault, AddressType,
                       CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
                FROM UserAddresses
                WHERE UserAddressID = @AddressId AND IsDeleted = 0";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@AddressId", SqlDbType.Int).Value = addressId;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UserAddress
                {
                    UserAddressID = reader.GetInt32("UserAddressID"),
                    UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
                    AddressName = reader.IsDBNull("AddressName") ? null : reader.GetString("AddressName"),
                    FullAddress = reader.IsDBNull("FullAddress") ? null : reader.GetString("FullAddress"),
                    Latitude = reader.IsDBNull("Latitude") ? null : reader.GetDouble("Latitude"),
                    Longitude = reader.IsDBNull("Longitude") ? null : reader.GetDouble("Longitude"),
                    IsDefault = reader.GetBoolean("IsDefault"),
                    AddressType = reader.IsDBNull("AddressType") ? null : reader.GetString("AddressType"),
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

        public async Task<IEnumerable<UserAddress>> GetUserAddressesByUserIdAsync(int userId)
        {
            var userAddresses = new List<UserAddress>();
            const string sql = @"
                SELECT UserAddressID, UserID, AddressName, FullAddress, Latitude, Longitude, IsDefault, AddressType,
                       CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
                FROM UserAddresses
                WHERE UserID = @UserId AND IsDeleted = 0
                ORDER BY IsDefault DESC, CreatedDate DESC";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                userAddresses.Add(new UserAddress
                {
                    UserAddressID = reader.GetInt32("UserAddressID"),
                    UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
                    AddressName = reader.IsDBNull("AddressName") ? null : reader.GetString("AddressName"),
                    FullAddress = reader.IsDBNull("FullAddress") ? null : reader.GetString("FullAddress"),
                    Latitude = reader.IsDBNull("Latitude") ? null : reader.GetDouble("Latitude"),
                    Longitude = reader.IsDBNull("Longitude") ? null : reader.GetDouble("Longitude"),
                    IsDefault = reader.GetBoolean("IsDefault"),
                    AddressType = reader.IsDBNull("AddressType") ? null : reader.GetString("AddressType"),
                    CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                    CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                    UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                    UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                    DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                    DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                    IsDeleted = reader.GetBoolean("IsDeleted")
                });
            }

            return userAddresses;
        }

        public async Task<IEnumerable<UserAddress>> GetActiveUserAddressesAsync()
        {
            var userAddresses = new List<UserAddress>();
            const string sql = @"
                SELECT UserAddressID, UserID, AddressName, FullAddress, Latitude, Longitude, IsDefault, AddressType,
                       CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
                FROM UserAddresses
                WHERE IsDeleted = 0
                ORDER BY IsDefault DESC, CreatedDate DESC";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                userAddresses.Add(new UserAddress
                {
                    UserAddressID = reader.GetInt32("UserAddressID"),
                    UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
                    AddressName = reader.IsDBNull("AddressName") ? null : reader.GetString("AddressName"),
                    FullAddress = reader.IsDBNull("FullAddress") ? null : reader.GetString("FullAddress"),
                    Latitude = reader.IsDBNull("Latitude") ? null : reader.GetDouble("Latitude"),
                    Longitude = reader.IsDBNull("Longitude") ? null : reader.GetDouble("Longitude"),
                    IsDefault = reader.GetBoolean("IsDefault"),
                    AddressType = reader.IsDBNull("AddressType") ? null : reader.GetString("AddressType"),
                    CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                    CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                    UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                    UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                    DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                    DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                    IsDeleted = reader.GetBoolean("IsDeleted")
                });
            }

            return userAddresses;
        }

        public async Task<IEnumerable<UserAddress>> GetUserAddressesPaginatedAsync(int pageNumber, int pageSize)
        {
            var userAddresses = new List<UserAddress>();
            var offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                SELECT UserAddressID, UserID, AddressName, FullAddress, Latitude, Longitude, IsDefault, AddressType,
                       CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
                FROM UserAddresses
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
                userAddresses.Add(new UserAddress
                {
                    UserAddressID = reader.GetInt32("UserAddressID"),
                    UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
                    AddressName = reader.IsDBNull("AddressName") ? null : reader.GetString("AddressName"),
                    FullAddress = reader.IsDBNull("FullAddress") ? null : reader.GetString("FullAddress"),
                    Latitude = reader.IsDBNull("Latitude") ? null : reader.GetDouble("Latitude"),
                    Longitude = reader.IsDBNull("Longitude") ? null : reader.GetDouble("Longitude"),
                    IsDefault = reader.GetBoolean("IsDefault"),
                    AddressType = reader.IsDBNull("AddressType") ? null : reader.GetString("AddressType"),
                    CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                    CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                    UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                    UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                    DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                    DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                    IsDeleted = reader.GetBoolean("IsDeleted")
                });
            }

            return userAddresses;
        }

        public async Task<int> GetTotalUserAddressesCountAsync()
        {
            const string sql = "SELECT COUNT(*) FROM UserAddresses WHERE IsDeleted = 0";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return result != null ? (int)result : 0;
        }

        public async Task<int> GetActiveUserAddressesCountAsync()
        {
            const string sql = "SELECT COUNT(*) FROM UserAddresses WHERE IsDeleted = 0";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return result != null ? (int)result : 0;
        }

        public async Task<IEnumerable<UserAddress>> SearchUserAddressesByTitleAsync(string searchTerm)
        {
            var userAddresses = new List<UserAddress>();
            const string sql = @"
                SELECT UserAddressID, UserID, AddressName, FullAddress, Latitude, Longitude, IsDefault, AddressType,
                       CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
                FROM UserAddresses
                WHERE IsDeleted = 0 AND (
                    AddressName LIKE @SearchTerm OR 
                    FullAddress LIKE @SearchTerm OR
                    AddressType LIKE @SearchTerm
                )
                ORDER BY CreatedDate DESC";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = $"%{searchTerm}%";

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                userAddresses.Add(new UserAddress
                {
                    UserAddressID = reader.GetInt32("UserAddressID"),
                    UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
                    AddressName = reader.IsDBNull("AddressName") ? null : reader.GetString("AddressName"),
                    FullAddress = reader.IsDBNull("FullAddress") ? null : reader.GetString("FullAddress"),
                    Latitude = reader.IsDBNull("Latitude") ? null : reader.GetDouble("Latitude"),
                    Longitude = reader.IsDBNull("Longitude") ? null : reader.GetDouble("Longitude"),
                    IsDefault = reader.GetBoolean("IsDefault"),
                    AddressType = reader.IsDBNull("AddressType") ? null : reader.GetString("AddressType"),
                    CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                    CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                    UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                    UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                    DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                    DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                    IsDeleted = reader.GetBoolean("IsDeleted")
                });
            }

            return userAddresses;
        }

        public async Task<IEnumerable<UserAddress>> GetUserAddressesByCreatedDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var userAddresses = new List<UserAddress>();
            const string sql = @"
                SELECT UserAddressID, UserID, AddressName, FullAddress, Latitude, Longitude, IsDefault, AddressType,
                       CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
                FROM UserAddresses
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
                userAddresses.Add(new UserAddress
                {
                    UserAddressID = reader.GetInt32("UserAddressID"),
                    UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
                    AddressName = reader.IsDBNull("AddressName") ? null : reader.GetString("AddressName"),
                    FullAddress = reader.IsDBNull("FullAddress") ? null : reader.GetString("FullAddress"),
                    Latitude = reader.IsDBNull("Latitude") ? null : reader.GetDouble("Latitude"),
                    Longitude = reader.IsDBNull("Longitude") ? null : reader.GetDouble("Longitude"),
                    IsDefault = reader.GetBoolean("IsDefault"),
                    AddressType = reader.IsDBNull("AddressType") ? null : reader.GetString("AddressType"),
                    CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                    CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                    UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                    UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                    DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
                    DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
                    IsDeleted = reader.GetBoolean("IsDeleted")
                });
            }

            return userAddresses;
        }

        public async Task<int> CreateUserAddressAsync(UserAddress userAddress)
        {
            const string sql = @"
                INSERT INTO UserAddresses (UserID, AddressName, FullAddress, Latitude, Longitude, IsDefault, AddressType, CreatedBy, CreatedDate)
                OUTPUT INSERTED.UserAddressID
                VALUES (@UserID, @AddressName, @FullAddress, @Latitude, @Longitude, @IsDefault, @AddressType, @CreatedBy, @CreatedDate)";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@UserID", SqlDbType.Int).Value = (object?)userAddress.UserID ?? DBNull.Value;
            command.Parameters.Add("@AddressName", SqlDbType.NVarChar, 100).Value = (object?)userAddress.AddressName ?? DBNull.Value;
            command.Parameters.Add("@FullAddress", SqlDbType.NVarChar, 500).Value = (object?)userAddress.FullAddress ?? DBNull.Value;
            command.Parameters.Add("@Latitude", SqlDbType.Float).Value = (object?)userAddress.Latitude ?? DBNull.Value;
            command.Parameters.Add("@Longitude", SqlDbType.Float).Value = (object?)userAddress.Longitude ?? DBNull.Value;
            command.Parameters.Add("@IsDefault", SqlDbType.Bit).Value = userAddress.IsDefault;
            command.Parameters.Add("@AddressType", SqlDbType.NVarChar, 50).Value = (object?)userAddress.AddressType ?? DBNull.Value;
            command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = (object?)userAddress.CreatedBy ?? DBNull.Value;
            command.Parameters.Add("@CreatedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return result != null ? (int)result : 0;
        }

        public async Task<bool> UpdateUserAddressAsync(UserAddress userAddress)
        {
            const string sql = @"
                UPDATE UserAddresses 
                SET UserID = @UserID, AddressName = @AddressName, FullAddress = @FullAddress, 
                    Latitude = @Latitude, Longitude = @Longitude, IsDefault = @IsDefault, 
                    AddressType = @AddressType, UpdatedBy = @UpdatedBy, UpdatedDate = @UpdatedDate
                WHERE UserAddressID = @UserAddressID AND IsDeleted = 0";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@UserAddressID", SqlDbType.Int).Value = userAddress.UserAddressID;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = (object?)userAddress.UserID ?? DBNull.Value;
            command.Parameters.Add("@AddressName", SqlDbType.NVarChar, 100).Value = (object?)userAddress.AddressName ?? DBNull.Value;
            command.Parameters.Add("@FullAddress", SqlDbType.NVarChar, 500).Value = (object?)userAddress.FullAddress ?? DBNull.Value;
            command.Parameters.Add("@Latitude", SqlDbType.Float).Value = (object?)userAddress.Latitude ?? DBNull.Value;
            command.Parameters.Add("@Longitude", SqlDbType.Float).Value = (object?)userAddress.Longitude ?? DBNull.Value;
            command.Parameters.Add("@IsDefault", SqlDbType.Bit).Value = userAddress.IsDefault;
            command.Parameters.Add("@AddressType", SqlDbType.NVarChar, 50).Value = (object?)userAddress.AddressType ?? DBNull.Value;
            command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = (object?)userAddress.UpdatedBy ?? DBNull.Value;
            command.Parameters.Add("@UpdatedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteUserAddressAsync(int addressId)
        {
            const string sql = @"
                UPDATE UserAddresses 
                SET IsDeleted = 1, DeletedDate = @DeletedDate, DeletedBy = @DeletedBy
                WHERE UserAddressID = @UserAddressID";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@UserAddressID", SqlDbType.Int).Value = addressId;
            command.Parameters.Add("@DeletedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;
            command.Parameters.Add("@DeletedBy", SqlDbType.Int).Value = DBNull.Value; // Should be set based on current user

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // First, set all user's addresses to non-default
                const string resetSql = @"
                    UPDATE UserAddresses 
                    SET IsDefault = 0, UpdatedDate = @UpdatedDate 
                    WHERE UserID = @UserID AND IsDeleted = 0";

                using var resetCommand = new SqlCommand(resetSql, connection, transaction);
                resetCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                resetCommand.Parameters.Add("@UpdatedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;
                await resetCommand.ExecuteNonQueryAsync();

                // Then, set the specified address as default
                const string setDefaultSql = @"
                    UPDATE UserAddresses 
                    SET IsDefault = 1, UpdatedDate = @UpdatedDate 
                    WHERE UserAddressID = @AddressID AND UserID = @UserID AND IsDeleted = 0";

                using var setDefaultCommand = new SqlCommand(setDefaultSql, connection, transaction);
                setDefaultCommand.Parameters.Add("@AddressID", SqlDbType.Int).Value = addressId;
                setDefaultCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                setDefaultCommand.Parameters.Add("@UpdatedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;
                
                var rowsAffected = await setDefaultCommand.ExecuteNonQueryAsync();
                
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}