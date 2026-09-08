# NeuroMaze — oyun içi Android nabız kaydı

## Kullanım

1. Oyundan önce **NABIZ / KAYIT** ekranında oyuncu kodunu (K001 gibi) yazın, klavyeden **Tamam**, ardından **Yeni oturum** seçin. Panel artık kayıt ekranıdır; manuel ölçüm düğmeleri yoktur.
2. Ortam ışığıyla ölçüm için **Ek telefon ışığı kullanıyorum** kutusunu kapalı bırakın. Başka telefonun ışığını kullanıyorsanız işaretleyin.
3. **Oyuna dön** ile labirenti oynayın. Güvenli alana girince sorular ve kamera ölçümü otomatik başlar. Kamera izni istenirse verin. Oyuncu hareketi 30 saniye kilitlenir; sorular cevaplanabilir.
4. Parmağı arka kamerada, kolu destekli ve sabit tutun. Mevcut nabız metni kalan süreyi ve sinyal durumunu gösterir. Süre bitince başlangıç, ortalama ve bitiş sonuçları gösterilir; oyuncu tekrar hareket edebilir.
5. Aynı oyuncunun tüm güvenli alan ziyaretleri aynı oturumda kalır. Sonraki çocuk için yeni kodla **Yeni oturum** açın. Kod girilmeden oynanırsa P ile başlayan otomatik bir kod oluşturulur; katılımcı eşleştirmesini kendiniz yapmanız gerekir.
6. **Kayıtları ZIP aktar** ile dosyayı **Dosyalarım → İndirilenler → NeuroMaze** konumuna kaydedin.

## Oyun zamanlaması

- Sahne ayarındaki ilk düşman doğma gecikmesi 30 saniyedir. Oyuncu güvenli alandayken bu sayaç ilerlemez; alandan çıkınca kalan süreden devam eder.
- Doğmuş düşman güvenli alanda görünmez ve oyuncuyu yakalayamaz. Oyuncu çıkınca 15 saniye bekler.
- Sorular ve ölçüm 30 saniye sürer. Soruların bitmesi oyuncunun hareketini açar; düşman koruması güvenli alanın dışına çıkana kadar sürer.
- Hazırlanan soru bankası kullanılır. Banka boşsa sahnedeki sorular korunur; ikisi de boşsa üç basit örnek soru gösterilir.
- Ölçüm, oyunun zaman ölçeğini durdurmaz. Android'de deneysel BPM üzerinden düşman hızı veya stres tanısı değiştirilmez.

## Başlangıç / ortalama / bitiş ne demek?

Tek kareden anlık nabız hesaplanmaz. Her güvenli alan ziyaretinde sabit aralıklar değerlendirilir:

- Başlangıç: 3–12. saniye (ilk üç saniye parmağı yerleştirmek için dışarıda).
- Orta: 12–21. saniye.
- Bitiş: 21–30. saniye.

Her aralık için en az 8 saniye kesintisiz ve sinyal kontrolünden geçen veri gerekir. Kamera geç açılırsa veya parmak geç konursa başlangıç boş kalabilir; sonraki veri başlangıç yerine yazılmaz. Eksik değerler `—` olarak görünür ve CSV'de boş kalır.

Ortalama, yalnızca kabul edilen aralıkların süre ağırlıklı BPM ortalamasıdır. **1/3 veya 2/3 pencere varsa bu eksik kapsamlı bir sonuçtur; 30 saniyenin tamamını temsil etmez.** Oyun boyunca sürekli nabız ölçümü yapılmaz; kayıtlar güvenli alan ziyaretlerine aittir.

## ZIP dosyaları

- **safe_zone_visits.csv:** Her güvenli alan ziyareti; oyuncu/oturum/alan, zaman, başlangıç-orta-ortalama-bitiş BPM, geçerli pencere sayısı, kapsam, doğru cevap sayısı, sinyal gerekçeleri.
- **measurements.csv:** Eski manuel geliştirme denemeleri dahil bütün ölçüm kayıtları.
- **phase_summary.csv:** Aynı oyuncu/oturum/kaynak/aşamadaki kabul edilen kayıtların aritmetik özetleri. Bir sürekli oyun ortalaması değildir; güvenli alan analizinde ayrıntılı ziyaret dosyasını kullanın.
- **records/** ve **signals/**: Ayrıntılı JSON ve zaman damgalı RGB/parlaklık sinyalleri. Fotoğraf veya video saklanmaz.

Uygulama yeniden başlatılırsa oturum seçimi sıfırlanır; dosyalar kalır. Aynı çocuğun oturumunu baştan başlatmanız gerekirse yeni oturum kimliğini ayrıca not edin. Uygulamayı kaldırmak veya verisini silmek dışa aktarılmamış kayıtları silebilir.

## Doğrulama sınırı

Bu sistem deneysel kamera PPG tahmini üretir; tablet ve çocuklar için referans cihazla doğrulanmadı. Sinyal kontrolünü geçmesi doğruluk kanıtı değildir. Makalede doğrulanmış nabız ölçümü olarak kullanılmadan önce referans ölçümle karşılaştırılmalıdır. Kayıtlarda `clinically_validated=false` bulunur.

Önceki, 25 saniyelik manuel sürümde ortam ışığında 58,74 ve 58,32 BPM geliştirme sonuçları alındı. Bu, yeni kısa oyun aralıklarının doğrulandığı anlamına gelmez. 001, TEST002 ve TEST003 eski geliştirme kodlarıdır; araştırma katılımcılarıyla karıştırmayın.

## Teknik kontrol

55 otomatik kontrol: sentetik sinyaller, kısa oyun aralıkları, geciken parmakta başlangıcın boş kalması, kayıt/ZIP ve güvenli alanda düşman sayacının durması. Unity C# derleme kontrolü geçti. Algoritma etiketi `android-game-ppg-2`. Gerçek cihaz testlerinin sonucu teslim mesajında belirtilir.

Build: Unity'de **Tools → NeuroMaze → Build Android Study APK**. ARM64 / IL2CPP / Android APK üretir. Açık ve lisanslı Editor, `Temp/PulseAndroidBuild.request` adlı tek seferlik yerel istek dosyasını da içeri aktarma tamamlandıktan sonra derler; istek başlamadan silinir.

