// Fișier: Models/SmConfig.cs

using System.ComponentModel;
namespace SysManager.Models
{
    /// <summary>
    /// Setări generale pentru aplicație din tabela SM_CONFIG
    /// </summary>
    public class SmConfig : INotifyPropertyChanged
    {
        private short _id;
        /// <summary>
        /// ID-ul configurației (de obicei 1)
        /// </summary>
        public short Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        private string _imprimantaNota;
        /// <summary>
        /// Numele imprimantei pentru note
        /// </summary>
        public string ImprimantaNota
        {
            get => _imprimantaNota;
            set
            {
                if (_imprimantaNota != value)
                {
                    _imprimantaNota = value;
                    OnPropertyChanged(nameof(ImprimantaNota));
                }
            }
        }

        private short _enabledSound;
        /// <summary>
        /// Sunet activat: 1 = da, -1 = nu
        /// </summary>
        public short EnabledSound
        {
            get => _enabledSound;
            set
            {
                if (_enabledSound != value)
                {
                    _enabledSound = value;
                    OnPropertyChanged(nameof(EnabledSound));
                    OnPropertyChanged(nameof(IsSoundEnabled));
                }
            }
        }

        /// <summary>
        /// Helper: Verifică dacă sunetul este activat
        /// </summary>
        public bool IsSoundEnabled => EnabledSound == 1;

        private short _sellNegativeStock;
        /// <summary>
        /// Permite vânzare cu stoc negativ: 1 = da, -1 = nu
        /// </summary>
        public short SellNegativeStock
        {
            get => _sellNegativeStock;
            set
            {
                if (_sellNegativeStock != value)
                {
                    _sellNegativeStock = value;
                    OnPropertyChanged(nameof(SellNegativeStock));
                    OnPropertyChanged(nameof(CanSellNegativeStock));
                }
            }
        }

        /// <summary>
        /// Helper: Verifică dacă se permite vânzare cu stoc negativ
        /// </summary>
        public bool CanSellNegativeStock => SellNegativeStock == 1;

        private short _cumuleazaArticoleVandute;
        /// <summary>
        /// Cumuleaza articole vandute: 1 = da, -1 = nu
        /// </summary>
        public short CumuleazaArticoleVandute
        {
            get => _cumuleazaArticoleVandute;
            set
            {
                if (_cumuleazaArticoleVandute != value)
                {
                    _cumuleazaArticoleVandute = value;
                    OnPropertyChanged(nameof(CumuleazaArticoleVandute));
                    OnPropertyChanged(nameof(ShouldCumuleazaArticole));
                }
            }
        }

        /// <summary>
        /// Helper: Verifică dacă se cumuleaza articolele
        /// </summary>
        public bool ShouldCumuleazaArticole => CumuleazaArticoleVandute == 1;

        private int _delComNotaPrint;
        /// <summary>
        /// Șterge comandă după printare notă
        /// </summary>
        public int DelComNotaPrint
        {
            get => _delComNotaPrint;
            set
            {
                if (_delComNotaPrint != value)
                {
                    _delComNotaPrint = value;
                    OnPropertyChanged(nameof(DelComNotaPrint));
                }
            }
        }

        private string _imprimantaImplicita;
        /// <summary>
        /// Imprimanta implicită
        /// </summary>
        public string ImprimantaImplicita
        {
            get => _imprimantaImplicita;
            set
            {
                if (_imprimantaImplicita != value)
                {
                    _imprimantaImplicita = value;
                    OnPropertyChanged(nameof(ImprimantaImplicita));
                }
            }
        }

        private string _imprimantaImplicitaNota;
        /// <summary>
        /// Imprimanta implicită pentru note
        /// </summary>
        public string ImprimantaImplicitaNota
        {
            get => _imprimantaImplicitaNota;
            set
            {
                if (_imprimantaImplicitaNota != value)
                {
                    _imprimantaImplicitaNota = value;
                    OnPropertyChanged(nameof(ImprimantaImplicitaNota));
                }
            }
        }

        private short _enabledSGR;
        /// <summary>
        /// Garanție SGR activată: 1 = da (adaugă garanție), -1 = nu (nu adaugă garanție)
        /// </summary>
        public short EnabledSGR
        {
            get => _enabledSGR;
            set
            {
                if (_enabledSGR != value)
                {
                    _enabledSGR = value;
                    OnPropertyChanged(nameof(EnabledSGR));
                    OnPropertyChanged(nameof(IsSGREnabled));
                }
            }
        }

        /// <summary>
        /// Helper: Verifică dacă garanția SGR este activată
        /// ✅ FOARTE IMPORTANT: Aceasta determină dacă se adaugă garanția SGR automat
        /// </summary>
        public bool IsSGREnabled => EnabledSGR == 1;

        // ═══════════════════════════════════════════════════════════════
        // INotifyPropertyChanged Implementation
        // ═══════════════════════════════════════════════════════════════

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ═══════════════════════════════════════════════════════════════
        // Metode utile
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Returnează reprezentarea text a configurației
        /// </summary>
        public override string ToString()
        {
            return $"SmConfig[Id={Id}, SGR={(IsSGREnabled ? "ENABLED" : "DISABLED")}, " +
                   $"Sound={(IsSoundEnabled ? "ON" : "OFF")}, " +
                   $"NegativeStock={(CanSellNegativeStock ? "YES" : "NO")}]";
        }
    }
}