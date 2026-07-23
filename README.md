# Billing ISP Mikrotik

Sistem otomatisasi tagihan (Billing) terpadu untuk Internet Service Provider (ISP) yang terintegrasi langsung dengan _Router_ Mikrotik. Dibangun menggunakan arsitektur modern untuk menjamin skalabilitas, skalabilitas keamanan, serta kemudahan dalam manajemen pelanggan.

## 🚀 Fitur Utama

- **Customer & Service Plan Management**: Kelola data pelanggan, profil PPP, beserta layanan paket internet (_Service Plan_) yang mereka gunakan.
- **Auto-Generate Invoices**: _Background job_ (menggunakan Hangfire) otomatis menghasilkan tagihan bulanan pada tanggal 1 setiap bulannya.
- **Auto-Isolir (Mikrotik Integration)**: Mengecek tagihan yang sudah _overdue_. Jika ada, sistem akan otomatis melakukan `SetPppProfile` ke profil isolir dan memutuskan sesi (kick session) pelanggan melalui RouterOS API.
- **Auto-Restore via Payment Gateway**: Terintegrasi menggunakan Webhook dengan Payment Gateway (seperti **Midtrans**). Begitu pelanggan membayar via VA/QRIS, webhook memvalidasi _signature_, melunasi tagihan, dan mengembalikan profil Mikrotik pelanggan ke kondisi normal.
- **Admin Dashboard (Next.js)**: Antarmuka cantik bergaya modern menggunakan TailwindCSS & Recharts. Mendukung pemantauan metrik *revenue*, export tagihan ke Excel (.xlsx), dan pelacakan log aktivitas (_Audit Trails_).

## 🏗️ Arsitektur Sistem

Proyek ini dipisah menjadi dua bagian utama yang ter-decoupling:
1. **Backend (ASP.NET Core 9.0)** - Clean Architecture (Domain, Application, Infrastructure, API).
   - *ORM*: Entity Framework Core (PostgreSQL).
   - *Background Jobs*: Hangfire.
   - *API*: RESTful dengan Swagger OpenAPI.
2. **Frontend (Next.js 16)** - Dashboard interaktif (_Client-Side & Server-Side Rendering_).
   - *Styling*: Tailwind CSS.
   - *Icons & Charts*: Lucide React & Recharts.

---

## 🛠️ Prasyarat

Pastikan Anda telah menginstal _tools_ berikut sebelum menjalankan sistem ini:
1. **.NET 9.0 SDK** (untuk backend API).
2. **Node.js (v18+) & npm** (untuk frontend Next.js).
3. **PostgreSQL** (sebagai database utama).
4. _Router_ Mikrotik yang memiliki akses API via port (default: 8728).

---

## ⚙️ Cara Menjalankan Aplikasi

### 1. Backend (API)
Arahkan terminal ke dalam folder `BillingISPMikrotik.API`.
```bash
# Lakukan restore package
dotnet restore

# Sesuaikan koneksi Database di appsettings.json
# Default: Host=localhost;Port=5432;Database=BillingISP;Username=postgres;Password=20;

# Update skema database (Migrations)
dotnet ef database update --project ../BillingISPMikrotik.Infrastructure --startup-project .

# Jalankan server
dotnet run
```
_Swagger UI dapat diakses di `http://localhost:5233/swagger`_

### 2. Frontend (Admin Dashboard)
Arahkan terminal baru ke dalam folder `frontend-admin`.
```bash
# Instal dependensi
npm install

# Jalankan development server
npm run dev
```
_Dashboard admin dapat diakses di `http://localhost:3000`_

---

## 🔑 JWT Authentication (Login Admin)

API dan Dashboard dilindungi oleh sistem Autentikasi JWT.
- **Username Default:** `admin`
- **Password Default:** `password123`

Untuk mencoba API langsung melalui Swagger, panggil *endpoint* `/api/Auth/login` untuk mendapatkan `token`, lalu masukkan ke tombol **Authorize** di pojok kanan atas Swagger dengan format: `Bearer <token_anda>`.

---

## 🔒 Konfigurasi Webhook Midtrans

Di dalam file `appsettings.json` (backend API), pastikan Anda mengubah `ServerKey` bawaan dengan kunci server asli yang Anda dapatkan dari Dashboard Midtrans:

```json
"PaymentGateway": {
  "ServerKey": "SB-Mid-server-KUNCI_RAHASIA_ANDA"
}
```
Webhook URL yang harus didaftarkan di Dashboard Midtrans adalah:
`https://domain-api-anda.com/api/webhooks/payment-gateway`

---

## 🧪 E2E Integration Testing

Aplikasi ini sudah dilengkapi dengan _End-to-End Test Suite_ (ditulis dengan **xUnit** dan **Moq**).
Untuk memastikan bahwa logika bisnis utama (termasuk *Invoice Generation*, *Auto-Isolir*, dan *Payment Auto Restore*) berjalan dengan aman:
1. Pindah ke direktori pengujian:
   ```bash
   cd C:\BilingISPMikrotik\BillingISPMikrotik.Application.Tests
   ```
2. Jalankan tes otomatis:
   ```bash
   dotnet test
   ```

---
## 📄 Lisensi
Proyek ini dibuat sebagai fondasi manajemen ISP yang mandiri. Silakan modifikasi sesuai dengan kebutuhan bisnis penyedia layanan internet Anda!
