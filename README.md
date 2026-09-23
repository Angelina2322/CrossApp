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