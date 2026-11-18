# CrossApp
Наскрізний проєкт з крос-платформного програмування.

Предметна область: Склад. Сутності: Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення).
Призначення: облік залишків товарів по партіях.

## Середовище
.NET SDK 10.0.401, Windows 11 x64

## Структура solution

```
CrossApp/
├── CrossApp.slnx
├── README.md
├── .gitignore
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   └── EnvironmentInfo.cs
    └── Cli/
        ├── Cli.csproj      (ProjectReference на Core)
        └── Program.cs
```

Залежність одностороння: **Cli → Core**. Уся інформація про середовище збирається в Core (`EnvironmentInfo.Collect()` повертає `EnvironmentReport`), а Cli лише форматує вивід: звичайний текст або JSON (`--json`).

## Команди

```powershell
dotnet build
dotnet run --project src/Cli
dotnet run --project src/Cli -- --json
dotnet publish src/Cli -c Release -r win-x64 --self-contained true  -o publish/sc
dotnet publish src/Cli -c Release -r win-x64 --self-contained false -o publish/fd
```

## Порівняння режимів публікації

| RID | Режим | Розмір publish | Потрібен runtime |
|---|---|---|---|
| win-x64 | self-contained | 76,9 МБ | ні |
| win-x64 | framework-dependent | 0,2 МБ | так (.NET 10) |

**Self-contained** публікація містить код застосунку разом із .NET Runtime, тому працює на машині без встановленого .NET, але велика й прив'язана до конкретної RID. **Framework-dependent** містить лише код і залежності, тому дуже мала, але на машині користувача має бути встановлений сумісний .NET Runtime.

## Multi-targeting

Core збирається лише під net10.0 (multi-targeting net8.0;net10.0 не вдався: <причина>, SDK 10.0.401).

## Плановані каталоги в Core

- `Core/Dto/` — record-типи формату даних (тиждень 3)
- `Core/Domain/` — сутності з поведінкою та інваріантами (тиждень 4)
- `Core/Storage/` — реалізації сховищ (тиждень 5)

Порожні каталоги git не зберігає, тому кожен буде створений разом з першим типом.

## Лабораторна робота 4: Доменна модель, інваріанти, інкапсуляція

### Перелік реалізованих інваріантів:
1. **Обов'язковість ключових та текстових полів (`Id`, `Sku`, `Name`)**:
   - Правило: рядок не може бути порожнім, складеним лише з пробілів або `null`.
   - Метод перевірки: `Product.Create`
   - Тип винятку: `ArgumentException`

2. **Коректність числових параметрів (`Quantity`, `amount`)**:
   - Правило: початковий залишок при створенні має бути невід'ємним (`quantity >= 0`); кількість товару при приході та видачі має бути строго більшою за нуль (`amount > 0`).
   - Методи перевірки: `Product.Create`, `Product.RegisterArrival`, `Product.Issue`
   - Тип винятку: `ArgumentOutOfRangeException`

3. **Неможливість перевитрати залишку товару (інваріант стану)**:
   - Правило: кількість товару, що списується, не може перевищувати поточний залишок на складі (`amount <= _quantity`).
   - Метод перевірки: `Product.Issue`
   - Тип винятку: `InvalidOperationException`

   Пояснення відкладеного виконання LINQ (Крок 3 та 6):

Оператор Where використовує механізм відкладеного виконання (deferred execution) і в момент створення змінної query не виконує фільтрацію джерела, а лише будує ітератор. Фактичне виконання запиту ініціюється викликом термінального методу Count(), який обходить колекцію щоразу заново. Через це перший виклик query.Count() повернув 5 (початкові елементи), а після мутації списку методом Add другий виклик Count() заново проітерував змінене джерело й видав уже 6. Натомість виклик ToList() одразу матеріалізував дані в незалежний список-знімок snapshot у пам'яті, тому подальше очищення source.Clear() уже ніяк не вплинуло на snapshot.Count.

Порівняння синтаксису методів (Method Syntax) та синтаксису запитів (Query Syntax):

Синтаксис запитів (from ... group ... into ...):

Нагадує класичний SQL, завдяки чому складні групування та багаторівневі join сприймаються логічно для розробників, знайомих з реляційними базами.

Дозволяє зручно оголошувати проміжні змінні за допомогою ключового слова let (наприклад, щоб двічі не обчислювати Sum).

Синтаксис методів (ланцюжок розширень):

Є природним для об'єктно-орієнтованого коду C# і Fluent API.

Набагато компактніший у простих фільтраціях та проєкціях (Where, Select, Take, OrderBy).

Легше дозволяє підставляти настроювані компаратори (наприклад, StringComparer.OrdinalIgnoreCase), що у синтаксисі запитів вимагає додаткових конструкцій.

Висновок: Для стандартних конвеєрів звітів синтаксис методів є більш гнучким і лаконічним у C#, тоді як Query Syntax виграє у читабельності при множинних join та агрегаціях з group into.

## Конфігурація та змінні середовища

Застосунок завантажує налаштування у такому порядку пріоритету (кожне наступне джерело перекриває попереднє):
1. `appsettings.json` (базові налаштування)
2. `appsettings.{CROSSAPP_ENVIRONMENT}.json` (профіль середовища: `Development` або `Production`)
3. `User Secrets` (локальні секрети розробника поза репозиторієм)
4. Змінні середовища з префіксом `CROSSAPP_` та `CROSSAPP_DATA_PATH`
5. Аргументи командного рядка (`--Storage:DataPath=...`)

### Профілі запуску
* **`Development`** (за замовчуванням): рівень логування `Debug`, файл даних `items.dev.json`.
* **`Production`**: рівень логування `Warning`, файл даних `items.json`.

### Таблиця ключів конфігурації

| Ключ у `appsettings.json` | Змінна середовища | Опис | Значення за замовчуванням |
| :--- | :--- | :--- | :--- |
| `EnvironmentName` | `CROSSAPP_ENVIRONMENT` | Активний профіль середовища | `"Development"` |
| `Logging:MinimumLevel` | `CROSSAPP_Logging__MinimumLevel` | Мінімальний рівень логування | `"Warning"` |
| `Storage:DataPath` | `CROSSAPP_Storage__DataPath` або `CROSSAPP_DATA_PATH` | Каталог збереження даних | `"data"` |
| `Storage:FileName` | `CROSSAPP_Storage__FileName` | Ім'я JSON-файлу каталогу | `"items.json"` |
| `Warehouse:DefaultUnit` | `CROSSAPP_Warehouse__DefaultUnit` | Одиниця виміру за замовчуванням | `"шт"` |
| `Integration:ApiKey` | `CROSSAPP_Integration__ApiKey` *(або `user-secrets`)* | API-ключ інтеграції (секрет) | *не задано* |

### Приклади перевизначення змінних

**PowerShell (Windows):**
```powershell
$env:CROSSAPP_ENVIRONMENT = "Production"
$env:CROSSAPP_DATA_PATH = "C:\Temp\CrossAppData"
$env:CROSSAPP_Integration__ApiKey = "prod-secret-key"
dotnet run --project src/Cli -- export