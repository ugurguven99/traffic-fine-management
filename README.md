# Trafik Cezası Yönetim Modülü

**Traffic Fine Management Module** — Junior .NET Developer case çalışması, ASP.NET Core MVC / **.NET 9**.

Araçların ve bunlara ait trafik cezalarının kayıt altına alınmasını, cezaların **Manager → Finance** sırasıyla onaylanmasını veya gerekçeyle reddedilmesini sağlayan rol tabanlı web uygulaması. Onay işlemleri ve durum geçişleri kayıt altına alınır.

## Kullanılan Teknolojiler

- .NET 9, ASP.NET Core MVC, Razor Views
- Entity Framework Core 9 (Code First, migrations), SQL Server / SQL Server LocalDB
- ASP.NET Core Identity (`IdentityUser`) ve rol bazlı yetkilendirme
- Bootstrap 5, HTML, CSS ve JavaScript

## Özellikler

- Araç CRUD işlemleri; plaka normalizasyonu ve benzersiz plaka kontrolü
- Trafik cezası CRUD işlemleri ve durum bazlı işlem kısıtlamaları
- Admin tarafından onaya gönderme; Manager ve Finance tarafından aşamalı onay veya gerekçeli ret
- İşlemi gerçekleştiren kullanıcı, tarih, işlem ve durum geçişini saklayan `ApprovalHistory`
- Admin tarafından Manager / Finance rol atama
- Araç, ceza ve onay durumları için dashboard
- Araç ve ceza düzenleme/silme işlemlerinde SQL Server `rowversion` ile eş zamanlılık kontrolü
- İlişkili kayıtların yanlışlıkla silinmesini önlemeye yönelik kontroller

## Roller ve Yetkiler

| İşlem | Admin | Manager | Finance |
| --- | :---: | :---: | :---: |
| Araçları ve cezaları görüntüleme | ✅ | ✅ | ✅ |
| Araç/ceza oluşturma, düzenleme, silme (uygun durumlarda) | ✅ | ❌ | ❌ |
| Cezayı onaya gönderme | ✅ | ❌ | ❌ |
| Yönetici aşamasında onay / ret | ❌ | ✅ | ❌ |
| Finans aşamasında onay / ret | ❌ | ❌ | ✅ |
| Kullanıcılara Manager / Finance rolü atama | ✅ | ❌ | ❌ |

Yeni kayıt olan kullanıcıya otomatik olarak ayrıcalıklı rol atanmaz.

## Onay Akışı

```text
Yeni
  │ Admin: Onaya Gönder
  ▼
Yönetici Onayı ── Manager: Ret ─────────► Reddedildi
  │ Manager: Onay
  ▼
Finans Onayı ──── Finance: Ret ─────────► Reddedildi
  │ Finance: Onay
  ▼
Tamamlandı
```

Onay/ret kuralları `ApprovalService` içinde kontrol edilir. İşlem geçmişi, ilgili ceza güncellemesiyle birlikte EF Core üzerinden kaydedilir.

## Gereksinimler ve Eksikse Kurulum

> **En kolay test ortamı:** Windows 10/11 + Visual Studio 2022 + SQL Server Express LocalDB. Visual Studio kullanmak zorunlu değildir; .NET CLI ile de çalıştırılabilir.

| Gereksinim | Kurulum / kontrol |
| --- | --- |
| **.NET 9 SDK** | [Microsoft .NET 9 indirme sayfası](https://dotnet.microsoft.com/download/dotnet/9.0). Kontrol: `dotnet --list-sdks` (çıktıda `9.0.x` bulunmalı). Yalnızca .NET Runtime yeterli değildir. |
| **Git** | [Git for Windows](https://git-scm.com/downloads/win). Kontrol: `git --version`. Alternatif olarak GitHub üzerinden ZIP indirilebilir. |
| **Visual Studio (isteğe bağlı)** | [Visual Studio](https://visualstudio.microsoft.com/downloads/) 2022 **17.12 veya üzeri**, **ASP.NET and web development** iş yüküyle kurulabilir. Alternatif: VS Code veya terminal. |
| **SQL Server** | Windows için **SQL Server Express LocalDB** ([SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)) veya erişilebilir bir SQL Server örneği. Visual Studio Installer üzerinden **SQL Server Express LocalDB** bileşeni de kurulabilir. |
| **EF Core CLI aracı** | Migration uygulamak için `dotnet-ef` gerekir. Aşağıdaki adımlarda kurulum anlatılmıştır. |

**LocalDB notu:** LocalDB yalnızca Windows ortamında çalışır. macOS/Linux kullanılıyorsa bir SQL Server örneği (ör. ayrı sunucu/container) sağlanmalı ve bağlantı dizesi değiştirilmelidir. Veritabanını incelemek için SSMS isteğe bağlıdır; uygulamayı çalıştırmak için zorunlu değildir.

## Sıfırdan Kurulum (Windows / PowerShell)

Komutlar, `Traffic.csproj` ve `Traffic.sln` dosyalarının bulunduğu **depo kök klasöründe** çalıştırılmalıdır.

### 1. Kaynak kodunu indirin

```powershell
git clone https://github.com/ugurguven99/traffic-fine-management.git
cd traffic-fine-management
```

Depo **Private** durumdaysa klonlamak için GitHub hesabınıza bu depoya erişim yetkisi verilmiş olmalıdır. ZIP ile indirdiyseniz arşivi açıp proje klasöründe terminal başlatın.

### 2. SDK ve SQL Server durumunu kontrol edin

```powershell
dotnet --list-sdks
git --version
```

`dotnet --list-sdks` çıktısında `9.0.x` yoksa yukarıdaki bağlantıdan **.NET 9 SDK** kurun ve terminali yeniden açın.

LocalDB tercih ediyorsanız şu komutu deneyin:

```powershell
sqllocaldb info MSSQLLocalDB
```

`MSSQLLocalDB` bulunamıyorsa LocalDB bileşenini yükleyin; gerekirse `sqllocaldb create MSSQLLocalDB` ve `sqllocaldb start MSSQLLocalDB` komutlarıyla örneği oluşturup başlatın. `sqllocaldb` komutunun bulunmaması LocalDB'nin yüklü olmadığını veya araç yolunun bulunamadığını gösterebilir.

### 3. Veritabanı bağlantısını ayarlayın

Uygulama `ConnectionStrings:DefaultConnection` anahtarını kullanır. Windows LocalDB için örnek bağlantı:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TrafficDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Bu örnek `appsettings.json` dosyasındaki bağlantı ayarını gösterir; **diğer ayarları silmeden** kendi ortamınıza uyarlayın. SQL Server/Express kullanıyorsanız `Server` değerini kendi örneğinize göre değiştirin.

Özel bağlantı bilgileri veya parola kullanıyorsanız bunları Git'e yazmak yerine **User Secrets** ile yapılandırın:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=TrafficDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

`UserSecretsId` proje dosyasında tanımlıdır. User Secrets geliştirme ortamı için uygundur, üretim ortamı için gizli bilgi yönetimi yerine geçmez.

### 4. NuGet paketlerini yükleyin ve derleyin

```powershell
dotnet restore
dotnet build
```

Her iki komutun da hatasız tamamlanması beklenir. İlk kullanımda NuGet paketlerinin indirilebilmesi için internet bağlantısı gerekir.

### 5. EF Core komut satırı aracını hazırlayın

Önce kurulu olup olmadığını kontrol edin:

```powershell
dotnet ef --version
```

Komut bulunamazsa proje paketleriyle uyumlu **9.x** sürümünü yükleyin:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.20
```

Araç zaten kuruluysa ve başka bir sürüm kullanıyorsa:

```powershell
dotnet tool update --global dotnet-ef --version 9.0.20
```

Gerekirse terminali yeniden başlatın. EF CLI'nin **9.x** olması önerilir.

### 6. Veritabanını migration'larla oluşturun

```powershell
dotnet ef database update --context ApplicationDbContext
```

Projede birden fazla `DbContext` algılanabildiği için `--context ApplicationDbContext` parametresi **bilerek** kullanılır. Bu komut, SQL Server bağlantısına erişilebiliyorsa mevcut migration'ları uygulayarak gerekli tabloları oluşturur/günceller.

> Bu aşamada `Add-Migration` çalıştırmak veya migration dosyalarını yeniden üretmek gerekmez. Migration'lar kaynak kodla birlikte depodadır.

### 7. Uygulamayı Development ortamında başlatın

PowerShell'de:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run
```

Terminalde görüntülenen `http://localhost:...` veya `https://localhost:...` adresini tarayıcıda açın. HTTPS geliştirme sertifikası uyarısı alınırsa aşağıdaki komut yardımcı olabilir:

```powershell
dotnet dev-certs https --trust
```

Development ortamında `Admin`, `Manager` ve `Finance` rolleri `IdentitySeeder.SeedRolesAsync` aracılığıyla oluşturulur. **Rollerin oluşması, otomatik bir Admin hesabı oluşturulduğu anlamına gelmez.**

## İlk Admin Kullanıcısını Hazırlama

> Bu bölüm, `Program.cs` içindeki Development bloğunda **hem** `IdentitySeeder.SeedRolesAsync(...)` **hem de** `IdentitySeeder.SeedFirstAdminAsync(scope.ServiceProvider, app.Configuration)` çağrıları bulunduğu sürüm için geçerlidir.

1. Uygulamayı başlatın ve **Kayıt Ol** ekranından yeni bir kullanıcı hesabı oluşturun. Şifre en az 8 karakter içermeli; büyük harf, küçük harf, rakam ve özel karakter bulunmalıdır.

2. Yeni oluşturulan kullanıcıya henüz herhangi bir rol atanmadığından, yönetim ekranlarına erişmeye çalıştığınızda **Access Denied (Erişim Engellendi)** uyarısıyla karşılaşabilirsiniz. Bu durum normaldir. İlk Admin yetkisini tanımlamak için uygulamayı durdurun.

3. Proje kök dizininde bir terminal açın ve oluşturduğunuz kullanıcının e-posta adresini User Secrets'a kaydedin:

   ```powershell
   dotnet user-secrets set "BootstrapAdmin:Email" "admin@example.com"
   ```

   **Önemli:** `admin@example.com` yalnızca örnek bir adrestir. Bu değeri, 1. adımda kayıt olduğunuz e-posta adresiyle değiştirin.
4. Uygulamayı tekrar **Development** ortamında başlatın. `SeedFirstAdminAsync`, veritabanında zaten bulunan ilgili kullanıcıya `Admin` rolünü atar. **Kullanıcıyı kendisi oluşturmaz.**
5. Uygulama yeniden başladıktan sonra Admin hesabıyla giriş yapın ve **Kullanıcı Yönetimi** ekranına erişebildiğinizi doğrulayın.

6. İlk Admin yetkisinin başarıyla atandığını doğruladıktan sonra geçici yapılandırmayı kaldırın:

   ```powershell
   dotnet user-secrets remove "BootstrapAdmin:Email"
   ```

   İsterseniz `dotnet user-secrets list` komutuyla ayarın kaldırıldığını kontrol edebilirsiniz.

   Bu işlem Admin hesabını veya kullanıcının rolünü silmez. Yalnızca ilk kurulum sırasında kullanılan yapılandırma ayarını kaldırır.

7. Diğer kullanıcıları **Kayıt Ol** ekranından oluşturun. Admin hesabıyla **Kullanıcı Yönetimi** sayfasından `Manager` ve `Finance` rollerini atayın.

## Örnek Test Senaryosu

1. Admin ile oturum açın, araç oluşturun ve trafik cezası ekleyin.
2. Admin olarak cezayı onaya gönderin.
3. Manager hesabıyla giriş yapıp cezayı onaylayın.
4. Finance hesabıyla giriş yapıp cezayı tamamlayın. Her adımda durum ve `ApprovalHistory` kayıtlarını kontrol edin.
5. Yeni bir cezada Manager veya Finance aşamasında gerekçeli ret deneyin.
6. Aynı aracın veya cezanın düzenleme ekranını iki sekmede açın. İlkinde kaydedin; eski sürümle diğer sekmeden kayıt yapmanın engellendiğini doğrulayın.
7. Trafik cezası bağlı aracı silmeyi deneyin ve engellendiğini doğrulayın.
8. Dashboard istatistiklerini kontrol edin.

## Mimari ve Teknik Kararlar

- **Models:** `Vehicle`, `TrafficFine`, `ApprovalHistory` ve ilgili enum'lar.
- **Data:** `ApplicationDbContext`, EF Core migration'ları, `IdentitySeeder`.
- **Controllers:** MVC istekleri ve rol bazlı işlem erişimi.
- **Services:** `ApprovalService` içinde onay/ret iş kuralları ve durum geçişleri.
- **Views / Areas/Identity:** Razor arayüzleri ve Identity giriş/kayıt ekranları.
- **Veri tutarlılığı:** Plaka için benzersiz veritabanı indeksi; EF Core optimistic concurrency (`RowVersion`); ilişkili kayıtların silinmesini kısıtlayan kontroller.
- **İzlenebilirlik:** Onay ve ret geçmişi `ApprovalHistory` aracılığıyla tutulur.

## Sık Karşılaşılan Kurulum Sorunları

| Sorun | Kontrol / Çözüm |
| --- | --- |
| `NETSDK1045`, `net9.0` desteklenmiyor | .NET **9 SDK** yükleyin; `dotnet --list-sdks` çıktısını ve varsa `global.json` ayarını kontrol edin. Visual Studio için 17.12+ gerekir. |
| `dotnet` komutu bulunamıyor | .NET SDK kurulumunu tamamlayıp terminali yeniden başlatın. |
| `dotnet ef` bulunamıyor | Yukarıdaki EF CLI kurulum adımını uygulayın. |
| `More than one DbContext was found` | Komutta `--context ApplicationDbContext` kullanın. |
| SQL Server bağlantı hatası | SQL Server/LocalDB'nin kurulu ve erişilebilir olduğunu, `DefaultConnection` ayarını ve örnek adını kontrol edin. |
| `Login failed` veya veritabanı erişim hatası | SQL Server kimlik doğrulaması, kullanıcı yetkileri ve connection string değerlerini kontrol edin. |
| Admin menüsü görünmüyor | Önce kayıt oluşturun; `BootstrapAdmin:Email` değeri ile kayıtlı e-postanın aynı olduğunu ve Development ortamında çalıştığınızı kontrol edin. Çıkış/giriş yapın. |
| HTTPS geliştirme sertifikası uyarısı | `dotnet dev-certs https --trust` komutunu çalıştırın veya terminalin gösterdiği uygun yerel adresi kullanın. |

## Geliştirme ve Güvenlik Notları

- Yetkilendirme yalnızca menülerin gizlenmesiyle sınırlı değildir; Controller ve servis katmanında da uygulanır.
- Kullanıcı parolaları ve bağlantı gizli bilgileri README'ye veya Git'e eklenmemelidir.
- `bin/`, `obj/`, `.vs/`, `*.user`, yerel veritabanı dosyaları ve User Secrets içeriği Git dışında tutulmalıdır.
- `appsettings.json` içindeki gerçek bağlantı bilgileri yayımlanmadan önce kontrol edilmelidir.
- Bu çalışma bir **teknik değerlendirme case'i** için hazırlanmıştır; üretime alınmadan önce kullanıcı kaydı, ilk yönetici kurulumu, gizli bilgi yönetimi ve dağıtım politikaları yeniden değerlendirilmelidir.
