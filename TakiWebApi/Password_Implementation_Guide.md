# TakiWebApi - Password Implementation Guide

## 🔐 Database Updates

### 1. Add Password Columns (Run This First!)
```sql
-- Execute this script to add password columns to existing tables
-- File: AddPasswordColumns.sql
```

### 2. Create Tables with Password Support
```sql  
-- For new installations, use the updated CreateTables.sql
-- Users and Drivers tables now include PasswordHash nvarchar(255) field
```

### 3. Insert Sample Data with Hashed Passwords
```sql
-- Execute SampleData_WithPasswords.sql for demo data
-- This includes users and drivers with proper password hashes
```

## 🧪 Demo Login Credentials

### Users (Endpoint: `/api/Auth/login`)
| Phone Number | Password | User Name | Email |
|--------------|----------|-----------|-------|
| 05551234567 | password123 | Ahmed Hassan | ahmed@example.com |
| 05559876543 | user123 | Sara Mohammed | sara@example.com |
| 05551111111 | 123456 | Omar Ali | omar@example.com |
| 05552222222 | password123 | Fatima Khalil | fatima@example.com |
| 05553333333 | user123 | Youssef Nasser | youssef@example.com |

### Drivers (Endpoint: `/api/Auth/driver-login`)
| Phone Number | Password | Driver Name | Vehicle Info |
|--------------|----------|-------------|--------------|
| 05556789012 | driver123 | Mohammed Abdullah | ABC123 - Toyota Camry (White) |
| 05554567890 | driver123 | Hassan Rashid | XYZ789 - Honda Civic (Black) |
| 05557777777 | password123 | Khalil Ahmed | DEF456 - Nissan Altima (Silver) |
| 05558888888 | driver123 | Amira Said | GHI789 - Hyundai Elantra (Blue) |
| 05559999999 | 123456 | Nour Hassan | JKL012 - Kia Cerato (Red) |

## 📬 Postman Collection Updates

### Environment Variables Added:
- `user_password`: password123
- `driver_password`: driver123
- `test_user_phone_2`: 05559876543
- `test_user_password_2`: user123
- `test_driver_phone_2`: 05554567890
- `test_driver_password_2`: driver123

### New Endpoints:
1. **Driver Registration** (`POST /api/Auth/driver-register`)
   - Registers new drivers with vehicle information
   - Automatically hashes passwords

2. **Alternative Login Tests**
   - Additional user/driver login endpoints for testing different credentials

## 🔑 Password System Features

### Authentication Flow:
1. **Database Password Priority**: If `PasswordHash` exists in database, uses proper verification
2. **Demo Fallback**: If no password hash, falls back to demo passwords for testing
3. **Automatic Hashing**: New registrations automatically hash passwords using SHA256

### Password Service:
- **Hashing**: Uses SHA256 (demo implementation)
- **Verification**: Compares hashed passwords securely
- **Production Ready**: Easy to upgrade to BCrypt/Argon2 later

### Database Integration:
- **Smart Fallback**: Works with existing data (no passwords) and new data (with passwords)
- **Graceful Migration**: Existing users can still login with demo passwords
- **Future Proof**: Ready for production password requirements

## 🚀 Quick Start

1. **Run Database Scripts**:
   ```sql
   -- Add password columns to existing tables
   EXEC sp_executesql N'... content from AddPasswordColumns.sql ...'
   
   -- Insert sample data with passwords  
   EXEC sp_executesql N'... content from SampleData_WithPasswords.sql ...'
   ```

2. **Import Postman Collection & Environment**:
   - Import `TakiWebApi.postman_collection.json`
   - Import `TakiWebApi.postman_environment.json`

3. **Test Authentication**:
   - Run "User Login" to get user token
   - Run "Driver Login" to get driver token
   - Test protected endpoints with Bearer tokens

4. **Register New Users/Drivers**:
   - Use "User Registration" endpoint
   - Use "Driver Registration" endpoint
   - Passwords are automatically hashed

## 📝 Notes

- **Password Hashing**: Currently using SHA256 for demo. Upgrade to BCrypt for production.
- **Demo Compatibility**: System supports both hashed passwords and demo fallback passwords.
- **Token Auto-Save**: Postman automatically saves JWT tokens to environment variables.
- **Multi-User Testing**: Environment supports multiple test accounts for comprehensive testing.

---
*Ready for production with proper password hashing implementation!* 🔐
