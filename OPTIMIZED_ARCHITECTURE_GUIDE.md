# راهنمای جامع معماری بهینه (Optimized Modular Monolith)

این سند به عنوان مرجع فنی برای توسعه پروژه جدید بر پایهٔ معماری **Modular Monolith** با رویکرد **Vertical Slice** تدوین شده است. هدف این معماری، دستیابی به سرعت توسعه بالا، نگهداری آسان و قابلیت مقیاس‌پذیری (تبدیل آسان به میکروسرویس در آینده) است.

---

## ۱. اصول کلی و فلسفه معماری

1.  **Modular Monolith:** سیستم به صورت یکپارچه (Single Deployment) اجرا می‌شود اما کدها بر اساس دامنه‌های کسب‌وکار (Modules) کاملاً تفکیک شده‌اند.
2.  **Vertical Slices:** به جای لایه‌بندی افقی (Controller/Service/Repository)، کدها بر اساس "ویژگی‌ها" (Features) سازماندهی می‌شوند. هر ویژگی (مثلاً "ثبت سفارش") تمام کدهای مورد نیاز خود را (از API تا دیتابیس) در کنار هم دارد.
3.  **High Cohesion, Low Coupling:** کدهای مرتبط کنار هم هستند و وابستگی بین ماژول‌ها حداقل است.
4.  **REPR Pattern:** الگوی Request-Endpoint-Response برای طراحی APIها جایگزین الگوی سنتی MVC می‌شود.

---

## ۲. ساختار فیزیکی پروژه (Physical Structure)

برای کاهش سربار بیلد و پیچیدگی، تعداد پروژه‌ها (`.csproj`) را حداقل نگه می‌داریم.

```text
Solution/
├── src/
│   ├── Host/                   # نقطه ورود برنامه (WebAPI)
│   │   └── Host.csproj
│   │
│   ├── Modules/                # ماژول‌های بیزنس (هر ماژول فقط یک پروژه)
│   │   ├── Identity/
│   │   │   └── Identity.csproj
│   │   ├── Sales/
│   │   │   └── Sales.csproj
│   │   └── Inventory/
│   │       └── Inventory.csproj
│   │
│   └── Shared/                 # کدهای مشترک (Kernel/BuildingBlocks)
│       └── Shared.Kernel.csproj
│
└── tests/                      # تست‌ها
```

### قانون مهم:
هر ماژول (`Identity`, `Sales`, ...) یک پروژه مستقل است. **نباید** پروژه‌های جداگانه برای Domain/Application/Infrastructure بسازید. تفکیک لایه‌ها درون همان پروژه با استفاده از Namespace و پوشه‌بندی انجام می‌شود.

---

## ۳. ساختار داخلی هر ماژول (Internal Architecture)

هر ماژول باید ساختار زیر را داشته باشد:

```text
src/Modules/Sales/
├── Features/                   # قلب تپنده ماژول (Vertical Slices)
│   ├── CreateOrder/            # یک ویژگی خاص
│   │   ├── CreateOrderEndpoint.cs  # API Endpoint (FastEndpoints/MinimalAPI)
│   │   ├── CreateOrderCommand.cs   # Request DTO / Command
│   │   ├── CreateOrderHandler.cs   # Business Logic
│   │   ├── CreateOrderValidator.cs # Validation (FluentValidation)
│   │   └── CreateOrderSummary.cs   # Response DTO
│   │
│   └── GetOrderById/
│       └── ...
│
├── Domain/                     # مدل‌های غنی دامنه (Entities, ValueObjects)
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── Events/
│
├── Infrastructure/             # جزئیات پیاده‌سازی (Database, External APIs)
│   ├── Persistence/
│   │   ├── SalesDbContext.cs
│   │   └── Configurations/     # EF configurations
│   └── Gateways/               # پیاده‌سازی سرویس‌های خارجی
│
└── PublicApi/                  # قراردادهای عمومی برای سایر ماژول‌ها
    └── ISalesModuleApi.cs      # متدهای فقط-خواندنی برای استفاده سایر ماژول‌ها
```

---

## ۴. استراتژی ارتباط بین ماژول‌ها (Communication Strategy)

ارتباط مستقیم بین ماژول‌ها ممنوع است (Reference مستقیم به کلاس‌های داخلی ممنوع).

### الف) تغییر وضعیت (Writes / Side Effects) -> **Event-Driven**
برای عملیاتی که روی ماژول‌های دیگر اثر می‌گذارد، از **Domain Events** و **Integration Events** استفاده کنید.
*   **تکنیک:** الگوی **Outbox Pattern** الزامی است (برای تضمین اتمیک بودن تراکنش دیتابیس و ارسال پیام).
*   **مثال:** وقتی `OrderCreated` در ماژول `Sales` رخ می‌دهد، یک ایونت در صف (MassTransit/RabbitMQ/InMemory) منتشر می‌شود و ماژول `Inventory` آن را دریافت کرده و موجودی را کم می‌کند.

### ب) خواندن اطلاعات (Reads) -> **Direct Public Interface**
برای نیازهای خواندن (مثلاً "آیا این کاربر وجود دارد؟") ارسال ایونت سربار دارد.
*   **تکنیک:** هر ماژول یک `Inter-Module Interface` در پوشه `PublicApi` ارائه می‌دهد.
*   **قانون:** متدهای این اینترفیس باید **فقط خواندنی (Read-Only)** باشند و داده‌های ساده (DTO) برگردانند. هرگز Entity دامنه را برنگردانید.

---

## ۵. پشته تکنولوژی و کتابخانه‌ها (Tech Stack)

*   **Runtime:** .NET 9.0
*   **API Framework:** **FastEndpoints** (پیشنهادی) یا Minimal APIs. (پرفورمنس بالاتر و کد کمتر نسبت به Controllers).
*   **Data Access:**
    *   **Writes:** EF Core (با رویکرد Domain Model).
    *   **Reads:** Dapper یا EF Core با `AsNoTracking` و `ProjectTo`.
*   **Messaging:** MassTransit (برای مدیریت آسان RabbitMQ و Outbox).
*   **Validation:** FluentValidation.
*   **Mapping:** Mapster یا کدهای دستی (اجتناب از AutoMapper برای پرفورمنس).
*   **Testing:**
    *   **Architecture Tests:** NetArchTest (برای تضمین قوانین معماری در CI/CD).
    *   **Integration Tests:** Testcontainers.

---

## ۶. قوانین کدنویسی (Development Guidelines)

1.  **Rich Domain Model:** منطق بیزنس باید در Entityها باشد، نه در Service/Handler. از `private set` برای پراپرتی‌ها استفاده کنید.
2.  **Use Records:** برای DTOها، Commandها، Eventها و Value Objectها حتماً از `record` استفاده کنید.
3.  **Result Pattern:** برای مدیریت خطاها از پرتاب Exception خودداری کنید. از پترن Result (کتابخانه `ErrorOr` یا `FluentResults`) استفاده کنید.
4.  **CQRS:** تفکیک مدل‌های نوشتن و خواندن الزامی است. لایه نوشتن از Repository استفاده می‌کند؛ لایه خواندن می‌تواند مستقیم SQL بزند.
5.  **Sealed By Default:** تمام کلاس‌ها (مگر اینکه Abstract باشند) باید `sealed` باشند (برای پرفورمنس).

---

## ۷. فرآیند توسعه یک ویژگی جدید (Workflow)

1.  **تعریف Feature:** ایجاد پوشه در `Features/Name`.
2.  **Domain:** نوشتن Entity و Unit Testهای آن.
3.  **Handler:** پیاده‌سازی Use Case.
4.  **Endpoint:** اتصال به دنیای بیرون.
5.  **Integration Test:** نوشتن تست برای Endpoint.

---

این سند باید به عنوان "قانون اساسی" پروژه در نظر گرفته شود. هرگونه انحراف از این اصول باید با دلیل موجه و تایید تیم فنی باشد.

