// Fisier: SysManager/Models/MetodaPlata.cs
using System;

namespace SysManager.Models
{
    /// <summary>
    /// Tipuri de metode de plată disponibile
    /// </summary>
    public enum TipMetodaPlata
    {
        Numerar = 1,
        Card = 2,
        Voucher = 3,
        TicketMasa = 4,
        Transfer = 5,
        CEC = 6,
        BonValoare = 7,
        Online = 8,
        Bacsis = 99  // Separat, nu e metodă principală
    }

    /// <summary>
    /// Metodă de plată cu sumă
    /// </summary>
    public class MetodaPlata
    {
        public TipMetodaPlata Tip { get; set; }
        public string Nume { get; set; }
        public decimal Suma { get; set; }
        public string IconPath { get; set; }

        public MetodaPlata(TipMetodaPlata tip, string nume, string iconPath = null)
        {
            Tip = tip;
            Nume = nume;
            IconPath = iconPath;
            Suma = 0;
        }

        public override string ToString()
        {
            return $"{Nume}: {Suma:F2} lei";
        }
    }
}
