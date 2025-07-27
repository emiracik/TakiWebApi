# TaksiWebApi Postman Collection

Bu döküman TaksiWebApi için hazırlanmış Postman collection'ının nasıl kullanılacağını açıklar.

## 📁 Dosyalar

- `TaksiWebApi_Postman_Collection.json` - Ana Postman collection dosyası
- `TaksiWebApi_Environment.postman_environment.json` - Environment variables dosyası
- `README_Postman.md` - Bu döküman

## 🚀 Kurulum

### 1. Postman'e Import Etme

1. **Postman'ı açın**
2. **Import** butonuna tıklayın
3. **File** sekmesini seçin
4. Aşağıdaki dosyaları sırayla import edin:
   - `TaksiWebApi_Postman_Collection.json`
   - `TaksiWebApi_Environment.postman_environment.json`

### 2. Environment Ayarlama

1. Postman'da sağ üst köşedeki **Environment** dropdown'ını açın
2. **TaksiWebApi Environment** seçin
3. Environment'ın aktif olduğundan emin olun

## 🔧 Environment Variables

Collection aşağıdaki environment variable'ları kullanır:

| Variable | Değer | Açıklama |
|----------|-------|----------|
| `baseUrl` | `https://localhost:5001` | HTTPS Base URL |
| `httpUrl` | `http://localhost:5086` | HTTP Base URL |
| `userId` | `1` | Test için örnek User ID |
| `driverId` | `1` | Test için örnek Driver ID |
| `tripId` | `1` | Test için örnek Trip ID |
| `userEmail` | `ahmet.yilmaz@email.com` | Test için örnek email |
| `userPhone` | `+905551234567` | Test için örnek telefon |
| `vehiclePlate` | `34ABC123` | Test için örnek plaka |
| `searchTerm` | `Ahmet` | Test için arama terimi |
| `startDate` | `2024-01-01` | Tarih aralığı başlangıcı |
| `endDate` | `2025-12-31` | Tarih aralığı sonu |
| `pageNumber` | `1` | Sayfalama - sayfa numarası |
| `pageSize` | `5` | Sayfalama - sayfa boyutu |

## 📋 Collection Yapısı

Collection aşağıdaki ana kategorileri içerir:

### 👥 Users (10 endpoint)
- Get All Users
- Get User by ID
- Get User by Phone/Email
- Get Active Users
- Pagination & Search
- Count operations

### 🚗 Drivers (12 endpoint)
- Get All Drivers
- Get Driver by ID
- Get Driver by Phone/Email
- Get Active Drivers
- Vehicle plate search
- Pagination & Search
- Count operations

### 🚖 Trips (11 endpoint)
- Get All Trips
- Get Trip by ID
- Get Trips by Passenger/Driver
- Payment method filtering
- Date range queries
- Cost calculations
- Pagination & Count

### 📢 Announcements (8 endpoint)
- Get All Announcements
- Get Announcement by ID
- Get Active Announcements
- Search & Date filtering
- Pagination & Count

### 📝 Blogs (9 endpoint)
- Get All Blogs
- Get Blog by ID
- Get Published/Active Blogs
- Search & Date filtering
- Pagination & Count

### ❓ FAQs (7 endpoint)
- Get All FAQs
- Get FAQ by ID
- Get Active FAQs
- Search functionality
- Pagination & Count

## 🎯 Kullanım Örnekleri

### Temel Test Sırası

1. **API'nin çalıştığını kontrol edin:**
   ```
   GET {{baseUrl}}/api/users
   ```

2. **Specific data alın:**
   ```
   GET {{baseUrl}}/api/users/{{userId}}
   GET {{baseUrl}}/api/drivers/{{driverId}}
   ```

3. **Arama yapın:**
   ```
   GET {{baseUrl}}/api/users/search?searchTerm={{searchTerm}}
   ```

4. **Pagination test edin:**
   ```
   GET {{baseUrl}}/api/users/paginated?pageNumber={{pageNumber}}&pageSize={{pageSize}}
   ```

### Turkish Data Examples

Collection Turkish sample data ile optimize edilmiştir:

- **Search Terms:** "Ahmet", "Mehmet", "taksi", "ödeme", "güncelleme"
- **Vehicle Plates:** "34ABC123", "06XYZ789", "35DEF456"
- **Phone Numbers:** Turkish format (+90555...)
- **Email Addresses:** Turkish domain names

## 🔍 Test Senaryoları

### 1. Kullanıcı Yönetimi Testi
```
1. GET /api/users (tüm kullanıcılar)
2. GET /api/users/1 (specific user)
3. GET /api/users/phone/+905551234567 (telefon ile arama)
4. GET /api/users/search?searchTerm=Ahmet (isim arama)
5. GET /api/users/count (toplam sayı)
```

### 2. Sürücü Yönetimi Testi
```
1. GET /api/drivers (tüm sürücüler)
2. GET /api/drivers/active (aktif sürücüler)
3. GET /api/drivers/vehicle-plate/34ABC123 (plaka arama)
4. GET /api/drivers/search?searchTerm=Mehmet (isim arama)
```

### 3. Yolculuk Analizi Testi
```
1. GET /api/trips (tüm yolculuklar)
2. GET /api/trips/passenger/1 (kullanıcının yolculukları)
3. GET /api/trips/driver/1 (sürücünün yolculukları)
4. GET /api/trips/cost/date-range?startDate=2024-01-01&endDate=2025-12-31
```

## 🛠️ Troubleshooting

### Yaygın Sorunlar

1. **Connection Error:**
   - TaksiWebApi'nin çalıştığından emin olun
   - Port numaralarını kontrol edin (5001/5086)
   - SSL sertifika sorunları için HTTP URL kullanın

2. **404 Not Found:**
   - Endpoint URL'lerini kontrol edin
   - Base URL'in doğru olduğundan emin olun

3. **Empty Response:**
   - Database'de data olduğundan emin olun
   - SampleInserts.sql script'ini çalıştırın

### Debug İpuçları

1. **Console Log:** Postman Console'da request/response detaylarını inceleyin
2. **Environment Check:** Variable'ların doğru set edildiğini kontrol edin  
3. **Response Time:** Yavaş response'lar için database connection'ı kontrol edin

## 📊 Başarı Metrikleri

Collection test edildiğinde şu sonuçları görmelisiniz:

- ✅ **Users:** 5 kullanıcı
- ✅ **Drivers:** 5 sürücü  
- ✅ **Trips:** 10 yolculuk
- ✅ **Announcements:** 5 duyuru
- ✅ **Blogs:** 5 blog yazısı
- ✅ **FAQs:** 8 sık sorulan soru

## 🎉 Collection Özellikleri

- **67 Total Endpoints:** Tüm CRUD operasyonları
- **Turkish Sample Data:** Gerçekçi Türkçe test verisi
- **Environment Variables:** Kolay configuration
- **Organized Structure:** Kategoriler halinde düzenlenmiş
- **Real-world Examples:** İstanbul routes, Turkish plates
- **Comprehensive Testing:** Pagination, search, filtering

Bu collection ile TaksiWebApi'nizin tüm özelliklerini kolayca test edebilirsiniz! 🚖✨
