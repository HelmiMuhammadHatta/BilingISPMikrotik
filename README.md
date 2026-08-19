# 🌐 Billing ISP Mikrotik

Sistem otomatisasi tagihan (Billing) terpadu untuk Internet Service Provider (ISP) yang terintegrasi langsung dengan Router Mikrotik. Dibangun menggunakan arsitektur modern untuk menjamin skalabilitas, keamanan, serta kemudahan dalam manajemen operasional pelanggan.

## 🚀 Fitur Utama

- **Customer & Service Plan Management**: Kelola data pelanggan, profil PPP, beserta layanan paket internet (_Service Plan_) yang mereka gunakan.
- **Auto-Generate Invoices**: _Background job_ (menggunakan Hangfire) otomatis menghasilkan tagihan bulanan pada tanggal 1 setiap bulannya.
- **Auto-Isolir (Mikrotik Integration)**: Mengecek tagihan yang sudah _overdue_. Jika ada, sistem akan otomatis melakukan `SetPppProfile` ke profil isolir dan memutuskan sesi pelanggan melalui RouterOS API.
- **Auto-Restore via Payment Gateway**: Terintegrasi menggunakan Webhook dengan Payment Gateway (seperti **Midtrans**). Begitu pelanggan membayar via VA/QRIS, webhook memvalidasi _signature_, melunasi tagihan, dan mengembalikan profil Mikrotik pelanggan ke kondisi normal.
- **Admin Dashboard (Next.js)**: Antarmuka cantik bergaya modern menggunakan TailwindCSS & Recharts. Mendukung pemantauan metrik *revenue*, ekspor tagihan ke Excel (.xlsx), dan pelacakan log aktivitas (_Audit Trails_).

## 🏗️ Arsitektur Sistem

Proyek ini dipisah menjadi dua bagian utama:
1. **Backend (ASP.NET Core 9.0)** - _Clean Architecture_ (Domain, Application, Infrastructure, API).
   - *ORM*: Entity Framework Core (PostgreSQL).
   - *Background Jobs*: Hangfire.
   - *API Documentation*: RESTful dengan Swagger OpenAPI.
2. **Frontend (Next.js)** - Dashboard interaktif (_Client-Side & Server-Side Rendering_).
   - *Styling*: Tailwind CSS.
   - *Icons & Charts*: Lucide React & Recharts.

---

## 🛠️ Prasyarat

Pastikan Anda telah menginstal _tools_ berikut sebelum menjalankan sistem ini:
1. **.NET 9.0 SDK** (untuk backend API).
2. **Node.js (v18+) & npm** (untuk frontend Next.js).
3. **PostgreSQL** (sebagai database utama).
4. **Router Mikrotik** yang memiliki akses API via port (default: `8728`).

---

## ⚙️ Cara Menjalankan Aplikasi

### 1. Backend (API)
Buka terminal dan arahkan ke dalam direktori backend:
```bash
cd BillingISPMikrotik.API

# Lakukan restore package
dotnet restore

# Sesuaikan koneksi Database di appsettings.json
# (Default Connection: Host=localhost;Port=5432;Database=BillingISP;Username=postgres;Password=20)

# Update skema database (Migrations)
dotnet ef database update --project ../BillingISPMikrotik.Infrastructure --startup-project .

# Jalankan server
dotnet run
```
> 💡 **Tip:** Swagger UI dapat diakses di `http://localhost:5233/swagger`

### 2. Frontend (Admin Dashboard)
Buka tab terminal baru dan arahkan ke direktori frontend:
```bash
cd frontend-admin

# Instal dependensi
npm install

# Jalankan development server
npm run dev
```
> 💡 **Tip:** Dashboard admin dapat diakses di `http://localhost:3000`

---

## 🔑 Autentikasi API & Dashboard (JWT)

API dan Dashboard dilindungi oleh sistem Autentikasi JWT.
- **Username Default:** `admin`
- **Password Default:** `password123`

Untuk mencoba API melalui Swagger:
1. Panggil *endpoint* `POST /api/Auth/login` dengan kredensial di atas untuk mendapatkan `token`.
2. Klik tombol **Authorize** di pojok kanan atas layar Swagger.
3. Masukkan token dengan format: `Bearer <token_anda>`.

---

## 🔒 Konfigurasi Webhook (Midtrans)

Di dalam file `appsettings.json` (backend API), pastikan Anda mengubah `ServerKey` dengan kunci server yang Anda dapatkan dari Dashboard Midtrans:

```json
"PaymentGateway": {
  "ServerKey": "SB-Mid-server-KUNCI_RAHASIA_ANDA"
}
```
**Webhook URL** yang harus didaftarkan di pengaturan Dashboard Midtrans adalah:
`https://domain-api-anda.com/api/webhooks/payment-gateway`

---

## 🧪 Pengujian Otomatis (E2E Integration Testing)

Aplikasi ini dilengkapi dengan _End-to-End Test Suite_ menggunakan **xUnit** dan **Moq**. Tes ini memastikan fitur krusial (seperti *Invoice Generation*, *Auto-Isolir*, dan *Payment Auto Restore*) berjalan dengan sempurna.

Untuk menjalankan pengujian:
```bash
cd BillingISPMikrotik.Application.Tests
dotnet test
```

---

## 📄 Lisensi
Proyek ini dibuat sebagai fondasi manajemen ISP mandiri. Silakan modifikasi dan kembangkan sesuai dengan kebutuhan bisnis penyedia layanan internet Anda!
