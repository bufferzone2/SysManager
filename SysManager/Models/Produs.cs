// Models/Produs.cs
namespace SysManager.Models
{
    public class Produs
    {
        public int Id { get; set; }
        public string Denumire { get; set; }
        public decimal ValoareTva { get; set; }           // Valoare tva produs
        public decimal PretBrut { get; set; }       // Preț cu TVA
        public decimal ProcentTva { get; set; }     // Procentul TVA (ex: 19)
        public string CaleImagine { get; set; }     // URL/path imagine
        public int ShowImage { get; set; }          // 0/1 - afișează imagine
        public int TvaId { get; set; }              // ID TVA
        public string CodSGR { get; set; }          // Cod SGR
        public string UnitateMasura { get; set; }   // "buc", "kg", etc.
        public int Departament { get; set; }        // ID departament
        public int TvaAmefId { get; set; }          // ID TVA AMEF

        // ✅ PROPRIETATE NOUĂ: Numele gestiunii
        public string NumeGestiune { get; set; }

        // Proprietăți calculate pentru afișare
        public string PretFormatat => $"{PretBrut:F2} RON";
        public string StocFormatat => "N/A"; // Dacă ai stoc, calculează aici
        public bool AreImagine => ShowImage == 1 && !string.IsNullOrWhiteSpace(CaleImagine);
    }
}
