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

        private decimal _valoaretva;
        /// <summary>
        /// Valoare tva produs
        /// </summary>
        public decimal ValoareTva
        {
            get => _valoaretva;
            set
            {
                if (_valoaretva != value)
                {
                    _valoaretva = value;
                    OnPropertyChanged(nameof(ValoareTva));
                }
            }
        }


        // ═══════════════════════════════════════════════════════════════
        // PROPRIETĂȚI SUPLIMENTARE (TVA, DEPARTAMENT, ETC.)
        // ═══════════════════════════════════════════════════════════════

        private decimal _pretBrut;
        /// <summary>
        /// Prețul cu TVA
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
                    OnPropertyChanged(nameof(Total)); // ✅ Recalculează automat totalul
                }
            }
        }

        /// <summary>
        /// Totalul pentru această linie (Cantitate × PretBrut)
        /// </summary>
        public decimal Total => Cantitate * PretBrut;


        private decimal _procentTva;
        /// <summary>
        /// Valoarea TVA pe unitate
        /// </summary>
        public decimal ProcentTva
        {
            get => _procentTva;
            set
            {
                if (_procentTva != value)
                {
                    _procentTva = value;
                    OnPropertyChanged(nameof(ProcentTva));
                }
            }
        }


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
            return $"{Nume} - {Cantitate} × {PretBrut:F2} LEI = {Total:F2} LEI";
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
                ValoareTva = this.ValoareTva,
                PretBrut = this.PretBrut,
                ProcentTva = this.ProcentTva,
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
