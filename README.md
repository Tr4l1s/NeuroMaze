<h1 align="center">NeuroMaze</h1>

<p align="center">
Çocuklar için geliştirilmiş, gerçek zamanlı nabız verisiyle adaptif zorluk sunan 3D labirent oyunu.
</p>

---

## Proje Hakkında

NeuroMaze, çocuklar için geliştirilmiş adaptif bir 3D labirent oyunudur.  
Oyun, gerçek zamanlı nabız verisini kullanarak dinamik zorluk ayarlaması yapar ve oyuncunun fizyolojik durumuna göre oyun deneyimini optimize eder.  

Proje, Unity tabanlı oyun sistemi ile SwiftUI kullanılarak geliştirilen iOS nabız ölçüm modülünün entegre çalışması üzerine kuruludur.

---

## Proje Amacı

NeuroMaze’in amacı, çocukların stres ve heyecan seviyelerine duyarlı bir oyun deneyimi sunmaktır.  

Geleneksel sabit zorluk seviyeleri yerine, oyuncunun kalp atış hızına göre şekillenen bir sistem tasarlanmıştır.  

Bu sayede:

- Aşırı stres durumunda güvenli alanlar devreye girer  
- Labirent atmosferi dinamik olarak değişir  
- Oyun içi varlıkların davranışları nabız verisine göre ayarlanır  

Bu yapı, hem eğlenceli hem de kontrollü bir deneyim sunmayı hedefler.

---

## Sistem Mimarisi

Proje iki ana bileşenden oluşmaktadır:

### 1. Unity Oyun Motoru

- 3D labirent tasarımı  
- Oyun mekaniği ve karakter kontrolü  
- Güvenli alan (Safe Zone) sistemi  
- Dinamik ortam karartma ve atmosfer değişimi  
- Oyun içi soru paneli sistemi  
- Adaptif zorluk mekanizması  

---

### 2. iOS Nabız Ölçüm Modülü (SwiftUI)

- Kamera ve flaş kullanarak nabız ölçümü  
- Gerçek zamanlı BPM hesaplama  
- AVFoundation tabanlı görüntü işleme  
- Unity ile veri entegrasyonu  

Unity tarafı C# ile, nabız ölçüm tarafı ise SwiftUI ve AVFoundation kullanılarak geliştirilmiştir.

---

## Adaptif Oyun Mekaniği

NeuroMaze’in en önemli özelliği gerçek zamanlı biyometrik veri entegrasyonudur.

- Oyuncunun nabzı yükseldiğinde sistem bunu algılar  
- Belirli eşik değerlerinde güvenli alanlar aktif olur  
- Ortam ışığı ve atmosfer değişir  
- Oyun içi tehdit seviyesi dinamik olarak ayarlanır  

Bu sistem klasik zorluk ayarlarından farklı olarak, oyuncuya özgü bir deneyim oluşturur.

---

## Teknik Detaylar

### Kullanılan Teknolojiler

- Unity (C#)
- SwiftUI
- AVFoundation
- Xcode
- iOS Camera & Torch API
- Git & GitHub

---

### Öne Çıkan Teknik Konular

- Gerçek zamanlı veri akışı  
- Cross-platform entegrasyon (Unity – iOS)  
- Adaptif oyun mantığı tasarımı  
- Oyun içi durum yönetimi  
- Performans optimizasyonu  

---

## Proje Geliştiricileri

**Unity Oyun Sistemi:** Murat Kaşlı  
**iOS Nabız Ölçüm Sistemi:** Beyza Uysal  

Oyun ve mobil sistem entegrasyonu ortak çalışma ile gerçekleştirilmiştir.

---

## Yarışma ve Başarı

NeuroMaze, bir yatırım yarışmasında final aşamasına kadar ilerlemiştir.  
Proje, teknik altyapısı ve yenilikçi yaklaşımı ile dikkat çekmiştir.

---

## 🚀 Gelecek Hedefler

- Gelişmiş veri analizi ve kayıt sistemi  
- Eğitim kurumları için lisanslama modeli  
- Genişletilmiş biyometrik veri desteği  
- App Store yayını ve ölçeklenebilir mimari  

---

## 📄 Lisans

Bu proje eğitim ve geliştirme amaçlı hazırlanmıştır.  
Tüm hakları saklıdır.
