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
        private readonly DbQuery _dbQuery;
        private readonly SmConfig _config;
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

        public BonManager(DbQuery dbQuery, SmConfig config)
        {
            _dbQuery = dbQuery ?? throw new ArgumentNullException(nameof(dbQuery));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            Items = new ObservableCollection<BonItem>();

            Items.CollectionChanged += (s, e) =>
            {
                RecalculeazaTotal();
                OnPropertyChanged(nameof(NumarArticole));
                OnPropertyChanged(nameof(NumarBucati));
                OnPropertyChanged(nameof(EsteGol));
            };

            Logs.Write($"✅ BonManager inițializat - SGR={(_config.IsSGREnabled ? "ACTIVAT" : "DEZACTIVAT")}");
        }

        // ═══════════════════════════════════════════════════════════════
        // METODE PUBLICE - ADĂUGARE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Adaugă sau incrementează produsul în bonul curent
        /// ✅ MODIFICAT: Adaugă automat garanția SGR dacă produsul are CodSGR setat
        /// </summary>
        /// <param name="produs">Produsul de adăugat</param>
        /// <param name="cantitate">Cantitatea (default: 1)</param>
        /// <returns>BonItem adăugat sau actualizat</returns>
        /// <summary>
        /// Adaugă sau incrementează produsul în bonul curent
        /// ✅ Adaugă garanția SGR DOAR dacă ENABLED_SGR = 1
        /// </summary>
        public BonItem AdaugaProdus(Produs produs, decimal cantitate = 1)
        {
            if (produs == null)
                throw new ArgumentNullException(nameof(produs));

            if (cantitate <= 0)
                throw new ArgumentException("Cantitatea trebuie să fie pozitivă", nameof(cantitate));

            var itemExistent = Items.FirstOrDefault(x => x.IdProdus == produs.Id &&
                                                         x.GarantiePentruProdusId == null);

            if (itemExistent != null)
            {
                var cantitateBefore = itemExistent.Cantitate;
                itemExistent.Cantitate += cantitate;

                Logs.Write($"  → Cantitate actualizată pentru '{itemExistent.Nume}': {cantitateBefore} → {itemExistent.Cantitate}");

                // ═══════════════════════════════════════════════════════════════
                // ✅ VERIFICĂ DACĂ SGR ESTE ACTIVAT ÎN CONFIGURAȚIE
                // ═══════════════════════════════════════════════════════════════
                if (_config.IsSGREnabled && !string.IsNullOrWhiteSpace(produs.CodSGR))
                {
                    SincronizeazaCantitateGarantieCuProdus(itemExistent);
                }

                CantitateModificata?.Invoke(this, itemExistent);
                return itemExistent;
            }
            else
            {
                var bonItem = new BonItem
                {
                    IdProdus = produs.Id,
                    Nume = produs.Denumire,
                    Cantitate = cantitate,
                    ValoareTva = produs.ValoareTva,
                    PretBrut = produs.PretBrut,
                    ProcentTva = produs.ProcentTva,
                    TvaId = produs.TvaId,
                    CodSGR = produs.CodSGR,
                    UnitateMasura = produs.UnitateMasura,
                    Departament = produs.Departament,
                    GarantiePentruProdusId = null
                };

                bonItem.PropertyChanged += BonItem_PropertyChanged;
                Items.Add(bonItem);

                Logs.Write($"✅ Produs NOU adăugat: {bonItem.Nume} - {bonItem.Cantitate} × {bonItem.PretBrut:F2} LEI = {bonItem.Total:F2} LEI");

                // ═══════════════════════════════════════════════════════════════
                // ✅ ADAUGĂ GARANȚIE DOAR DACĂ SGR ESTE ACTIVAT
                // ═══════════════════════════════════════════════════════════════
                if (_config.IsSGREnabled && !string.IsNullOrWhiteSpace(produs.CodSGR))
                {
                    AdaugaGarantieDupaProdus(bonItem);
                }
                else if (!string.IsNullOrWhiteSpace(produs.CodSGR))
                {
                    Logs.Write($"  ℹ️ SGR dezactivat în configurație - nu se adaugă garanție pentru '{bonItem.Nume}'");
                }

                ProdusAdaugat?.Invoke(this, bonItem);
                return bonItem;
            }
        }

        /// <summary>
        /// Adaugă garanția SGR IMEDIAT DUPĂ produsul specificat
        /// </summary>
        /// <param name="produs">Produsul pentru care se adaugă garanția</param>
        private void AdaugaGarantieDupaProdus(BonItem produs)
        {
            if (produs == null || string.IsNullOrWhiteSpace(produs.CodSGR))
                return;

            try
            {
                // ✅ Convertește CodSGR în ID
                if (!int.TryParse(produs.CodSGR, out int idProdusGarantie))
                {
                    Logs.Write($"⚠️ CodSGR invalid: '{produs.CodSGR}'");
                    return;
                }

                // ✅ Încarcă produsul garanție din DB
                var produsGarantie = _dbQuery.GetProdusDupaId(idProdusGarantie);

                if (produsGarantie == null)
                {
                    Logs.Write($"⚠️ Produsul garanție cu ID={idProdusGarantie} nu a fost găsit!");
                    return;
                }

                // ✅ Găsește poziția produsului în listă
                int indexProdus = Items.IndexOf(produs);

                if (indexProdus == -1)
                {
                    Logs.Write($"⚠️ Produsul '{produs.Nume}' nu a fost găsit în Items!");
                    return;
                }

                // ═══════════════════════════════════════════════════════════════
                // ✅ CREEAZĂ GARANȚIA CU ACEEAȘI CANTITATE CA PRODUSUL
                // ═══════════════════════════════════════════════════════════════
                var bonItemGarantie = new BonItem
                {
                    IdProdus = produsGarantie.Id,
                    Nume = produsGarantie.Denumire,  // Ex: "GARANȚIE SGR"
                    Cantitate = produs.Cantitate,    // ✅ Aceeași cantitate ca produsul
                    ValoareTva = produsGarantie.ValoareTva,
                    PretBrut = produsGarantie.PretBrut,
                    ProcentTva = produsGarantie.ProcentTva,
                    TvaId = produsGarantie.TvaId,
                    CodSGR = produsGarantie.CodSGR,
                    UnitateMasura = produsGarantie.UnitateMasura,
                    Departament = produsGarantie.Departament,
                    GarantiePentruProdusId = produs.IdProdus  // ✅ Link către produsul său
                };

                bonItemGarantie.PropertyChanged += BonItem_PropertyChanged;

                // ✅ INSEREAZĂ GARANȚIA IMEDIAT DUPĂ PRODUS (index + 1)
                Items.Insert(indexProdus + 1, bonItemGarantie);

                Logs.Write($"  ✅ Garanție SGR adăugată după '{produs.Nume}': {produsGarantie.Denumire} × {bonItemGarantie.Cantitate} = {bonItemGarantie.Total:F2} LEI");
            }
            catch (Exception ex)
            {
                Logs.Write($"❌ EROARE la adăugarea garanției pentru '{produs.Nume}':");
                Logs.Write(ex);
            }
        }


        /// <summary>
        /// Incrementează cantitatea unui articol
        /// ✅ Incrementează automat și garanția
        /// </summary>
        public bool IncrementeazaCantitate(BonItem item, decimal increment = 1)
        {
            if (item == null || !Items.Contains(item))
                return false;

            if (item.GarantiePentruProdusId != null)
            {
                Logs.Write("⚠️ Nu poți modifica manual garanția!");
                return false;
            }

            decimal cantitateBefore = item.Cantitate;
            item.Cantitate += increment;

            Logs.Write($"📝 Cantitate modificată pentru '{item.Nume}': {cantitateBefore} → {item.Cantitate}");

            // ✅ Sincronizează garanția DOAR dacă SGR este activat
            if (_config.IsSGREnabled && !string.IsNullOrWhiteSpace(item.CodSGR))
            {
                SincronizeazaCantitateGarantieCuProdus(item);
            }

            CantitateModificata?.Invoke(this, item);
            return true;
        }

        /// <summary>
        /// Decrementează cantitatea unui articol
        /// ✅ Decrementează automat și garanția
        /// </summary>
        public bool DecrementeazaCantitate(BonItem item, decimal decrement = 1)
        {
            if (item == null || !Items.Contains(item))
                return false;

            if (item.GarantiePentruProdusId != null)
            {
                Logs.Write("⚠️ Nu poți modifica manual garanția!");
                return false;
            }

            decimal cantitateNoua = item.Cantitate - decrement;

            if (cantitateNoua <= 0)
            {
                return StergeArticol(item);
            }

            decimal cantitateBefore = item.Cantitate;
            item.Cantitate = cantitateNoua;

            Logs.Write($"📝 Cantitate modificată pentru '{item.Nume}': {cantitateBefore} → {item.Cantitate}");

            // ✅ Sincronizează garanția DOAR dacă SGR este activat
            if (_config.IsSGREnabled && !string.IsNullOrWhiteSpace(item.CodSGR))
            {
                SincronizeazaCantitateGarantieCuProdus(item);
            }

            CantitateModificata?.Invoke(this, item);
            return true;
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
        /// ✅ Sincronizează automat cantitatea garanției cu produsul
        /// </summary>
        private void BonItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BonItem.Total) ||
                e.PropertyName == nameof(BonItem.Cantitate) ||
                e.PropertyName == nameof(BonItem.PretBrut))
            {
                RecalculeazaTotal();

                // ✅ ELIMINĂ SINCRONIZAREA AUTOMATĂ
                // Garanția se sincronizează explicit în IncrementeazaCantitate/DecrementeazaCantitate

                if (sender is BonItem item && e.PropertyName == nameof(BonItem.Cantitate))
                {
                    CantitateModificata?.Invoke(this, item);
                }
            }
        }

        /// <summary>
        /// Sincronizează cantitatea garanției cu cantitatea produsului
        /// </summary>
        private void SincronizeazaCantitateGarantieCuProdus(BonItem produs)
        {
            if (produs == null || string.IsNullOrWhiteSpace(produs.CodSGR))
                return;

            // ✅ NU sincroniza dacă acest item ESTE o garanție
            if (produs.GarantiePentruProdusId != null)
                return;

            try
            {
                if (!int.TryParse(produs.CodSGR, out int idProdusGarantie))
                {
                    Logs.Write($"⚠️ CodSGR invalid pentru sincronizare: '{produs.CodSGR}'");
                    return;
                }

                // ═══════════════════════════════════════════════════════════════
                // ✅ GĂSEȘTE GARANȚIA ACESTUI PRODUS SPECIFIC
                // Garanția trebuie să îndeplinească TOATE condițiile:
                // 1. Are IdProdus = idProdusGarantie (este o garanție)
                // 2. Are GarantiePentruProdusId = produs.IdProdus (e garanția ACESTUI produs)
                // 3. Vine imediat după produs în listă
                // ═══════════════════════════════════════════════════════════════

                int indexProdus = Items.IndexOf(produs);

                if (indexProdus == -1)
                {
                    Logs.Write($"⚠️ Produsul '{produs.Nume}' nu a fost găsit în Items pentru sincronizare!");
                    return;
                }

                // ✅ Verifică dacă există item după produs
                if (indexProdus + 1 >= Items.Count)
                {
                    Logs.Write($"⚠️ Nu există garanție după '{produs.Nume}' (index={indexProdus}, Count={Items.Count})");
                    return;
                }

                var garantieCandidata = Items[indexProdus + 1];

                // ✅ VERIFICĂRI STRICTE pentru a fi siguri că este garanția ACESTUI produs
                bool esteGarantiaCeaCorecta =
                    garantieCandidata.IdProdus == idProdusGarantie &&                    // Este o garanție
                    garantieCandidata.GarantiePentruProdusId == produs.IdProdus;        // Este garanția ACESTUI produs

                if (!esteGarantiaCeaCorecta)
                {
                    Logs.Write($"⚠️ Item-ul după '{produs.Nume}' nu este garanția sa!");
                    Logs.Write($"   Expected: IdProdus={idProdusGarantie}, GarantiePentruProdusId={produs.IdProdus}");
                    Logs.Write($"   Got: IdProdus={garantieCandidata.IdProdus}, GarantiePentruProdusId={garantieCandidata.GarantiePentruProdusId}");
                    return;
                }

                // ✅ Sincronizează cantitatea
                if (garantieCandidata.Cantitate != produs.Cantitate)
                {
                    decimal garantieBefore = garantieCandidata.Cantitate;
                    garantieCandidata.Cantitate = produs.Cantitate;

                    Logs.Write($"  ✅ Garanție sincronizată pentru '{produs.Nume}': {garantieBefore} → {garantieCandidata.Cantitate}");
                }
                else
                {
                    Logs.Write($"  ℹ️ Garanție deja sincronizată pentru '{produs.Nume}': {garantieCandidata.Cantitate}");
                }
            }
            catch (Exception ex)
            {
                Logs.Write($"❌ EROARE la sincronizarea garanției pentru '{produs.Nume}':");
                Logs.Write(ex);
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
