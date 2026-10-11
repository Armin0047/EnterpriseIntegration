# EnterpriseIntegration API

وب‌سرویس یکپارچه‌سازی فاکتورها و گزارش‌های انبار. پروژه با ASP.NET Core 8 ساخته شده، برای اتصال به SQL Server از Dapper استفاده می‌کند و احراز هویت API را با JWT انجام می‌دهد.

## قابلیت‌ها

- دریافت توکن JWT برای کلاینت یکپارچه‌سازی
- دریافت فاکتورهای فروش و خرید با فیلتر تاریخ
- دریافت محصولات فاکتورها بر اساس نوع کالا
- گزارش موجودی و کالاهای راکد
- مستندات تعاملی API از طریق Swagger
- ثبت رخدادهای درخواست در لاگ برنامه

## پیش‌نیازها

- .NET SDK 8 برای توسعه و انتشار
- دسترسی شبکه‌ای از برنامه به SQL Server
- دیتابیس و جداول/رویه‌های ذخیره‌شده‌ای که کوئری‌های پروژه استفاده می‌کنند
- برای میزبانی در IIS، نصب ASP.NET Core Hosting Bundle متناسب با .NET 8

## تنظیمات

برنامه تنظیمات را از `appsettings.json` و منابع پیکربندی محیط اجرا می‌خواند. نام کلیدهای موردنیاز:

| کاربرد | کلید پیکربندی | نام متغیر محیطی |
|---|---|---|
| اتصال SQL Server | `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |
| کلید امضای JWT | `Jwt:Key` | `Jwt__Key` |
| صادرکنندهٔ JWT | `Jwt:Issuer` | `Jwt__Issuer` |
| مخاطب JWT | `Jwt:Audience` | `Jwt__Audience` |
| مدت اعتبار توکن برحسب دقیقه | `Jwt:ExpiryMinutes` | `Jwt__ExpiryMinutes` |
| هش SHA-256 رمز کلاینت (۶۴ رقم hex) | `PartnerAuth:ClientSecretHash` | `PartnerAuth__ClientSecretHash` |

مقادیر واقعی اتصال دیتابیس و کلید JWT را در مخزن کد یا فایل README قرار ندهید. برای IIS، آن‌ها را در تنظیمات امن همان سرور تعریف کنید. پس از تغییر تنظیمات، برنامه یا Application Pool را راه‌اندازی مجدد کنید.

فایل‌های واقعی `appsettings.json` و `appsettings.Development.json` به‌دلیل احتمال داشتن اطلاعات محرمانه در Git نادیده گرفته می‌شوند. برای اجرای محلی، نمونه را کپی و مقادیر آن را با تنظیمات محیط خود جایگزین کنید:

```powershell
Copy-Item .\EnterpriseIntegration\appsettings.example.json .\EnterpriseIntegration\appsettings.json
```

فایل کپی‌شده محلی است و وارد commit نمی‌شود. برای IIS، مقادیر واقعی را روی سرور تنظیم کنید؛ کلیدهای محیطی در جدول بالا آمده‌اند.

کنترلر احراز هویت در نسخهٔ فعلی، شناسهٔ کلاینت را بررسی می‌کند و رمز ارسالی را با هش SHA-256 تنظیم‌شده در `PartnerAuth:ClientSecretHash` مقایسه می‌کند. هش باید ۶۴ رقم hexadecimal باشد و از همان رمز کلاینت با UTF-8 و SHA-256 محاسبه شده باشد. رمز خام یا هش آن را در مخزن کد قرار ندهید؛ در IIS مقدار هش را به‌عنوان متغیر محیطی `PartnerAuth__ClientSecretHash` روی سرور تنظیم کنید.

## اجرا و انتشار

از پوشهٔ Solution (`EnterpriseIntegration`) اجرا کنید:

```powershell
dotnet restore .\EnterpriseIntegration.sln
dotnet run --project .\EnterpriseIntegration\EnterpriseIntegration.csproj
```

برای ساخت خروجی انتشار:

```powershell
dotnet publish .\EnterpriseIntegration\EnterpriseIntegration.csproj --configuration Release --output .\publish
```

پوشهٔ خروجی `publish` را روی سرور قرار دهید و IIS را طوری تنظیم کنید که سایت به همان پوشه اشاره کند. Application Pool برای برنامهٔ ASP.NET Core معمولاً با گزینهٔ **No Managed Code** پیکربندی می‌شود. برای دسترسی امن، روی سایت IIS گواهی TLS و HTTPS را تنظیم کنید.

پس از بالا آمدن برنامه:

- Swagger: `/swagger`
- مسیر ریشهٔ سایت `/` به `/swagger` هدایت می‌شود.

Swagger در تنظیمات فعلی در همهٔ محیط‌ها فعال است.

## احراز هویت

برای دریافت توکن، به `POST /api/Auth/token` درخواست JSON بفرستید:

```json
{
  "ClientId": "AriaSystem",
  "ClientSecret": "رمز کلاینت"
}
```

پاسخ موفق شامل `AccessToken`، `TokenType` و `ExpiresInSeconds` است. برای مسیرهای محافظت‌شده، توکن را در هدر زیر بفرستید:

```http
Authorization: Bearer <AccessToken>
```

کنترلرهای فاکتور و گزارش نیازمند JWT معتبر هستند. مسیر دریافت توکن برای صدور JWT عمومی است و خودش رمز کلاینت را بررسی می‌کند.

## مسیرهای API

تاریخ‌ها در API به شکل عدد ۸ رقمی شمسی `YYYYMMDD` ارسال می‌شوند؛ برای نمونه `14030115`.

### احراز هویت

| متد | مسیر | توضیح |
|---|---|---|
| `POST` | `/api/Auth/token` | دریافت توکن JWT |

### فاکتورها

مسیرهای این بخش نیازمند JWT هستند.

| متد | مسیر | پارامترها | توضیح |
|---|---|---|---|
| `GET` | `/api/Invoices` | `fromDate`, `toDate`, `pageNumber`, `pageSize` | فاکتورهای فروش در بازهٔ تاریخ |
| `GET` | `/api/Invoices/daily` | `date` | فاکتورهای فروش یک روز |
| `GET` | `/api/Invoices/ProductsType` | `fromDate`, `toDate`, `type`, `pageNumber`, `pageSize` | اقلام فاکتور بر اساس نوع کالا در بازه |
| `GET` | `/api/Invoices/ProductsType/daily` | `date`, `type` | اقلام یک روز بر اساس نوع کالا |
| `GET` | `/api/Invoices/Warehouse` | `fromDate`, `toDate`, `pageNumber`, `pageSize` | فاکتورهای خرید انبار در بازه |
| `GET` | `/api/Invoices/Warehouse/daily` | `date` | فاکتورهای خرید انبار برای یک روز |

در مسیرهای صفحه‌بندی‌شده، `pageNumber` پیش‌فرض ۱ و `pageSize` پیش‌فرض ۵۰ است؛ کنترلر حداکثر اندازهٔ صفحه را به ۲۰۰ محدود می‌کند. مسیرهای `daily` صفحه‌بندی دریافت نمی‌کنند و در حال حاضر اندازهٔ بسیار بزرگی به Repository می‌فرستند؛ در روزهای پرتراکنش ممکن است پاسخ حجیم شود.

نمونه:

```http
GET /api/Invoices?fromDate=14030101&toDate=14030131&pageNumber=1&pageSize=50
Authorization: Bearer <AccessToken>
```

پاسخ صفحه‌بندی‌شده شامل `TotalCount`، `PageNumber`، `PageSize`، `TotalPages` و `Data` است.

### گزارش‌ها

مسیرهای این بخش نیازمند JWT هستند.

| متد | مسیر | پارامترها | توضیح |
|---|---|---|---|
| `GET` | `/api/Reports/StockList` | `fromProductKey`, `toProductKey`, `fromWarehouse`, `toWarehouse` | گزارش موجودی در بازهٔ کد کالا و انبار |
| `GET` | `/api/Reports/Stagnantgoods` | `fromDate`, `toDate` | کالاهای راکد در بازهٔ تاریخ |

پارامترهای `StockList` اختیاری‌اند؛ کد کالا و انبار از صفر تا بیشینهٔ عدد صحیح در نظر گرفته می‌شوند، مگر اینکه بازهٔ محدودتری ارسال شود.

## لاگ‌ها

`AuditLoggingMiddleware` برای هر درخواست، کلاینت (در صورت وجود claim)، IP، روش و مسیر HTTP، کد وضعیت و زمان اجرا را با سطح `Information` ثبت می‌کند. تنظیم Event Log در appsettings فعال است؛ در ویندوز، لاگ‌ها را در **Event Viewer → Windows Logs → Application** بررسی کنید. لاگ پس از پایان درخواست ثبت می‌شود.

## ساختار کلی کد

- `Controllers`: مسیرهای HTTP و کنترل ورودی/خروجی
- `Core/Models`: مدل‌های درخواست و پاسخ
- `Core/Repositories`: قرارداد و پیاده‌سازی کوئری‌های Dapper
- `Infrastructure/Middleware`: ثبت رخداد و Middleware مربوط به idempotency

Middleware مربوط به idempotency در حال حاضر در pipeline غیرفعال است.
