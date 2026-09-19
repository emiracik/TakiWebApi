# panel.veloxtaxi.com — Yönetim Paneli Dokümantasyonu

> Playwright ile panele giriş yapılıp (emiracik2@gmail.com) tüm menüler gezilerek,
> ağ trafiği ve `/api/panel/modules` şema ucundan çıkarılmıştır. Tarih: 2026‑09‑10.

---

## 1. Genel Mimari

| | |
|---|---|
| Panel adresi | `https://panel.veloxtaxi.com` |
| Sunucu | ASP.NET / IIS 10 (bizim `apivelox.truvaburo.com` API'sinden **ayrı** bir uygulama) |
| Panel API kökü | `https://panel.veloxtaxi.com/api` |
| Frontend | SPA (hash routing: `/#Users`, `/#Trips` …), CSP `connect-src 'self'` → yalnızca kendi API'sini çağırır |
| Kimlik | Çerez tabanlı oturum (`POST /api/auth/login` sonrası cookie) |
| **Önemli** | Panel, bizim mobil API'mizi (`apivelox.truvaburo.com`) **çağırmaz**. Aynı **veritabanını** (VeloxTaxiDb) tablo tablo yönetir. Yani panel ile mobil API "aynı veriyi" farklı uçlardan gösterir. |

### Yanıt zarfı (bütün panel uçları)
```json
{ "returnCode": 0, "message": "…", "data": null, "objectData": <içerik> }
```
Liste uçlarında `objectData` şu şekildedir:
```json
{ "rows": [ … ], "total": 123, "page": 1, "pageSize": 20 }
```
İlişkili (FK) sütunlar için satırda ayrıca `__DriverID`, `__PassengerID`, `__CreatedBy` gibi
**çözülmüş görünen değerler** (isim/etiket) döner.

---

## 2. Kimlik Doğrulama Uçları

| Metod | Uç | Ne için |
|---|---|---|
| POST | `/api/auth/login` | E‑posta + şifre ile giriş (gövde: `{email, password}`) |
| GET  | `/api/auth/session` | Oturum geçerli mi? (her sayfa açılışında çağrılıyor) |
| GET  | `/api/auth/profile` | Giriş yapan yöneticinin profili (Profil detayları menüsü) |

Bizim API karşılığı: `POST /api/Auth/login` (yolcu), `POST /api/Auth/driver-login` (sürücü),
`POST /api/Auth/refresh-token`. Panelin admin girişi ayrı (`Admins` tablosu).

---

## 3. Genel CRUD Şablonu

Panel **generic bir DB yönetim paneli**. Her modül aynı uç kalıbını kullanır
(modül adı = tablo adı, örn. `Users`, `Trips`, `Wallets`):

| Metod | Uç | Açıklama | Koşul |
|---|---|---|---|
| GET | `/api/panel/{Modül}/list?page=&pageSize=&search=&sort=&descending=` | Sayfalı liste + arama + sıralama | Her modül |
| GET | `/api/panel/{Modül}/{id}` | Tek kayıt detayı | Her modül |
| POST | `/api/panel/{Modül}` | Yeni kayıt | `readOnly = false` modüller |
| PUT | `/api/panel/{Modül}/{id}` | Kayıt güncelle (yalnız `editable` alanlar) | `readOnly = false` |
| DELETE | `/api/panel/{Modül}/{id}` | Kayıt sil (soft delete — `DeletedDate`) | `canDelete = true` |
| POST | `/api/panel/{Modül}/{id}/adjust` | **Bakiye/tutar düzeltmesi** (tutar + gerekçe, audit'e yazılır) | `canAdjust = true` (yalnız `Wallets`) |

`search` = serbest metin araması, `sort` = sütun adı, `descending` = `true/false`.
Alan meta verisi (`type`, `required`, `editable`, `maxLength`, `relation`, `options`)
`GET /api/panel/modules` ucundan gelir.

---

## 4. Özel (CRUD dışı) Uçlar

| Metod | Uç | Menü | İçerik | Bizim API karşılığı |
|---|---|---|---|---|
| GET | `/api/panel/dashboard` | Ana ekran | `stats[]` (modül sayıları + dönemsel değişim), `financial{WalletBalance, CompletedRevenue, OnlineDrivers}`, `trend[]` (günlük yolculuk), `recent[]` (son yolculuklar) | Parçalı: `/api/Matching/statistics/*`, `/api/Trips/count`, `/api/Trips/active/count`, `/api/Trips/cost/date-range` |
| GET | `/api/panel/modules` | (önyüz init) | 34 modülün tam şeması (grup, key, izinler, alan listesi) | — (panel'e özel) |
| GET | `/api/panel/live-drivers` | Canlı sürücü haritası | Sürücü + konum + `LocationAgeSeconds` + varsa aktif `TripID/TripStatus/StartAddress` | `GET /api/Matching/all-driver-locations` (Admin) — ama trip birleşimi bizde yok |
| GET | `/api/panel/notification-center` | Bildirim çanı | Panel içi bildirim özeti | — |
| GET | `/api/panel/audit?page=&adminId=` | İşlem geçmişi | Yönetici işlem günlüğü (kim neyi değiştirdi) | — (panel'e özel) |

> `live-drivers` yanıtındaki `LocationAgeSeconds` = "konum kaç saniye önce güncellendi".
> Bizim eşleştirmedeki 30 dk (1800 sn) eşiğinin üstündekiler "bayat" sayılır.

---

## 5. Modüller — Menü → Panel Ucu → Bizim API Karşılığı

Panel 34 modülü **7 grup** altında listeler. `RO` = readOnly (sadece görüntüleme),
`DEL` = silinebilir, `ADJ` = düzeltme (adjust) yapılabilir.

### 5.1. Grup: Yolculuk Yönetimi

| Menü | Panel modülü / uç | İzin | Bizim API (`apivelox.truvaburo.com`) | Ne için |
|---|---|---|---|---|
| Yolcular | `Users` `/api/panel/Users/*` | DEL | `UsersController` `/api/Users`, `/api/Auth/register` | Yolcu hesapları: FullName, PhoneNumber, Email, IsActive |
| Sürücüler | `Drivers` `/api/panel/Drivers/*` | DEL | `DriversController` `/api/Drivers`, `/api/Auth/driver-login` | Sürücü hesapları + araç: Plate, Model, Color |
| Yolculuklar | `Trips` `/api/panel/Trips/*` | DEL | `TripsController` `/api/Trips` (+ bu oturumda eklenenler: `/driver/{id}/active`, `/{id}/driver-finish`, `/{id}/confirm`, `/{id}/complete`, `/{id}/payment-method`) | Yolculuk kaydı: PassengerID→Users, DriverID→Drivers, Start/End adres+koordinat, Cost, **PaymentMethod**, Distance, **Status** (`inProgress`/`awaitingConfirmation`/`completed`/`cancelled`) |
| Eşleşme talepleri | `MatchingRequests` `/api/panel/MatchingRequests/list` | **RO** | `MatchingController` `/api/Matching/*` (find-best-match, accept, reject, status) | Eşleştirme talepleri: Status (int: 0 Pending, 1 Accepted, 2 Rejected, 3 Cancelled, 4 Expired), EstimatedCost/Distance/Duration, ExpiryTime |
| Sürücü konumları | `DriverLocations` `/api/panel/DriverLocations/list` | **RO** | `MatchingController` `/api/Matching/driver/{id}/location` (GET/PUT), `/all-driver-locations`, `/nearby-drivers` | Anlık konum: Lat/Lng, **IsAvailable**, **IsOnline**, LocationTime, Speed, Heading. Sürücü başına tek satır. |
| Paylaşımlı yolculuklar | `SharedRides` `/api/panel/SharedRides/*` | DEL | `SharedRidesController` `/api/SharedRides` | TripID→Trips, PassengerCount, Status |
| Sürücü detayları | `DriverDetails` `/api/panel/DriverDetails/*` | DEL | **Yok** (bizde ayrı `Drivers` tablosu var) | UserID→Users, LicenseNumber, ExperienceYears, RatingAverage — panelde `Drivers`'tan farklı, User'a bağlı ikinci bir sürücü modeli |
| Sürücü belgeleri | `DriverDocuments` `/api/panel/DriverDocuments/*` | DEL | `DriverDocumentsController` `/api/DriverDocuments` | DriverID→Drivers, DocumentType, DocumentUrl, UploadedDate |
| Yolcu adresleri | `UserAddresses` `/api/panel/UserAddresses/*` | DEL | `UserAddressesController` `/api/UserAddresses/user/{id}` | Kayıtlı adresler: AddressName, FullAddress, Lat/Lng, IsDefault, AddressType (Ev/İş) |

### 5.2. Grup: Finans

| Menü | Panel modülü / uç | İzin | Bizim API | Ne için |
|---|---|---|---|---|
| Cüzdanlar | `Wallets` `/api/panel/Wallets/*` | **RO + ADJ** | `WalletController` `/api/Wallet/{userId}`, `/deposit`, `/withdraw` | Cüzdan: UserID→Users, Balance, TotalIn, TotalOut. **Panel `/adjust` ile elle bakiye düzeltebilir (tutar + gerekçe)** — mobil `deposit/withdraw` ile aynı tabloyu değiştirir |
| Cüzdan hareketleri | `WalletTransactions` `/api/panel/WalletTransactions/list` | **RO** | `WalletTransactionsController` + `/api/Wallet/{userId}/transactions` | Her hareket: WalletID, UserID, TransactionType (`Deposit`/`Withdraw`/`TripPayment`), Amount (±), Description, TransactionDate |
| Faturalar | `Invoices` `/api/panel/Invoices/list` | **RO** | `InvoicesController` `/api/Invoices` | UserID→Users, TripID→Trips, Amount, InvoiceDate, Status |
| Sürücü kazançları | `DriverEarnings` `/api/panel/DriverEarnings/list` | **RO** | **Yok — bizim API'de controller yok** | DriverID→Drivers, TripID→Trips, Amount, EarnedAt. ⚠️ Bu tablo boş; mobil "sürücü cüzdanı" tamamlanan `Trips.Cost` toplamından hesaplanıyor. Öneri: yolculuk tamamlanınca buraya satır yaz → panelde otomatik görünür |
| Banka bilgileri | `BankInfo` `/api/panel/BankInfo/*` | DEL | `BankInfoController` `/api/BankInfo` | Sürücü IBAN: DriverID→Drivers, BankName, IBAN, AccountNumber (ödeme/hakediş için) |
| Kayıtlı kartlar | `CreditCards` `/api/panel/CreditCards/list` | **RO** | `CreditCardsController` `/api/CreditCards` | CardID, UserID→Users, CardNumber, CardHolder, ExpiryMonth/Year, IsDefault. ⚠️ Mobil uygulama `UserCreditCards` (`/api/UserCreditCards/user/{id}`) tablosunu kullanıyor — **iki ayrı kart tablosu var** |

### 5.3. Grup: Destek

| Menü | Panel modülü / uç | İzin | Bizim API | Ne için |
|---|---|---|---|---|
| Destek talepleri | `SupportTickets` `/api/panel/SupportTickets/*` | DEL | **Yok** | Subject, Description, UserID/DriverID, Status, Priority, AssignedTo→Admins, ResolvedDate |
| Geri bildirimler | `Feedbacks` `/api/panel/Feedbacks/list` | **RO** | `FeedbacksController` `/api/Feedbacks` | UserID/DriverID/TripID, Rating (decimal), Comment |
| Mesajlar | `Messages` `/api/panel/Messages/list` | **RO** | `MessagesController` `/api/Messages`, `/api/Messages/user/{id}` | Yolcu↔sürücü sohbeti: SenderID/ReceiverID (→Users; sürücü NEGATİF id ile), Content, SentAt, IsRead. ⚠️ Panel `SenderID/ReceiverID`'yi `Users`'a bağlıyor; sürücü mesajlarında id negatif olduğu için panelde çözümlenemeyebilir |

### 5.4. Grup: Değerlendirmeler (hepsi RO)

| Menü | Panel modülü | Bizim API | Alanlar |
|---|---|---|---|
| Sürücü puanları | `DriverRatings` | `DriverRatingsController` `/api/DriverRatings/driver/{id}`, `/driver/{id}/average` | TripID, DriverID, UserID, Rating, Comment |
| Yolcu puanları | `UserRatings` | `UserRatingsController` `/api/UserRatings/user/{id}/average` | TripID, RatedUserID, RatedByDriverID, Rating, Comment |
| Yolculuk puanları | `TripRatings` | `TripRatingsController` `/api/TripRatings/trip/{id}` | TripID, RatedByUserID, Rating, Comment |
| Sürücü yorumları | `DriverReviews` | **Yok** | TripID, DriverID, UserID, ReviewText, CreatedAt |

### 5.5. Grup: İçerik

| Menü | Panel modülü / uç | İzin | Bizim API | Ne için |
|---|---|---|---|---|
| Duyurular | `Announcements` `/api/panel/Announcements/*` | DEL | `AnnouncementsController` `/api/announcements`, `/active` | Title, Content, PublishedAt |
| Duyuru okunmaları | `AnnouncementRead` `/api/panel/AnnouncementRead/list` | **RO** | — | userId→Users, announcementId→Announcements, createdDate |
| Blog yazıları | `Blogs` `/api/panel/Blogs/*` | DEL | `BlogsController` `/api/blogs` | Title, Content, ImageUrl, PublishedAt, IsPublished |
| Sık sorulan sorular | `FAQs` `/api/panel/FAQs/*` | DEL | `FAQsController` `/api/faqs`, `/active` | Question, Answer, IsActive, SortOrder |
| Kampanyalar | `Promotions` `/api/panel/Promotions/*` | DEL | **Yok** | Title, DiscountType, DiscountValue, MinimumAmount, MaximumDiscount, StartDate/EndDate, UsageLimit, UsedCount, IsActive |
| Bildirimler | `Notifications` `/api/panel/Notifications/*` | DEL | `NotificationsController` `/api/Notifications/user/{id}`, `/unread`, `/mark-as-read` | Title, Message, UserID/DriverID, NotificationType, IsRead, ReadDate |

### 5.6. Grup: Sistem

| Menü | Panel modülü / uç | İzin | Bizim API | Ne için |
|---|---|---|---|---|
| Bildirim tercihleri | `UserNotificationSettings` `/api/panel/UserNotificationSettings/*` | DEL | `UserNotificationSettingsController` `/api/UserNotificationSettings/user/{id}` | UserID, AllowPromotions, AllowTripUpdates, AllowNews |
| Uygulama sürümleri | `AppVersions` `/api/panel/AppVersions/*` | DEL | **Yok** | Platform, VersionNumber, IsMandatory, ReleaseNotes, ReleaseDate — zorunlu güncelleme kontrolü |
| OTP kayıtları | `OtpRequests` `/api/panel/OtpRequests/list` | **RO** | **Yok** (Auth/OTP akışı içinde) | UserID, PhoneNumber, ExpirationTime, IsUsed, UsedAt, RequestIP, RequestSource |
| SMS kayıtları | `SmsLogs` `/api/panel/SmsLogs/list` | **RO** | **Yok** | PhoneNumber, IsSuccess, SentAt, OTPID→OtpRequests |

### 5.7. Grup: Yönetim (panel'e özel — mobil API'de yok)

| Menü | Panel modülü / uç | İzin | Ne için |
|---|---|---|---|
| Yöneticiler | `Admins` `/api/panel/Admins/*` | DEL | Panel kullanıcıları: FullName, Email, Phone, RoleID→AdminRoles, IsActive, LastLoginDate |
| Roller ve yetkiler | `AdminRoles` `/api/panel/AdminRoles/list` | **RO** | RoleName, Description |
| İşlem geçmişi | `audit` `/api/panel/audit?page=&adminId=` | **RO** | Tüm panel değişikliklerinin günlüğü (kim, ne zaman, hangi kayıt) |

---

## 6. Bu Oturumda Yapılan Mobil Özellikler → Panelde Nerede Görünür

| Mobil özellik | İlgili tablo | Panel menüsü | Not |
|---|---|---|---|
| Eşleştirme (find‑best‑match) | `MatchingRequests`, `DriverLocations` | Eşleşme talepleri, Sürücü konumları, Canlı sürücü haritası | Panel RO — sadece izler |
| Yolculuk yaşam döngüsü (`inProgress`→`awaitingConfirmation`→`completed`) | `Trips.Status` | Yolculuklar | Panel `Status`'ü serbest metin gösterir; yeni `awaitingConfirmation` değeri de görünür |
| KM bazlı ücret (UKOME tarifesi) | `Trips.Cost`, `MatchingRequests.EstimatedCost` | Yolculuklar, Eşleşme talepleri | — |
| Ödeme yöntemi (Cüzdan/Kart) | `Trips.PaymentMethod` | Yolculuklar | Panelde `PaymentMethod` sütunu düzenlenebilir |
| Cüzdandan tahsilat | `Wallets.Balance`, `WalletTransactions` (`TransactionType='TripPayment'`) | Cüzdanlar, Cüzdan hareketleri | Panel `/adjust` ile elle düzeltme yapılabilir |
| Yolcuya 1.500 TL yükleme | `WalletTransactions` (`Deposit`) | Cüzdan hareketleri | — |
| Sürücü kazancı ("Kazançlarım") | (şu an `Trips.Cost` toplamı) | Sürücü kazançları (`DriverEarnings` — **boş**) | Öneri: tamamlanınca `DriverEarnings` yaz |
| Yolcu ↔ sürücü sohbeti + yolculuk bazlı filtre | `Messages` | Mesajlar | — |
| Yolcu onayı (driver‑finish → confirm) | `Trips.Status='awaitingConfirmation'` | Yolculuklar, dashboard `recent[]` | — |

---

## 7. Dikkat Edilmesi Gereken Şema Farkları / Öneriler

1. **İki kart tablosu**: Panel `CreditCards`, mobil `UserCreditCards`. Hangisi kanonik olacak netleştirilmeli.
2. **İki sürücü modeli**: Panel hem `Drivers` (bağımsız) hem `DriverDetails` (User'a bağlı) gösteriyor. Mobil API sadece `Drivers` kullanıyor.
3. **`DriverEarnings` boş**: Panelde menü var ama veri yok. Yolculuk tamamlanınca
   (`/api/Trips/{id}/confirm` veya `/complete`) bu tabloya `{DriverID, TripID, Amount, EarnedAt}`
   satırı yazılırsa panel "Sürücü kazançları" otomatik dolar ve mobil sürücü cüzdanı da
   `Trips` toplamı yerine buradan beslenebilir.
4. **Negatif sürücü ID'si**: `Messages.SenderID/ReceiverID` sürücüde negatif (örn. `-6`).
   Panel bunları `Users` tablosuna bağladığı için sürücü tarafı isim çözemez → panelde
   mesaj gönderen/alıcı boş görünebilir.
5. **`MatchingRequests` ve `DriverLocations` panelde RO**: yönetici düzeltemez; sadece izler.
   Bir sürücüyü elle "müsait/çevrimdışı" yapmak istenirse panelde uç yok
   (bizim `PUT /api/Matching/driver/{id}/availability` kullanılmalı).
6. **Panelde olmayan mobil davranışlar**: 30 dk konum tazeliği eşiği, arka plan servisi
   IsAvailable yarışı, kabul sonrası IsAvailable=0 — bunlar `driver-status-sorgulari.txt`
   dosyasında.
7. **Panel'in kendi API'si dokümante değil** (swagger 401). Yukarıdaki CRUD kalıbı
   ağ trafiğinden + `/api/panel/modules` şemasından çıkarıldı; `POST/PUT/DELETE/adjust`
   uçları gözlemlenmedi (veri değiştirmemek için), kalıptan tahmin edildi.
