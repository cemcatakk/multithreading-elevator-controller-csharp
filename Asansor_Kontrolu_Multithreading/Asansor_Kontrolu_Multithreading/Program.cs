using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Asansor_Kontrolu_Multithreading
{
    enum Yon { Yukari, Asagi }
    enum Mod { Beklemede, Calisiyor }
    class Musteri
    {
        public Kat bulunduguKat { get; set; }
        public int gidecegiKat { get; set; }
        public bool asansordeMi { get; set; }
    }
    class Asansor
    {
        public bool aktifmi { get; set; }
        public Mod mod { get; set; }
        public int bulunduguKat { get; set; }
        public int gidecegiKat { get; set; }
        public int kapasite { get; set; }
        public Yon yon;
        public List<Musteri> icindekiKisiler { get; set; }
        public int icindekiKisiSayisi
        {
            get
            {
                return icindekiKisiler.Count;
            }
            set { this.icindekiKisiSayisi = value; }
        }
        private bool doluMu()
        {
            return icindekiKisiSayisi == kapasite ? true : false;
        }
        private void MusteriBindir(Musteri musteri)
        {
            if (this.icindekiKisiler.Count < this.kapasite)
            {
                lock (icindekiKisiler)
                {
                    musteri.asansordeMi = true;
                    this.icindekiKisiler.Add(musteri);
                }
            }
        }
        public void KatBelirle()
        {
            if (icindekiKisiSayisi > 0)
            {
                foreach (Musteri musteri in icindekiKisiler)
                {
                    if (yon == Yon.Yukari)
                    {
                        if (musteri.gidecegiKat > gidecegiKat)
                        {
                            gidecegiKat = musteri.gidecegiKat;
                        }
                    }
                    else
                    {
                        if (musteri.gidecegiKat < gidecegiKat)
                        {
                            gidecegiKat = musteri.gidecegiKat;
                        }
                    }
                }
            }
        }
        public void HedefeGit()
        {
            if (yon == Yon.Yukari)//Yukarı
            {
                mod = Mod.Calisiyor;
                while (bulunduguKat <= gidecegiKat)
                {
                    lock (Program.Katlar[bulunduguKat])
                    {
                        foreach (Musteri musteri in icindekiKisiler.ToList())
                        {
                            if (musteri.gidecegiKat == bulunduguKat)
                            {
                                lock (Program.Katlar[bulunduguKat])
                                {
                                    musteri.asansordeMi = false;
                                    Program.Katlar[bulunduguKat].musteriler.Add(musteri);
                                    icindekiKisiler.Remove(musteri);
                                }
                            }
                        }
                        foreach (Musteri musteri in Program.Katlar[bulunduguKat].musteriler.ToList())
                        {
                            if (doluMu()) break;
                            else if (musteri.gidecegiKat > this.bulunduguKat)
                            {
                                MusteriBindir(musteri);
                            }
                        }
                    }
                    if (bulunduguKat == gidecegiKat)
                    {
                        break;
                    }
                    Thread.Sleep(200);
                    bulunduguKat++;
                }
                yon = Yon.Asagi;
                mod = Mod.Beklemede;
            }
            else //Aşağı
            {
                while (bulunduguKat >= gidecegiKat)
                {
                    mod = Mod.Calisiyor;
                    lock (Program.Katlar[bulunduguKat])
                    {
                        foreach (Musteri musteri in icindekiKisiler.ToList())
                        {
                            if (musteri.gidecegiKat == bulunduguKat)
                            {
                                musteri.asansordeMi = false;
                                Program.Katlar[bulunduguKat].musteriler.Add(musteri);
                                icindekiKisiler.Remove(musteri);
                            }
                        }
                        foreach (Musteri musteri in Program.Katlar[bulunduguKat].musteriler.ToList())
                        {
                            if (doluMu()) break;
                            else if (musteri.gidecegiKat < this.bulunduguKat)
                            {
                                MusteriBindir(musteri);
                            }
                        }
                    }
                    if (bulunduguKat == gidecegiKat)
                    {
                        break;
                    }
                    Thread.Sleep(200);
                    bulunduguKat--;
                }
                yon = Yon.Yukari;
                mod = Mod.Beklemede;
            }
        }

    }
    class Kat
    {
        public int kat { get; set; }
        public List<Musteri> musteriler { get; set; }
        public void MusteriEkle(Musteri musteri)
        {
            musteriler.Add(musteri);
            musteri.bulunduguKat = this;
        }
        public int Kuyruk()
        {
            int sayac = 0;
            foreach (Musteri musteri in this.musteriler.ToList())
            {
                if (musteri.gidecegiKat != kat)
                {
                    sayac++;
                }
            }
            return sayac;
        }
    }
    class Program
    {
        static List<Asansor> Asansorler = new List<Asansor>();
        static public List<Kat> Katlar = new List<Kat>();
        static Random rnd = new Random();
        public static int CikanKisiSayisi = 0;
        public static void AVMGiris()
        {
            while (true)
            {
                lock (Katlar[0])
                {
                    int yeniMusteriSayisi = rnd.Next(1, 10);
                    for (int i = 0; i < yeniMusteriSayisi; i++)
                    {
                        Musteri yeniMusteri = new Musteri();
                        yeniMusteri.bulunduguKat = Katlar[0];
                        yeniMusteri.gidecegiKat = rnd.Next(1, 5);
                        Katlar[0].MusteriEkle(yeniMusteri);
                    }
                }
                Thread.Sleep(500);
            }
        }
        public static void AVMCikis()
        {
            int rastgeleKat = rnd.Next(1, 5);
            int musteriSayisi = rnd.Next(1, 6);
            while (true)
            {
                for (int i = 0; i < musteriSayisi; i++)
                {
                    lock (Katlar[rastgeleKat])
                    {
                        if (Katlar[rastgeleKat].musteriler.Count >= 1)
                        {
                            Katlar[rastgeleKat].musteriler[0].gidecegiKat = 0;
                            rastgeleKat = rnd.Next(1, 5);
                        }
                    }
                }
                lock (Katlar[0].musteriler)
                {
                    foreach (Musteri musteri in Katlar[0].musteriler.ToList())
                    {
                        if (musteri.gidecegiKat == 0)
                        {
                            Katlar[0].musteriler.Remove(musteri);
                            CikanKisiSayisi++;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }
        public static void AsansorThread()
        {
            Asansor asansor = new Asansor();
            asansor.bulunduguKat = 0;
            asansor.aktifmi = false;
            asansor.mod = Mod.Beklemede;
            asansor.kapasite = 10;
            asansor.yon = Yon.Yukari;
            asansor.gidecegiKat = 0;
            asansor.icindekiKisiler = new List<Musteri>();
            Asansorler.Add(asansor);
            while (true)
            {
                if (asansor.aktifmi)
                {
                    asansor.KatBelirle();
                    asansor.HedefeGit();
                }
            }
        }
        public static int CalisanAsansorSayisi()
        {
            int sayac = 0;
            foreach (Asansor asansor in Asansorler)
            {
                if (asansor.aktifmi)
                {
                    sayac++;
                }
            }
            return sayac;
        }

        public static void KontrolThread()
        {
            Asansorler[0].aktifmi = true;
            int toplamBekleyen;
            while (true)
            {
                toplamBekleyen = 0;
                lock (Katlar)
                {
                    foreach (Kat kat in Katlar)
                    {
                        lock (kat)
                        {
                            foreach (Musteri musteri in kat.musteriler.ToList())
                            {
                                if (musteri != null)
                                {
                                    if (musteri.gidecegiKat != musteri.bulunduguKat.kat && !musteri.asansordeMi)
                                    {
                                        toplamBekleyen++;
                                    }
                                }
                            }
                        }
                    }
                }

                if (toplamBekleyen >= Asansorler[0].kapasite * 2 * (CalisanAsansorSayisi()))
                {
                    foreach (Asansor asansor in Asansorler)
                    {
                        if (!asansor.aktifmi)
                        {
                            asansor.aktifmi = true;
                            break;
                        }
                    }
                }
                else
                {
                    if (CalisanAsansorSayisi() > 1)
                    {
                        foreach (Asansor asansor in Asansorler)
                        {
                            if (asansor.icindekiKisiSayisi == 0)
                            {
                                asansor.aktifmi = false;
                            }
                        }
                    }
                }
            }
        }
        public static void BilgiYazdir()
        {
            while (true)
            {
                for (int i = 0; i < Katlar.Count; i++)
                {
                    Console.WriteLine("{0}. Kat: Kişi Sayısı: {1} Kuyruk: {2}", i, Katlar[i].musteriler.Count, Katlar[i].Kuyruk());
                }
                Console.WriteLine("Çıkan kişi sayısı: {0}\n\n", CikanKisiSayisi);
                foreach (Asansor asansor in Asansorler)
                {
                    Console.WriteLine("Aktif mi: {0}", asansor.aktifmi.ToString());
                    Console.WriteLine("\t\t\tMod: {0}", asansor.mod.ToString());
                    Console.WriteLine("\t\t\tKat: {0}", asansor.bulunduguKat.ToString());
                    Console.WriteLine("\t\t\tHedef: {0}", asansor.gidecegiKat.ToString());
                    Console.WriteLine("\t\t\tYön: {0}", asansor.yon.ToString());
                    Console.WriteLine("\t\t\tKapasite: {0}", asansor.kapasite.ToString());
                    Console.WriteLine("\t\t\tİçindeki Kişi Sayısı: {0}", asansor.icindekiKisiSayisi.ToString());
                    Console.Write("\t\t\tMüşteriler ve Hedefleri:\n", asansor.icindekiKisiSayisi.ToString());
                    int[] katlar = new int[5] { 0, 0, 0, 0, 0 };
                    foreach (Musteri musteri in asansor.icindekiKisiler)
                    {
                        katlar[musteri.gidecegiKat]++;
                    }
                    for (int i = 0; i < katlar.Length; i++)
                    {
                        if (katlar[i] != 0)
                        {
                            Console.Write("{0} Adet => {1}.Kata \t\t", katlar[i], i);
                        }
                    }
                    Console.WriteLine("\n");
                }
                Thread.Sleep(2000);
                Console.Clear();
            }
        }
        static void Main(string[] args)
        {
            for (int i = 0; i < 5; i++)
            {
                Kat kat = new Kat();
                kat.kat = i;
                kat.musteriler = new List<Musteri>();
                Katlar.Add(kat);
            }
            Thread GirisThread = new Thread(new ThreadStart(AVMGiris));
            Thread CikisThread = new Thread(new ThreadStart(AVMCikis));
            Thread Asansor1 = new Thread(AsansorThread);
            Thread Asansor2 = new Thread(AsansorThread);
            Thread Asansor3 = new Thread(AsansorThread);
            Thread Asansor4 = new Thread(AsansorThread);
            Thread Asansor5 = new Thread(AsansorThread);
            Thread Kontrol = new Thread(KontrolThread);
            Thread KonsolThread = new Thread(BilgiYazdir);
            GirisThread.Start();
            CikisThread.Start();
            Asansor1.Start();
            Asansor2.Start();
            Asansor3.Start();
            Asansor4.Start();
            Asansor5.Start();
            while (true)
            {
                if (Asansorler.Count == 5)
                {
                    Kontrol.Start();
                    KonsolThread.Start();
                    break;
                }
            }
            Console.ReadKey();
        }
    }
}
