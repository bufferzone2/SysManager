using System.ComponentModel;

namespace SysManager.Models
{
    /// <summary>
    /// Reprezintă un articol (produs) din bonul fiscal
    /// </summary>
    public class BonItem : INotifyPropertyChanged
    {
        // ═══════════════════════════════════════════════════════════════
        // PROPRIETĂȚI PRINCIPALE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID-ul produsului din baza de date
        /// </summary>
        public int IdProdus { get; set; }

        private string _nume;
        /// <summary>
        /// Denumirea produsului
        /// </summary>
        public string Nume
        {
            get => _nume;
            set
            {
                if (_nume != value)
                {
                    _nume = value;
                    OnPropertyChanged(nameof(Nume));
                }
            }
        }

        private decimal _cantitate;
        /// <summary>
        /// Cantitatea comandată
        /// </summary>
        public decimal Cantitate
        {
            get => _cantitate;
            set
            {
                if (_cantitate != value)
                {
                    _cantitate = value;
                    OnPropertyChanged(nameof(Cantitate));
                    OnPropertyChanged(nameof(Total)); // ✅ Recalculează automat totalul
                }
            }
        }

        private decimal _pret;
        /// <summary>
        /// Prețul unitar cu TVA
        /// </summary>
        public decimal Pret
        {
            get => _pret;
            set
            {
                if (_pret != value)
                {
                    _pret = value;
                    OnPropertyChanged(nameof(Pret));
                    OnPropertyChanged(nameof(Total)); // ✅ Recalculează automat totalul
                }
            }
        }

        /// <summary>
        /// Totalul pentru această linie (Cantitate × Preț)
        /// </summary>
        public decimal Total => Cantitate * Pret;

        // ═══════════════════════════════════════════════════════════════
        // PROPRIETĂȚI SUPLIMENTARE (TVA, DEPARTAMENT, ETC.)
        // ═══════════════════════════════════════════════════════════════

        private decimal _pretBrut;
        /// <summary>
        /// Prețul fără TVA
        /// </summary>
        public decimal PretBrut
        {
            get => _pretBrut;
            set
            {
                if (_pretBrut != value)
                {
                    _pretBrut = value;
                    OnPropertyChanged(nameof(PretBrut));
                }
            }
        }

        private decimal _tvaValoare;
        /// <summary>
        /// Valoarea TVA pe unitate
        /// </summary>
        public decimal TvaValoare
        {
            get => _tvaValoare;
            set
            {
                if (_tvaValoare != value)
                {
                    _tvaValoare = value;
                    OnPropertyChanged(nameof(TvaValoare));
                    OnPropertyChanged(nameof(TotalTVA));
                }
            }
        }

        /// <summary>
        /// Total TVA pentru această linie
        /// </summary>
        public decimal TotalTVA => TvaValoare * Cantitate;

        private int? _tvaId;
        /// <summary>
        /// ID-ul cotei de TVA
        /// </summary>
        public int? TvaId
        {
            get => _tvaId;
            set
            {
                if (_tvaId != value)
                {
                    _tvaId = value;
                    OnPropertyChanged(nameof(TvaId));
                }
            }
        }

        private string _codSGR;
        /// <summary>
        /// Codul SGR (Sistem Garanție-Returnare) pentru ambalaje
        /// </summary>
        public string CodSGR
        {
            get => _codSGR;
            set
            {
                if (_codSGR != value)
                {
                    _codSGR = value;
                    OnPropertyChanged(nameof(CodSGR));
                }
            }
        }

        private string _unitateMasura;
        /// <summary>
        /// Unitatea de măsură (buc, kg, l, etc.)
        /// </summary>
        public string UnitateMasura
        {
            get => _unitateMasura;
            set
            {
                if (_unitateMasura != value)
                {
                    _unitateMasura = value;
                    OnPropertyChanged(nameof(UnitateMasura));
                }
            }
        }

        private int _departament;
        /// <summary>
        /// ID-ul departamentului produsului
        /// </summary>
        public int Departament
        {
            get => _departament;
            set
            {
                if (_departament != value)
                {
                    _departament = value;
                    OnPropertyChanged(nameof(Departament));
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // PROPRIETĂȚI OPȚIONALE SUPLIMENTARE
        // ═══════════════════════════════════════════════════════════════

        private string _codBare;
        /// <summary>
        /// Codul de bare al produsului
        /// </summary>
        public string CodBare
        {
            get => _codBare;
            set
            {
                if (_codBare != value)
                {
                    _codBare = value;
                    OnPropertyChanged(nameof(CodBare));
                }
            }
        }

        private decimal _discount;
        /// <summary>
        /// Discount aplicat (procent sau valoare fixă)
        /// </summary>
        public decimal Discount
        {
            get => _discount;
            set
            {
                if (_discount != value)
                {
                    _discount = value;
                    OnPropertyChanged(nameof(Discount));
                    OnPropertyChanged(nameof(TotalCuDiscount));
                }
            }
        }

        /// <summary>
        /// Total după aplicarea discountului
        /// </summary>
        public decimal TotalCuDiscount => Total - Discount;

        private string _observatii;
        /// <summary>
        /// Observații sau note pentru acest articol
        /// </summary>
        public string Observatii
        {
            get => _observatii;
            set
            {
                if (_observatii != value)
                {
                    _observatii = value;
                    OnPropertyChanged(nameof(Observatii));
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

        // ═══════════════════════════════════════════════════════════════
        // METODE UTILE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Returnează reprezentarea text a articolului
        /// </summary>
        public override string ToString()
        {
            return $"{Nume} - {Cantitate} × {Pret:F2} LEI = {Total:F2} LEI";
        }

        /// <summary>
        /// Clonează articolul curent
        /// </summary>
        public BonItem Clone()
        {
            return new BonItem
            {
                IdProdus = this.IdProdus,
                Nume = this.Nume,
                Cantitate = this.Cantitate,
                Pret = this.Pret,
                PretBrut = this.PretBrut,
                TvaValoare = this.TvaValoare,
                TvaId = this.TvaId,
                CodSGR = this.CodSGR,
                UnitateMasura = this.UnitateMasura,
                Departament = this.Departament,
                CodBare = this.CodBare,
                Discount = this.Discount,
                Observatii = this.Observatii
            };
        }
    }
}
