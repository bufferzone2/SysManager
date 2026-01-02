// BonAsteptare.cs
using System;
using System.Collections.Generic;

namespace SysManager.Models
{
    /// <summary>
    /// Model pentru bonurile în așteptare
    /// </summary>
    public class BonAsteptare
    {
        public int Id { get; set; }
        public string NrBon { get; set; }
        public DateTime DataCreare { get; set; }
        public int IdUtilizator { get; set; }
        public int IdGestiune { get; set; }
        public decimal Total { get; set; }
        public string Observatii { get; set; }
        public string NumeClient { get; set; }
        public string Status { get; set; }

        // Lista produse
        public List<BonAsteptareDetaliu> Detalii { get; set; } = new List<BonAsteptareDetaliu>();

        /// <summary>
        /// Afișare pentru UI
        /// </summary>
        public string DisplayText => $"BON #{NrBon} - {Total:F2} LEI";
        public string DisplayInfo => $"{NumeClient ?? "Client necunoscut"} | {DataCreare:dd.MM.yyyy HH:mm}";
    }

    /// <summary>
    /// Detalii produs din bonul în așteptare
    /// </summary>
    public class BonAsteptareDetaliu
    {
        public int Id { get; set; }
        public int IdBonAsteptare { get; set; }
        public int IdProdus { get; set; }
        public string DenumireProdus { get; set; }
        public decimal Cantitate { get; set; }
        public decimal PretUnitar { get; set; }
        public decimal Valoare { get; set; }
    }
}
