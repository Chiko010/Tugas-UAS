using System;

namespace Aplikasi_Manajemen_Bangun_Geometri
{ 
    public abstract class BangunGeometri
    {
        public int Id { get; set; }
        public string Nama { get; set; }
        public string Tipe { get; set; }
        public double Dimensi1 { get; set; }
        public double Dimensi2 { get; set; }

        public abstract double HitungLuas();
        public abstract double HitungKeliling();
    }

    public class Persegi : BangunGeometri
    {
        public override double HitungLuas() => Dimensi1 * Dimensi1;
        public override double HitungKeliling() => 4 * Dimensi1;
    }

    public class PersegiPanjang : BangunGeometri
    {
        public override double HitungLuas() => Dimensi1 * Dimensi2;
        public override double HitungKeliling() => 2 * (Dimensi1 + Dimensi2);
    }

    public class Lingkaran : BangunGeometri
    {
        public override double HitungLuas() => Math.PI * Dimensi1 * Dimensi1;
        public override double HitungKeliling() => 2 * Math.PI * Dimensi1;
    }

    public class Segitiga : BangunGeometri
    {
        public override double HitungLuas() => 0.5 * Dimensi1 * Dimensi2;
        public override double HitungKeliling()
        {
            double sisiMiring = Math.Sqrt(Dimensi1 * Dimensi1 + Dimensi2 * Dimensi2);
            return Dimensi1 + Dimensi2 + sisiMiring;
        }
    }
}