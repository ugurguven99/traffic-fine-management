# Trafik Cezası Yönetim Modülü

**Traffic Fine Management Module** — ASP.NET Core MVC / .NET 9 case çalışması.

## Proje hakkında

Bu uygulama, araçların ve bu araçlara ait trafik cezalarının yönetilmesini; cezaların yetkili kullanıcılar tarafından sıralı olarak değerlendirilmesini sağlar. Yönetim ekranları, rol bazlı yetkilendirme ve işlem geçmişi içerir.

## Teknolojiler

- .NET 9, ASP.NET Core MVC, Razor Views
- Entity Framework Core 9 ve SQL Server (LocalDB)
- ASP.NET Core Identity (kimlik doğrulama ve rol yönetimi)
- Bootstrap 5

## Özellikler

- Araç ekleme, listeleme, detay, düzenleme ve silme
- Plaka normalizasyonu ve benzersiz plaka kontrolü
- Trafik cezası oluşturma, düzenleme, listeleme ve detay ekranları
- **Admin → Manager → Finance** aşamalarını içeren onay akışı
- Yetkili kullanıcılar tarafından gerekçeli ret
- Onay ve ret işlemlerinin kullanıcı, zaman ve durum geçişiyle kaydedilmesi
- Kullanıcıların rollerinin Admin tarafından yönetilmesi
- Dashboard üzerinde araç, ceza ve onay durumu özetleri
- Trafik cezası ve araç düzenleme/silme işlemlerinde **RowVersion** tabanlı eş zamanlılık kontrolü
- İlişkili kayıtların silinmesine karşı veri bütünlüğü kontrolleri

## Roller ve yetkiler

| İşlem | Admin | Manager | Finance |
| --- | :---: | :---: | :---: |
| Araçları ve cezaları görüntüleme | ✅ | ✅ | ✅ |
| Araç ve ceza oluşturma/düzenleme/silme | ✅ | ❌ | ❌ |
| Cezayı onaya gönderme | ✅ | ❌ | ❌ |
| Yönetici aşamasında onay/ret | ❌ | ✅ | ❌ |
| Finans aşamasında onay/ret | ❌ | ❌ | ✅ |
| Kullanıcı rollerini yönetme | ✅ | ❌ | ❌ |

## Ceza iş akışı

```text
Yeni → Yönetici Onayı → Finans Onayı → Tamamlandı
                     ↘ Reddedildi ↙
```

Onay ve ret işlemleri rol ve mevcut durum doğrulamalarından geçer. İşlemler bir **ApprovalHistory** kaydıyla birlikte veritabanına yazılır.

## Gereksinimler

- .NET 9 SDK
- .NET 9 destekleyen Visual Studio 2022 veya başka bir geliştirme ortamı
- SQL Server LocalDB veya SQL Server

## Kurulum ve çalıştırma

1. Depoyu klonlayın:

   ```bash
   git clone https://github.com/ugurguven99/traffic-fine-management.git
   cd traffic-fine-management
   ```

2. `appsettings.json` / User Secrets üzerinden `ConnectionStrings:DefaultConnection` bağlantısını kendi SQL Server ortamınıza göre yapılandırın. **Gerçek bağlantı parolalarını veya gizli bilgileri Git'e eklemeyin.**
3. Bağımlılıkları yükleyin:

   ```bash
   dotnet restore
   ```

4. Veritabanı migration'larını uygulayın (proje dosyasının bulunduğu klasörde):

   ```bash
   dotnet ef database update --context ApplicationDbContext
   ```

   `dotnet ef` komutu kurulu değilse öncelikle EF Core 9 ile uyumlu `dotnet-ef` aracını kurun.

5. Uygulamayı başlatın:

   ```bash
   dotnet run
   ```

6. Terminalde gösterilen yerel adresten uygulamayı açın.

> **İlk Admin hesabı:** Bu projede Admin rolünün ilk hesaba hangi yöntemle atandığı, `Program.cs` ve geliştirme ortamı seed yapılandırması doğrulanarak burada açıkça belgelenmelidir. Bu adıma gerçek parola veya kişisel hesap bilgisi yazılmamalıdır.

## Test senaryoları

- Admin, Manager ve Finance rollerinin yetki sınırları
- Araç/ceza ekleme, güncelleme ve silme işlemleri
- Aynı plakanın tekrar eklenmesinin engellenmesi
- Manager ve Finance onay/ret akışı
- Onay geçmişinin doğru kaydedilmesi
- İki farklı sekmeden eş zamanlı güncellemede eski verinin reddedilmesi (`RowVersion`)
- Trafik cezası bağlı bir aracın silinmesinin engellenmesi
- Dashboard verilerinin güncellenmesi

## Geliştirme ve güvenlik notları

- Kimlik doğrulama ve rol yönetimi ASP.NET Core Identity ile yapılır.
- Yetkiler yalnızca arayüzde değil Controller ve servis katmanında da kontrol edilir.
- SQL Server `rowversion`, güncelleme/silme çakışmalarını algılamak için kullanılır.
- Register ekranı case testleri için açıktır; yeni kullanıcıya ayrıcalıklı rol otomatik verilmez. Gerçek üretim ortamında kontrollü davet ve hesap onaylama akışı değerlendirilmelidir.
- `appsettings.Development.json`, User Secrets, `bin/`, `obj/` ve yerel veritabanı dosyaları gibi geliştirme ve gizli veri içerikleri Git'e eklenmeden önce kontrol edilmelidir.

## Not

Bu çalışma Junior .NET Developer teknik değerlendirme case'i kapsamında hazırlanmıştır.
