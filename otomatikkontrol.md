# Final Ödevi

## Unity Ortamında PID Kontrol ile Araba Şerit Takibi

Ders: Otomatik Kontrol

## Ödev Tanımı

Öğrencilerden Unity ortamında bir araba şerit takip sistemi geliştirmeleri istenmek-
tedir. Simülasyonda araç, tanımlanan yol veya şerit merkez çizgisini takip etmelidir. Yol
yapısı yalnızca düz hatlardan oluşmamalı; farklı eğriliklerde virajlar içermelidir.
Araç, PID kontrol kullanarak şerit merkezinden sapmayı minimize edecek şekilde yönlen-
dirilmelidir.

## Sistem Açıklaması

Bu projedetemelamaç,aracınyol üzerindekişeritmerkezçizgisinemümkünoldu-
ğuncayakınkalmasıdır.
Kontrolsistemigenelolarakşu şekildetanımlanacaktır:

- Referansdeğerr(t): Şeritmerkezhattı/ hedefyörünge
- Sistemçıkışıy(t): Şeritmerkezinegöreyanalkonumu
- Hatae(t): Aracınşeritmerkezindensapması
- Kontrolçıkışıu(t): Aracınsağaveyasolayönelmesinisağlayankontrolgirdisi

## İstenenler

1. UnityOrtamındaYol ve AraçModeliOluşturulması
    Unityortamındaaşağıdakiözellikleresahipbir sahneoluşturmalarıbeklenmektedir:
       - bir araçmodeli,
       - en az bir şeritveyayol merkezitanımı,
       - yol üzerindevirajlıbölümler,
       - hafifviraj+ keskinvirajiçerenyapı(en az 6 virajlı).
Yol yapısıyalnızcadüz bir çizgidenoluşmamalıdır.Araç performansınınözellikleviraj-
lardagözlemlenebilmesiiçin rotakıvrımlıolmalıdır.


2. PID Kontrolcünün Gerçeklenmesi
Araç şerit takibini sağlamak için PID kontrolcü uygulanmalıdır.
PID kontrol denklemi:

```
u(t) = Kpe(t) + Ki
```
### Z

```
e(t) dt + Kddedt(t)
```
Burada:

- Kp: oransal kazanç
- Ki: integral kazanç
- Kd: türevsel kazanç
Aracın kontrol çıkışı u(t) ve referans yol ile izlenen yolun karşılaştırmasına ait grafikler
Unity içinde gösterilmelidir. PID denetleyici Unity projesi içinde gerçek zamanlı çalışma-
lıdır. C# scriptlerinin geliştirilmesinde Visual Studio kullanılabilir.
3. Hata Tanımı
Araç için hata sinyali, aracın referans yolun merkez hattına göre yanal sapması olarak
tanımlanmalıdır.

```
e(t) = r(t)− y(t)
```
Burada r(t) referans yolun merkezini, y(t) ise aracın mevcut yanal konumunu ifade eder.

4. KontrolÇıkışı
Kontrolcüçıkışıu(t), aracınyol üzerindeşerit merkezinetekraryaklaşmasınısağlayan
yönlendirmekontrolsinyalidir.
Bu sinyal,aracınhareketyönünüdeğiştirereksağaveyasola yönelmesineetki eder.Başka
bir ifadeyle,kontrolcütarafındanüretilenu(t)değeri,aracınşeritmerkezindensapmasını
azaltmakiçin kullanılankontrolgirdisidir.
5. P, PI ve PID Kontrolcülerinin Test Edilmesi
Aynı sistem üzerinde P, PI ve PID kontrolcülerinin ayrı ayrı uygulanması ve elde edi-
len sonuçların karşılaştırılması zorunludur. Karşılaştırmada özellikle referans yolu takip
başarımı dikkate alınmalıdır.


## Çizdirilmesi İstenen Grafikler

Her öğrenci, simülasyon sonucunda aşağıdaki grafiklere raporunda yer vermelidir.

Zorunlu Grafikler

- kontrolcü çıkışı u(t),
- referans ve gerçek izlenen yol karşılaştırması.

## Analizler

```
Öğrencilerraporlarındaaşağıdakisorularacevapvermelidir:
```
- Araçdüz yoldanasıldavranmaktadır?
- Virajlardahatanasıldeğişmektedir?
- PIDparametrelerideğiştirildiğindearaçdavranışınasıletkilenmektedir?
- Hangiparametreseti en iyi sonucuvermiştir?
- Kontrolsinyaliçok agresifmidir,yumuşakmıdır?
- Araçsalınımyapmaktamıdır?
- Sistemkararlımıdır?
- Keskinvirajlardatakipbaşarımıdüşmektemidir?
- Kontrolcüparametrelerideğiştiğindeyükselme ve yerleşmesürelerine
ilişkindavranışnasıletkilenmektedir?

## TeslimdeBulunmasıGerekenler

1. Unity Projesi
    - Çalışan proje dosyaları
    - Kodlar düzenli ve açıklamalı olmalıdır
2. Rapor
Raporaşağıdakibaşlıklarıiçermelidir:
    (a) Amaç
(b) Sistemve yol tanımı
(c) Hatave kontrolcüyapısı
(d) Kullanılanparametreler
(e) Grafikler
(f) Karşılaştırmave analizler


```
(g) Sonuç
```
## Teknik Beklentiler

- Araç şerit merkezini mümkün olduğunca takip etmelidir.
- Yol üzerinde mutlaka viraj bulunmalıdır.
- Kontrolcü doğrudan öğrenci tarafından kodlanmalıdır.
- Hazır bir paket kullanılmışsa, öğrencinin kontrol mantığını ayrıntılı biçimde açık-
    laması gerekir.
- Grafikler simülasyondan elde edilen gerçek verilere dayanmalıdır.
- Kod içinde kullanılan değişkenler anlamlı olmalıdır.
- PID parametre ayarı deneme-yanılma veya sistematik yöntemle yapılabilir; ancak
    nasıl seçildiği raporda belirtilmelidir.

## Değerlendirme Ölçütleri

100 Puan Üzerinden

1. Unity ortamında araç ve yol modelinin kurulması – 15 puan
    - Sahnenin çalışır durumda olması
    - Yolun virajlar içermesi
2. Şerit takip mantığının kurulması – 15 puan
    - Hata tanımının doğru yapılması
    - Referans yolun doğru belirlenmesi
3. PID kontrolcünün doğru uygulanması – 20 puan
    - Kp, Ki, Kdbileşenlerinin doğru kullanılması
    - Kontrol çıkışının araca doğru uygulanması
4. Virajlı yolda başarı – 15 puan
    - Aracın virajlarda şeritten çıkmadan veya minimum sapmayla ilerlemesi
5. Grafiklerin sunulması – 10 puan
    - e(t), u(t), yol takibi ve sapma grafiklerinin verilmesi
6. Analiz ve yorum – 15 puan
    - PID parametrelerinin etkilerinin doğru yorumlanması


- Düz yol ve virajlı yol farklarının açıklanması
7. Rapor kalitesi – 10 puan

## Önemli Notlar

- Yolun yalnızca düz olması kabul edilmeyecektir.
- Virajlı yol yapısı ödevin temel gerekliliklerinden biridir.
- Sadece çalışan bir araç modeli yeterli değildir; hata, kontrol sinyali ve sistem dav-
    ranışı analiz edilmelidir.
- Grafik vermeyen veya teknik yorum içermeyen çalışmalar eksik kabul edilecektir.

```
Başarılar dilerim.
```

