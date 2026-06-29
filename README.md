# NSI2000

NSI2000 COM arayüzü üzerinden ölçüm dosyalarını yönetmek için .NET 10 C# konsol uygulaması.

## Gereksinimler

- Windows x64
- .NET 10 SDK
- NSI2000 yazılımı kurulu ve COM sunucusu kayıtlı (`NSI2000.server`)
- VSCode + C# Dev Kit eklentisi

## Derleme

```bash
dotnet build
```

## Çalıştırma

```bash
dotnet run
```

## Yayımlama (tek klasör)

```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

## Proje Yapısı

```
NSI2000/
├── .vscode/
│   ├── launch.json     # F5 debug yapılandırması
│   ├── settings.json   # VSCode ayarları
│   └── tasks.json      # Build / publish görevleri
├── src/
│   ├── Measurement.cs  # NSI2000 COM wrapper sınıfı
│   └── Program.cs      # Giriş noktası
├── .gitignore
├── NSI2000.csproj
└── README.md
```
