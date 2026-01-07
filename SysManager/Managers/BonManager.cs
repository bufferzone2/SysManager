// BonManager.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using SysManager.Models;

namespace SysManager.Managers
{
    /// <summary>
    /// Manager pentru gestionarea bonului fiscal curent
    /// Gestionează adăugarea, ștergerea și modificarea produselor
    /// </summary>
    public class BonManager : INotifyPropertyChanged
    {
        // ═══════════════════════════════════════════════════════════════
        // PROPRIETĂȚI
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Colecția observabilă de produse din bonul curent
        /// </summary>
        public ObservableCollection<BonItem> Items { get; private set; }

        private decimal _total;
        /// <summary>
        /// Totalul bonului curent
        /// </summary>
        public decimal Total
        {
            get => _total;
            private set
            {
                if (_total != value)
                {
                    _total = value;
                    OnPropertyChanged(nameof(Total));
                    TotalModificat?.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// Numărul de articole din bon
        /// </summary>
        public int NumarArticole => Items.Count;

        /// <summary>
        /// Numărul total de bucăți (suma cantităților)
        /// </summary>
        public decimal NumarBucati => Items.Sum(x => x.Cantitate);

        /// <summary>
        /// Verifică dacă bonul este gol
        /// </summary>
        public bool EsteGol => Items.Count == 0;

        // ═══════════════════════════════════════════════════════════════
        // EVENIMENTE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Eveniment declanșat când se adaugă un produs
        /// </summary>
        public event EventHandler<BonItem> ProdusAdaugat;

        /// <summary>
        /// Eveniment declanșat când se șterge un produs
        /// </summary>
        public event EventHandler<BonItem> ProdusSters;

        /// <summary>
        /// Eveniment declanșat când se modifică cantitatea
        /// </summary>
        public event EventHandler<BonItem> CantitateModificata;

        /// <summary>
        /// Eveniment declanșat când se golește bonul
        /// </summary>
        public event EventHandler BonGolit;

        /// <summary>
        /// Eveniment declanșat când se modifică totalul
        /// </summary>
        public event EventHandler<decimal> TotalModificat;

        // ═══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════

        public BonManager()
        {
            Items = new ObservableCollection<BonItem>();

            // ✅ Ascultă modificările în colecție
            Items.CollectionChanged += (s, e) =>
            {
                RecalculeazaTotal();
                OnPropertyChanged(nameof(NumarArticole));
                OnPropertyChanged(nameof(NumarBucati));
                OnPropertyChanged(nameof(EsteGol));
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // METODE PUBLICE - ADĂUGARE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Adaugă sau incrementează produsul în bonul curent
        /// ✅ MODIFICAT: Folosește PretBrut (cu TVA) pentru calcul, NU Pret (fără TVA)!
        /// </summary>
        /// <param name="produs">Produsul de adăugat</param>
        /// <param name="cantitate">Cantitatea (default: 1)</param>
        /// <returns>BonItem adăugat sau actualizat</returns>
        public BonItem AdaugaProdus(Produs produs, decimal cantitate = 1)
        {
            if (produs == null)
                throw new ArgumentNullException(nameof(produs));

            if (cantitate <= 0)
                throw new ArgumentException("Cantitatea trebuie să fie pozitivă", nameof(cantitate));

            // ✅ Verifică dacă produsul există deja
            var itemExistent = Items.FirstOrDefault(x => x.IdProdus == produs.Id);

            if (itemExistent != null)
            {
                // ✅ Produsul există → INCREMENTEAZĂ cantitatea
                var cantitateBefore = itemExistent.Cantitate;
                itemExistent.Cantitate += cantitate;

                Logs.Write($"  → Cantitate actualizată pentru '{itemExistent.Nume}': {cantitateBefore} → {itemExistent.Cantitate}");

                CantitateModificata?.Invoke(this, itemExistent);
                return itemExistent;
            }
            else
            {
                // ✅ Produs NOU → ADAUGĂ în bon
                // ⚠️ MODIFICARE CRITICĂ: Folosim PretBrut (cu TVA), NU Pret (fără TVA)!
                var bonItem = new BonItem
                {
                    IdProdus = produs.Id,
                    Nume = produs.Denumire,
                    Cantitate = cantitate,
                    ValoareTva = produs.ValoareTva,         // Valoare tva produs
                    PretBrut = produs.PretBrut,             // Pret brut
                    ProcentTva = produs.ProcentTva,         // Procent tva
                    TvaId = produs.TvaId,
                    CodSGR = produs.CodSGR,
                    UnitateMasura = produs.UnitateMasura,
                    Departament = produs.Departament
                };

                // ✅ Ascultă modificările cantității pentru recalculare
                bonItem.PropertyChanged += BonItem_PropertyChanged;

                Items.Add(bonItem);

                Logs.Write($"✅ Produs NOU adăugat: {bonItem.Nume} - {bonItem.Cantitate} × {bonItem.PretBrut:F2} LEI = {bonItem.Total:F2} LEI");

                ProdusAdaugat?.Invoke(this, bonItem);
                return bonItem;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // METODE PUBLICE - ȘTERGERE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Șterge un articol din bonul curent
        /// </summary>
        /// <param name="item">Articolul de șters</param>
        /// <returns>True dacă ștergerea a reușit</returns>
        public bool StergeArticol(BonItem item)
        {
            if (item == null)
                return false;

            if (Items.Contains(item))
            {
                item.PropertyChanged -= BonItem_PropertyChanged;
                Items.Remove(item);

                Logs.Write($"🗑️ Produs șters din bon: {item.Nume}");

                ProdusSters?.Invoke(this, item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Șterge un articol după ID-ul produsului
        /// </summary>
        /// <param name="idProdus">ID-ul produsului</param>
        /// <returns>True dacă ștergerea a reușit</returns>
        public bool StergeArticolDupaId(int idProdus)
        {
            var item = Items.FirstOrDefault(x => x.IdProdus == idProdus);
            return item != null && StergeArticol(item);
        }

        /// <summary>
        /// Golește complet bonul curent
        /// </summary>
        /// <returns>Numărul de articole șterse</returns>
        public int GolesteBon()
        {
            int count = Items.Count;

            if (count == 0)
                return 0;

            // ✅ Dezabonează toate event-urile
            foreach (var item in Items.ToList())
            {
                item.PropertyChanged -= BonItem_PropertyChanged;
            }

            Items.Clear();

            Logs.Write($"🗑️ Bon golit complet ({count} articole șterse)");

            BonGolit?.Invoke(this, EventArgs.Empty);
            return count;
        }

        // ═══════════════════════════════════════════════════════════════
        // METODE PUBLICE - MODIFICARE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Modifică cantitatea unui articol
        /// </summary>
        /// <param name="item">Articolul</param>
        /// <param name="cantitateNoua">Noua cantitate</param>
        /// <returns>True dacă modificarea a reușit</returns>
        public bool ModificaCantitate(BonItem item, decimal cantitateNoua)
        {
            if (item == null || !Items.Contains(item))
                return false;

            if (cantitateNoua <= 0)
            {
                // ✅ Dacă cantitatea e 0 sau negativă, șterge articolul
                return StergeArticol(item);
            }

            var cantitateBefore = item.Cantitate;
            item.Cantitate = cantitateNoua;

            Logs.Write($"📝 Cantitate modificată pentru '{item.Nume}': {cantitateBefore} → {cantitateNoua}");

            CantitateModificata?.Invoke(this, item);
            return true;
        }

        /// <summary>
        /// Incrementează cantitatea unui articol
        /// </summary>
        /// <param name="item">Articolul</param>
        /// <param name="increment">Valoarea de incrementare (default: 1)</param>
        /// <returns>True dacă operația a reușit</returns>
        public bool IncrementeazaCantitate(BonItem item, decimal increment = 1)
        {
            if (item == null || !Items.Contains(item))
                return false;

            return ModificaCantitate(item, item.Cantitate + increment);
        }

        /// <summary>
        /// Decrementează cantitatea unui articol
        /// </summary>
        /// <param name="item">Articolul</param>
        /// <param name="decrement">Valoarea de decrementare (default: 1)</param>
        /// <returns>True dacă operația a reușit</returns>
        public bool DecrementeazaCantitate(BonItem item, decimal decrement = 1)
        {
            if (item == null || !Items.Contains(item))
                return false;

            return ModificaCantitate(item, item.Cantitate - decrement);
        }

        // ═══════════════════════════════════════════════════════════════
        // METODE PUBLICE - CĂUTARE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Găsește un articol după ID-ul produsului
        /// </summary>
        /// <param name="idProdus">ID-ul produsului</param>
        /// <returns>BonItem sau null dacă nu există</returns>
        public BonItem GasesteArticol(int idProdus)
        {
            return Items.FirstOrDefault(x => x.IdProdus == idProdus);
        }

        /// <summary>
        /// Verifică dacă un produs există în bon
        /// </summary>
        /// <param name="idProdus">ID-ul produsului</param>
        /// <returns>True dacă produsul există</returns>
        public bool ExistaProdus(int idProdus)
        {
            return Items.Any(x => x.IdProdus == idProdus);
        }

        // ═══════════════════════════════════════════════════════════════
        // METODE PUBLICE - STATISTICI
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Returnează detalii statistice despre bon
        /// </summary>
        public string GetDetaliiStatistice()
        {
            return $"Articole: {NumarArticole} | Bucăți: {NumarBucati} | Total: {Total:F2} LEI";
        }

        /// <summary>
        /// Calculează TVA-ul total
        /// </summary>
        public decimal CalculeazaTVATotal()
        {
            return Items.Sum(item => item.ValoareTva * item.Cantitate);
        }

        /// <summary>
        /// Calculează valoarea fără TVA
        /// </summary>
        public decimal CalculeazaTotalFaraTVA()
        {
            return Total - CalculeazaTVATotal();
        }

        // ═══════════════════════════════════════════════════════════════
        // METODE PRIVATE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Recalculează totalul bonului
        /// ✅ MODIFICAT: Folosește Total (care e calculat din Cantitate × Pret, 
        /// iar Pret e acum PretBrut)
        /// </summary>
        private void RecalculeazaTotal()
        {
            // ✅ Total se calculează automat în BonItem.Total (Cantitate × Pret)
            // Pret este acum setat la PretBrut (cu TVA), deci totalul va fi corect
            Total = Items.Sum(item => item.Total);

            Logs.Write($"💰 Total bon actualizat: {Total:F2} LEI ({NumarArticole} articole, {NumarBucati} bucăți)");
        }

        /// <summary>
        /// Handler pentru modificarea proprietăților din BonItem
        /// </summary>
        private void BonItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BonItem.Total) ||
                e.PropertyName == nameof(BonItem.Cantitate) ||
                e.PropertyName == nameof(BonItem.PretBrut))
            {
                RecalculeazaTotal();

                if (sender is BonItem item && e.PropertyName == nameof(BonItem.Cantitate))
                {
                    CantitateModificata?.Invoke(this, item);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // INotifyPropertyChanged IMPLEMENTATION
        // ═══════════════════════════════════════════════════════════════

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
