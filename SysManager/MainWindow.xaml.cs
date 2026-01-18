// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SysManager.Controls;
using SysManager.Models;
using SysManager.Managers;
using SysManager.Windows;

namespace SysManager
{
    /// <summary>
    /// Partial class - Logica principală MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #region === VARIABILE PRIVATE ===
        private int _numarBonuriInAsteptare = 0;


        private readonly DbQuery _dbQuery;
        private SmConfig _config; 

        private List<Gestiune> _gestiuni = new List<Gestiune>();
        private int _selectedGestiuneId = 0;
        private int _selectedGrupaId = 0;

        // SETĂRI DIN BAZA DE DATE
        private GrupeSettings _grupeSettings;
        private ProduseSettings _produseSettings;

        // VARIABILE PAGINARE GRUPE
        private readonly List<GroupButton> _allButtons = new List<GroupButton>();
        private int _currentPage = 0;
        private int _totalPages = 0;
        private int _buttonsPerPage = 0;
        private int _columnsPerRow = 0;
        private int _rowsPerPage = 0;

        // VARIABILE PAGINARE PRODUSE
        private readonly List<POSButton> _allProductButtons = new List<POSButton>();
        private int _currentProductPage = 0;
        private int _totalProductPages = 0;
        private int _productsPerPage = 0;
        private int _productColumns = 0;
        private int _productRows = 0;

        private BonManager _bonManager;
        private BonuriAsteptareManager _bonuriAsteptareManager;
        private BonAsteptare _bonCurent; // Bonul curent activ (inclusiv cel reîncărcat

        private SearchKeypadWindow _activeKeyboardWindow = null;

        #endregion

        #region === INIȚIALIZARE ===

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            Logs.Write("MainWindow: Start App");

            this.KeyDown += MainWindow_KeyDown;
            _dbQuery = new DbQuery();

            DataInchiderii.Text = DateTime.Now.ToString("dd.MM.yyyy");

            this.Loaded += MainWindow_Loaded;
            this.SizeChanged += MainWindow_SizeChanged;

            ProductsPanel.SizeChanged += ProductsPanel_SizeChanged;
            _bonuriAsteptareManager = new BonuriAsteptareManager();

            // ❌ ȘTERGE ACEST APEL DIN CONSTRUCTOR:
            // InitializeazaBonManager();

            InitializeSearchTimer();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                // ✅ PAS 1: ÎNCARCĂ SETĂRILE DIN BAZA DE DATE
                _grupeSettings = _dbQuery.GetGrupeSettings();
                _produseSettings = _dbQuery.GetProduseSettings();

                // ✅ PAS 2: ÎNCARCĂ CONFIGURAȚIA (ÎNAINTE DE BonManager!)
                _config = _dbQuery.GetConfig();

                if (_config == null)
                {
                    //Logs.Write("⚠️ WARNING: Configurația nu a fost găsită, folosim valori default");
                    _config = new SmConfig
                    {
                        Id = 1,
                        EnabledSGR = -1,  // ✅ Default: SGR DEZACTIVAT
                        EnabledSound = 1,
                        SellNegativeStock = 1,
                        CumuleazaArticoleVandute = 1
                    };
                }

                //Logs.Write($"✅ Configurație încărcată: {_config}");

                // ✅ PAS 3: INIȚIALIZEAZĂ BonManager CU CONFIGURAȚIA ÎNCĂRCATĂ
                InitializeazaBonManager();

                // ✅ SETEAZĂ ÎNĂLȚIMEA RÂNDULUI PENTRU GRUPE
                if (_grupeSettings.PanouHeight > 0)
                {
                    this.MainGrid.RowDefinitions[1].Height = new GridLength(_grupeSettings.PanouHeight);
                    //Logs.Write($"MainWindow_Loaded: Rând Grupe setat la {_grupeSettings.PanouHeight}px din DB");
                }
                else
                {
                    this.MainGrid.RowDefinitions[1].Height = GridLength.Auto;
                    //Logs.Write("MainWindow_Loaded: Rând Grupe setat pe Auto (PANOU_HEIGHT=0)");
                }

                // ✅ ÎNCARCĂ DATELE
                CalculateLayout();
                LoadGestiuni();
                LoadGroups();
                LoadProducts();

                // Actualizează badge-ul bonuri așteptare
                ActualizeazaBadgeBonuriAsteptare();

            }, System.Windows.Threading.DispatcherPriority.Loaded);

            VerificaImagine();

        }


        private void VerificaImagine()
        {
            Logs.Write("═══ VERIFICARE IMAGINE ═══");

            // Test 1: Verifică calea
            string cale1 = "/Resources/delete.png";
            string cale2 = "Resources/delete.png";
            string cale3 = "pack://application:,,,/Resources/delete.png";
            string cale4 = "pack://application:,,,/SysManager;component/Resources/delete.png";

            foreach (var cale in new[] { cale1, cale2, cale3, cale4 })
            {
                try
                {
                    var uri = new Uri(cale, UriKind.RelativeOrAbsolute);
                    var streamInfo = Application.GetResourceStream(uri);

                    if (streamInfo != null)
                    {
                        Logs.Write($"✅ GĂSIT: {cale}");
                        Logs.Write($"   Length: {streamInfo.Stream.Length} bytes");
                        streamInfo.Stream.Close();
                    }
                    else
                    {
                        Logs.Write($"❌ NU EXISTĂ: {cale}");
                    }
                }
                catch (Exception ex)
                {
                    Logs.Write($"❌ EROARE la {cale}: {ex.Message}");
                }
            }

            Logs.Write("═══ SFÂRȘIT VERIFICARE ═══");
        }


        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_allButtons.Count > 0 && _grupeSettings != null && e.PreviousSize.Width != e.NewSize.Width)
            {
                CalculateLayout();
                DisplayCurrentPage();
            }
        }

        // ✅ NOTA: Implementarea ProductsPanel_SizeChanged() este în MainWindow.Products.cs
        // Partial class permite ca event handler-ul să fie definit în alt fișier

        #endregion

        #region === GESTIUNI ===

        private void LoadGestiuni()
        {
            try
            {
                GestiuniPanel.Children.Clear();
                _gestiuni = _dbQuery.GetGestiuni();

                Logs.Write($"📦 LoadGestiuni: Încărcare gestiuni ({_gestiuni.Count} active în DB)");

                // ✅ BUTON "TOATE GESTIUNILE"
                var btnAllGestiuni = new GestiuneButton
                {
                    GestiuneId = 0,
                    GestiuneName = "TOATE GESTIUNILE",
                    GestiuneColor = new SolidColorBrush(Color.FromRgb(0, 82, 163)),
                    Width = 160,
                    Height = 55,
                    Margin = new Thickness(0),
                    IsSelected = true,
                    Tag = new Gestiune { Id = 0, Nume = "TOATE GESTIUNILE", Status = 1 }
                };
                btnAllGestiuni.Click += GestiuneButton_Click;
                GestiuniPanel.Children.Add(btnAllGestiuni);

                _selectedGestiuneId = 0;

                // ✅ BUTOANE GESTIUNI
                foreach (var gestiune in _gestiuni)
                {
                    var btn = new GestiuneButton
                    {
                        GestiuneId = gestiune.Id,
                        GestiuneName = gestiune.DisplayName,
                        GestiuneColor = new SolidColorBrush(Color.FromRgb(0, 102, 204)),
                        Width = 120,
                        Height = 55,
                        Margin = new Thickness(0),
                        Tag = gestiune
                    };

                    btn.Click += GestiuneButton_Click;
                    GestiuniPanel.Children.Add(btn);
                }

                Logs.Write($"✅ Încărcate {GestiuniPanel.Children.Count} butoane gestiuni");
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE la încărcarea gestiunilor:");
                Logs.Write(ex);
            }
        }

        private void GestiuneButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is GestiuneButton btn && btn.Tag is Gestiune gestiune)
                {
                    // Deselectează toate
                    foreach (var child in GestiuniPanel.Children)
                    {
                        if (child is GestiuneButton gBtn)
                            gBtn.IsSelected = false;
                    }

                    // Selectează curent
                    btn.IsSelected = true;
                    _selectedGestiuneId = gestiune.Id;

                    if (gestiune.Id == 0)
                    {
                        //Logs.Write($"📦 Selectat: TOATE GESTIUNILE");
                        StatusText.Text = "Afișare: Toate gestiunile";
                    }
                    else
                    {
                        //Logs.Write($"📦 Gestiune selectată: {gestiune.DisplayName} (ID: {gestiune.Id})");
                        StatusText.Text = $"Gestiune: {gestiune.DisplayName}";
                    }

                    LoadGroups();
                    LoadProducts();
                }
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE la selectarea gestiunii:");
                Logs.Write(ex);
            }
        }

        #endregion

        #region === BON & ALTE FUNCȚII ===

        /// <summary>
        /// Click pe TextBox cantitate - deschide tastatura numerică CU UPDATE ÎN TIMP REAL
        /// </summary>
        private void TxtCantitateBon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // ✅ PREVINE COMPORTAMENTUL DEFAULT AL MOUSE-ULUI
            e.Handled = true;

            try
            {
                // Citește valoarea curentă din TextBox
                decimal valoareCurenta = 1;
                if (!string.IsNullOrWhiteSpace(TxtCantitateBon.Text))
                {
                    if (decimal.TryParse(TxtCantitateBon.Text.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal val))
                    {
                        valoareCurenta = val;
                    }
                }
                Logs.Write($"TxtCantitateBon_MouseDown: Valoare curentă = {valoareCurenta}");

                // ✅ CALCULEAZĂ POZIȚIA PENTRU A AFIȘA FEREASTRA SUB TEXTBOX
                Point textBoxPosition = TxtCantitateBon.PointToScreen(new Point(0, 0));
                Point keypadPosition = new Point(
                    textBoxPosition.X,
                    textBoxPosition.Y + TxtCantitateBon.ActualHeight + 5
                );

                // Verifică să nu iasă fereastra în afara ecranului
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                double keypadWidth = 330;
                double keypadHeight = 330;

                if (keypadPosition.X + keypadWidth > screenWidth)
                {
                    keypadPosition.X = screenWidth - keypadWidth - 10;
                }

                if (keypadPosition.Y + keypadHeight > screenHeight)
                {
                    keypadPosition.Y = textBoxPosition.Y - keypadHeight - 5;
                }

                // ✅ DESCHIDE TASTATURA NUMERICĂ
                decimal? nouaCantitate = Windows.NumericKeypadWindow.ShowDialog(
                    this,
                    TxtCantitateBon,
                    valoareCurenta,
                    "INTRODUCERE CANTITATE",
                    keypadPosition
                );

                if (nouaCantitate.HasValue)
                {
                    TxtCantitateBon.Text = nouaCantitate.Value.ToString("F3");
                    Logs.Write($"✅ Cantitate confirmată: {nouaCantitate.Value}");
                    StatusText.Text = $"Cantitate setată: {nouaCantitate.Value}";
                }
                else
                {
                    Logs.Write("❌ Introducere cantitate anulată de utilizator");
                }
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE la deschiderea tastaturii numerice:");
                Logs.Write(ex);
                MessageBox.Show($"Eroare la deschiderea tastaturii:\n{ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Când TextBox-ul primește focus, selectează tot textul
        /// </summary>
        private void TxtCantitateBon_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Selectează tot textul
                textBox.SelectAll();

                // Previne re-selecția când dai click cu mouse-ul
                // (altfel click-ul ar deselecta textul imediat)
                //e.Handled = true;
            }
        }

        /// <summary>
        /// Click pe TextBox căutare articol - deschide tastatura alfanumerică
        /// </summary>
        private void TxtCautareArticolBon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Previne comportamentul default
            e.Handled = true;

            try
            {
                // Citește textul curent din TextBox
                string textCurent = TxtCautareArticolBon.Text ?? "";

                Logs.Write($"TxtCautareArticolBon_MouseDown: Text curent = '{textCurent}'");

                // ✅ CALCULEAZĂ POZIȚIA PENTRU A AFIȘA FEREASTRA SUB TEXTBOX
                Point textBoxPosition = TxtCautareArticolBon.PointToScreen(new Point(0, 0));
                Point keypadPosition = new Point(
                    textBoxPosition.X - 150,  // Centrat aproximativ (720px lățime / 2 - 360)
                    textBoxPosition.Y + TxtCautareArticolBon.ActualHeight + 5
                );

                // Verifică să nu iasă fereastra în afara ecranului
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                double keypadWidth = 720;
                double keypadHeight = 380;

                if (keypadPosition.X < 0)
                {
                    keypadPosition.X = 10;
                }

                if (keypadPosition.X + keypadWidth > screenWidth)
                {
                    keypadPosition.X = screenWidth - keypadWidth - 10;
                }

                if (keypadPosition.Y + keypadHeight > screenHeight)
                {
                    // Afișează deasupra TextBox-ului în loc de dedesubt
                    keypadPosition.Y = textBoxPosition.Y - keypadHeight - 5;
                }

                // Creează fereastra cu poziție customizată
                var window = new Windows.SearchKeypadWindow(textCurent, TxtCautareArticolBon)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = keypadPosition.X,
                    Top = keypadPosition.Y
                };

                // ✅ DESCHIDE TASTATURA ALFANUMERICĂ
                bool? result = window.ShowDialog();

                if (result == true && window.EnteredText != null)
                {
                    TxtCautareArticolBon.Text = window.EnteredText;
                    Logs.Write($"✅ Text căutare confirmat: '{window.EnteredText}'");
                    StatusText.Text = $"Căutare: {window.EnteredText}";

                    // ✅ CĂUTAREA SE VA EXECUTA AUTOMAT PRIN TxtCautareArticolBon_TextChanged
                    // Nu mai e nevoie să apelăm ExecuteSearch() manual!
                }
                else
                {
                    Logs.Write("❌ Căutare anulată de utilizator");
                }
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE la deschiderea tastaturii de căutare:");
                Logs.Write(ex);
                MessageBox.Show($"Eroare la deschiderea tastaturii:\n{ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _bonManager.GolesteBon();
            TotalText.Text = "0.00";
            StatusText.Text = "Bon anulat";
            //Logs.Write("MainWindow: Bon anulat");
        }

        private void IncarcaBonAsteptare_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Procesare încasare...";
            Logs.Write("MainWindow: Încasare inițiată");
            var bonuri = _bonuriAsteptareManager.GetBonuriInAsteptare();
            var dialog = new BonuriAsteptareWindow(bonuri, _bonuriAsteptareManager);
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                IncarcaBonInGrid(dialog.BonSelectat);
            }
        }

        /// <summary>
        /// Salvează bonul curent în așteptare
        /// </summary>
        private void SalveazaBonAsteptare_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_bonManager.EsteGol)
                {
                    MessageBox.Show("Bonul este gol. Adaugă produse înainte de a-l salva în așteptare.",
                        "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var bon = new BonAsteptare
                {
                    NrBon = GenerareNrBon(),
                    DataCreare = DateTime.Now,
                    Total = _bonManager.Total,
                    Observatii = ""
                };

                // ✅ MODIFICĂ AICI - adaugă EsteGarantie
                foreach (var bonItem in _bonManager.Items)
                {
                    bon.Detalii.Add(new BonAsteptareDetaliu
                    {
                        IdProdus = bonItem.IdProdus,
                        DenumireProdus = bonItem.Nume,
                        Cantitate = bonItem.Cantitate,
                        PretUnitar = bonItem.PretBrut,
                        Valoare = bonItem.Total,
                        EsteGarantie = bonItem.GarantiePentruProdusId != null  // ← ADAUGĂ ACEST RÂND
                    });
                }

                int bonId = _bonuriAsteptareManager.SalveazaBonInAsteptare(bon);

                MessageBox.Show($"Bonul #{bon.NrBon} a fost salvat în așteptare!",
                    "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

                _bonManager.GolesteBon();
                TotalText.Text = "0.00";
                _bonCurent = null;

                Logs.Write($"Bon #{bon.NrBon} salvat în așteptare (ID: {bonId})");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la salvarea bonului în așteptare: {ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Logs.Write("EROARE salvare bon în așteptare:");
                Logs.Write(ex);
            }
        }

        /// <summary>
        /// Click pe butonul Bon Așteptare - LOGICĂ INTELIGENTĂ
        /// - Dacă bonul curent ARE produse → SALVEAZĂ în așteptare
        /// - Dacă bonul curent este GOL → DESCHIDE listă bonuri
        /// </summary>
        private void BtnBonAsteptare_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ═══════════════════════════════════════════════════════════════
                // VERIFICĂ DACĂ BONUL CURENT ARE PRODUSE
                // ═══════════════════════════════════════════════════════════════

                if (!_bonManager.EsteGol)
                {
                    // ✅ BONUL ARE PRODUSE → SALVEAZĂ ÎN AȘTEPTARE
                    Logs.Write("BtnBonAsteptare_Click: Bonul curent are produse → Salvare în așteptare");
                    SalveazaBonulCurentInAsteptare();
                }
                else
                {
                    // ✅ BONUL ESTE GOL → DESCHIDE LISTĂ BONURI
                    Logs.Write("BtnBonAsteptare_Click: Bonul curent este gol → Deschidere listă bonuri");
                    DeschideListaBonuriAsteptare();
                }
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE în BtnBonAsteptare_Click:");
                Logs.Write(ex);
                MessageBox.Show($"Eroare: {ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Salvează bonul curent în așteptare
        /// </summary>
        private void SalveazaBonulCurentInAsteptare()
        {
            try
            {
                Logs.Write("SalveazaBonulCurentInAsteptare: Începe salvarea");

                var bon = new BonAsteptare
                {
                    NrBon = GenerareNrBon(),
                    DataCreare = DateTime.Now,
                    Total = _bonManager.Total,
                    Observatii = ""
                };

                // ✅ Adaugă detaliile bonului
                foreach (var bonItem in _bonManager.Items)
                {
                    bon.Detalii.Add(new BonAsteptareDetaliu
                    {
                        IdProdus = bonItem.IdProdus,
                        DenumireProdus = bonItem.Nume,
                        Cantitate = bonItem.Cantitate,
                        PretUnitar = bonItem.PretBrut,
                        Valoare = bonItem.Total,
                        EsteGarantie = bonItem.GarantiePentruProdusId != null
                    });
                }

                // ✅ Salvează în DB
                int bonId = _bonuriAsteptareManager.SalveazaBonInAsteptare(bon);

                // ✅ Afișează confirmare
                MessageBox.Show($"Bonul #{bon.NrBon} a fost salvat în așteptare!\n\n" +
                               $"Articole: {_bonManager.NumarArticole}\n" +
                               $"Total: {bon.Total:F2} LEI",
                    "Bon salvat", MessageBoxButton.OK, MessageBoxImage.Information);

                // ✅ Golește bonul curent
                _bonManager.GolesteBon();
                TotalText.Text = "0.00";
                _bonCurent = null;

                // ✅ Actualizează badge-ul
                ActualizeazaBadgeBonuriAsteptare();

                StatusText.Text = $"Bon #{bon.NrBon} salvat în așteptare";
                Logs.Write($"✅ Bon #{bon.NrBon} salvat în așteptare (ID: {bonId})");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la salvarea bonului în așteptare:\n\n{ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Logs.Write("❌ EROARE salvare bon în așteptare:");
                Logs.Write(ex);
            }
        }

        /// <summary>
        /// Deschide fereastra cu lista bonurilor în așteptare
        /// </summary>
        private void DeschideListaBonuriAsteptare()
        {
            try
            {
                Logs.Write("DeschideListaBonuriAsteptare: Încărcare bonuri");

                // ✅ Obține bonurile în așteptare
                var bonuri = _bonuriAsteptareManager.GetBonuriInAsteptare();

                if (bonuri.Count == 0)
                {
                    MessageBox.Show("Nu există bonuri în așteptare.",
                        "Informație", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // ✅ Deschide fereastra
                var dialog = new BonuriAsteptareWindow(bonuri, _bonuriAsteptareManager);
                dialog.Owner = this;

                if (dialog.ShowDialog() == true && dialog.BonSelectat != null)
                {
                    // ✅ Utilizatorul a selectat un bon
                    IncarcaBonInGrid(dialog.BonSelectat);

                    // ✅ Actualizează badge-ul (s-a putut șterge un bon)
                    ActualizeazaBadgeBonuriAsteptare();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la deschiderea listei bonuri:\n\n{ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Logs.Write("❌ EROARE deschidere listă bonuri:");
                Logs.Write(ex);
            }
        }

        /// <summary>
        /// Actualizează badge-ul cu numărul de bonuri în așteptare
        /// </summary>
        private void ActualizeazaBadgeBonuriAsteptare()
        {
            try
            {
                var bonuri = _bonuriAsteptareManager.GetBonuriInAsteptare();
                NumarBonuriInAsteptare = bonuri.Count;  // ✅ Asta e tot!
                Logs.Write($"📋 Bonuri așteptare: {NumarBonuriInAsteptare}");
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE:");
                Logs.Write(ex);
                NumarBonuriInAsteptare = 0;
            }
        }
        public int NumarBonuriInAsteptare
        {
            get { return _numarBonuriInAsteptare; }
            set
            {
                if (_numarBonuriInAsteptare != value)
                {
                    _numarBonuriInAsteptare = value;
                    OnPropertyChanged(); // ✅ Notifică UI-ul că s-a schimbat!
                    Logs.Write($"📋 NumarBonuriInAsteptare actualizat: {value}");
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Logs.Write("MainWindow: Închidere aplicație");
            Application.Current.Shutdown();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Exit_Click(sender, e);
            }
        }

        /// <summary>
        /// Încarcă un bon din așteptare în grid PRIN BONMANAGER
        /// </summary>
        private void IncarcaBonInGrid(BonAsteptare bon)
        {
            try
            {
                Logs.Write($"IncarcaBonInGrid: Începe încărcarea bonului #{bon.NrBon}");

                // ✅ Golește bonul
                _bonManager.GolesteBon();

                int produseAdaugate = 0;
                int garantiiOmise = 0;

                foreach (var detaliu in bon.Detalii)
                {
                    // ✅ OMITE GARANȚIILE - vor fi regenerate automat
                    if (detaliu.EsteGarantie)
                    {
                        garantiiOmise++;
                        Logs.Write($"  ⏭️  Omit garanție: {detaliu.DenumireProdus} × {detaliu.Cantitate}");
                        continue;
                    }

                    // ✅ ÎNCARCĂ PRODUSUL COMPLET DIN DB (cu CodSGR!)
                    var produs = _dbQuery.GetProdusDupaId(detaliu.IdProdus);

                    if (produs == null)
                    {
                        Logs.Write($"⚠️  Produsul ID={detaliu.IdProdus} nu a fost găsit în DB");

                        // Fallback: creează produs din datele salvate (fără garanție)
                        produs = new Produs
                        {
                            Id = detaliu.IdProdus,
                            Denumire = detaliu.DenumireProdus,
                            PretBrut = detaliu.PretUnitar,
                            CodSGR = null
                        };
                    }

                    // ✅ ADAUGĂ PRODUSUL - garanția se va adăuga automat dacă are CodSGR
                    _bonManager.AdaugaProdus(produs, detaliu.Cantitate);
                    produseAdaugate++;

                    Logs.Write($"  ✅ Produs adăugat: {produs.Denumire} × {detaliu.Cantitate}" +
                              (!string.IsNullOrWhiteSpace(produs.CodSGR) ? " (cu garanție SGR)" : ""));
                }

                // ✅ Actualizează totalul
                TotalText.Text = _bonManager.Total.ToString("F2");

                // Salvează referința bonului curent
                _bonCurent = bon;

                // ✅ ACTUALIZEAZĂ BADGE-UL (bonul a fost încărcat, posibil șters din așteptare)
                ActualizeazaBadgeBonuriAsteptare();

                StatusText.Text = $"Bon #{bon.NrBon} încărcat: {produseAdaugate} produse" +
                                 (garantiiOmise > 0 ? $", {garantiiOmise} garanții regenerate" : "");

                Logs.Write($"✅ Bon #{bon.NrBon} încărcat: {produseAdaugate} produse, {garantiiOmise} garanții regenerate");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea bonului: {ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Logs.Write("❌ EROARE la încărcarea bonului:");
                Logs.Write(ex);
            }
        }


        // ═══════════════════════════════════════════════════════════════
        // INIȚIALIZARE BON MANAGER
        // ═══════════════════════════════════════════════════════════════
        private void InitializeazaBonManager()
        {
            // ✅ PASEAZĂ _dbQuery ȘI _config LA BonManager
            _bonManager = new BonManager(_dbQuery, _config);
            BonGrid.ItemsSource = _bonManager.Items;

            // ✅ Ascultă evenimentul de modificare total
            _bonManager.TotalModificat += (s, total) =>
            {
                // Optional: update total display
            };

            // ✅ Ascultă evenimentele de adăugare/ștergere
            _bonManager.ProdusAdaugat += (s, item) =>
            {
                Logs.Write($"✅ EVENT: Produs adăugat - {item.Nume}");
                BonGrid.ScrollIntoView(item);
            };

            _bonManager.ProdusSters += (s, item) =>
            {
                Logs.Write($"🗑️ EVENT: Produs șters - {item.Nume}");
            };

            _bonManager.CantitateModificata += (s, item) =>
            {
                Logs.Write($"📝 EVENT: Cantitate modificată - {item.Nume}: {item.Cantitate}");
            };

            Logs.Write($"✅ BonManager inițializat cu configurație: SGR={(_config.IsSGREnabled ? "ACTIVAT" : "DEZACTIVAT")}");
        }

        /// <summary>
        /// Click pe butonul de tastatură - deschide/închide tastatura alfanumerică (TOGGLE)
        /// </summary>
        private void BtnKeyboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ✅ DACĂ TASTATURA E DESCHISĂ, O ÎNCHIDE
                if (_activeKeyboardWindow != null)
                {
                    Logs.Write("BtnKeyboard_Click: Închidere tastatură existentă");
                    _activeKeyboardWindow.Close();
                    _activeKeyboardWindow = null;

                    // ✅ RESTAUREAZĂ FOCUS-UL PE MAINWINDOW
                    this.Activate();
                    return;
                }

                // Citește textul curent din TextBox
                string textCurent = TxtCautareArticolBon.Text ?? "";

                Logs.Write($"BtnKeyboard_Click: Deschidere tastatură, text curent = '{textCurent}'");



                // ✅ CREEAZĂ FEREASTRA NON-MODAL
                _activeKeyboardWindow = new SysManager.Windows.SearchKeypadWindow(textCurent, TxtCautareArticolBon)
                {
                    Owner = this
                };

                // ✅ CÂND SE ÎNCHIDE, RESETEAZĂ REFERINȚA ȘI RESTAUREAZĂ FOCUS
                _activeKeyboardWindow.Closed += (s, args) =>
                {
                    _activeKeyboardWindow = null;

                    // ✅ RESTAUREAZĂ FOCUS-UL PE MAINWINDOW DUPĂ ÎNCHIDERE
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.Activate();
                        this.Focus();
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                    Logs.Write("SearchKeypadWindow: Fereastră închisă, focus restaurat");
                };

                // ✅ DESCHIDE FEREASTRA NON-MODAL (Show în loc de ShowDialog)
                _activeKeyboardWindow.Show();

                // ✅ ACTIVEAZĂ FEREASTRA TASTATURII
                _activeKeyboardWindow.Activate();

                Logs.Write("✅ Tastatură deschisă (non-modal)");
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE la deschiderea tastaturii de căutare:");
                Logs.Write(ex);
                MessageBox.Show($"Eroare la deschiderea tastaturii:\n{ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Închide tastatura de căutare dacă e deschisă
        /// Apelează această metodă în Product_Click ÎNAINTE de a adăuga produsul
        /// </summary>
        private void CloseSearchKeyboardIfOpen()
        {
            if (_activeKeyboardWindow != null)
            {
                Logs.Write("CloseSearchKeyboardIfOpen: Închidere tastatură înainte de adăugare produs");
                _activeKeyboardWindow.Close();
                _activeKeyboardWindow = null;
            }
        }

        /// <summary>
        /// Curăță interfața de căutare (închide tastatura și șterge textul)
        /// Apelează această metodă când utilizatorul selectează un produs
        /// </summary>
        private void ClearSearchInterface()
        {
            try
            {
                // ✅ ÎNCHIDE TASTATURA DE CĂUTARE DACĂ ESTE DESCHISĂ
                if (_activeKeyboardWindow != null)
                {
                    _activeKeyboardWindow.Close();
                    _activeKeyboardWindow = null;

                    // ✅ RESTAUREAZĂ FOCUS PE MAINWINDOW
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.Activate();
                        this.Focus();
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                }

                // ✅ ȘTERGE TEXTUL DIN CĂUTARE (dacă nu e gol)
                if (!string.IsNullOrWhiteSpace(TxtCautareArticolBon.Text))
                {
                    TxtCautareArticolBon.Text = "";
                }
            }
            catch (Exception ex)
            {
                Logs.Write($"⚠️ Eroare la curățarea interfeței de căutare: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler pentru butonul de scroll SUS
        /// Derulează BonGrid-ul în sus cu un pas fix
        /// </summary>
        private void BtnScrollUp_Click(object sender, RoutedEventArgs e)
        {
            if (BonScrollViewer != null)
            {
                // Derulează cu 3 rânduri în sus (3 × înălțime_rând = 3 × 35 = 105px)
                double scrollAmount = 105; // 3 rânduri
                double newOffset = Math.Max(0, BonScrollViewer.VerticalOffset - scrollAmount);
                BonScrollViewer.ScrollToVerticalOffset(newOffset);
            }
        }

        /// <summary>
        /// Event handler pentru butonul de scroll JOS
        /// Derulează BonGrid-ul în jos cu un pas fix
        /// </summary>
        private void BtnScrollDown_Click(object sender, RoutedEventArgs e)
        {
            if (BonScrollViewer != null)
            {
                // Derulează cu 3 rânduri în jos (3 × înălțime_rând = 3 × 35 = 105px)
                double scrollAmount = 105; // 3 rânduri
                double newOffset = Math.Min(
                    BonScrollViewer.ScrollableHeight,
                    BonScrollViewer.VerticalOffset + scrollAmount
                );
                BonScrollViewer.ScrollToVerticalOffset(newOffset);
            }
        }

        /// <summary>
        /// Buton + : Incrementează cantitatea articolului selectat cu valoarea din TxtCantitateBon
        /// </summary>
        private void BtnAddCant_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ✅ VERIFICĂ DACĂ BonManager E INIȚIALIZAT
                if (_bonManager == null)
                {
                    StatusText.Text = "⚠️ BonManager nu este inițializat!";
                    Logs.Write("⚠️ BtnAddCant_Click: BonManager null");
                    return;
                }

                // ✅ VERIFICĂ DACĂ EXISTĂ ARTICOLE ÎN BON
                if (_bonManager.EsteGol)
                {
                    StatusText.Text = "⚠️ Bonul este gol! Selectează un produs mai întâi.";
                    Logs.Write("⚠️ BtnAddCant_Click: Bon gol");
                    return;
                }

                // ✅ VERIFICĂ DACĂ E SELECTAT UN ARTICOL ÎN DATAGRID
                if (BonGrid.SelectedItem == null)
                {
                    StatusText.Text = "⚠️ Selectează un articol din bon!";
                    Logs.Write("⚠️ BtnAddCant_Click: Niciun articol selectat");
                    return;
                }

                var bonItemSelectat = BonGrid.SelectedItem as Models.BonItem;
                if (bonItemSelectat == null)
                {
                    StatusText.Text = "⚠️ Eroare la citirea articolului selectat!";
                    Logs.Write("⚠️ BtnAddCant_Click: Cast la BonItem eșuat");
                    return;
                }

                // ✅ CITEȘTE CANTITATEA DIN TxtCantitateBon
                decimal cantitateIncrement = 1; // valoare implicită

                if (!string.IsNullOrWhiteSpace(TxtCantitateBon.Text))
                {
                    // Suportă atât virgulă cât și punct ca separator zecimal
                    string cantitateText = TxtCantitateBon.Text.Replace(',', '.');

                    if (decimal.TryParse(cantitateText,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal cantitateInput))
                    {
                        if (cantitateInput > 0)
                        {
                            cantitateIncrement = cantitateInput;
                        }
                        else
                        {
                            StatusText.Text = "⚠️ Cantitatea trebuie să fie pozitivă!";
                            Logs.Write($"⚠️ BtnAddCant_Click: Cantitate invalidă ({cantitateInput})");
                            return;
                        }
                    }
                    else
                    {
                        StatusText.Text = "⚠️ Format cantitate invalid!";
                        Logs.Write($"⚠️ BtnAddCant_Click: Parse failed pentru '{TxtCantitateBon.Text}'");
                        return;
                    }
                }

                // ✅ INCREMENTEAZĂ CANTITATEA
                decimal cantitateBefore = bonItemSelectat.Cantitate;
                bool success = _bonManager.IncrementeazaCantitate(bonItemSelectat, cantitateIncrement);

                if (success)
                {
                    // ✅ Actualizează totalul în UI
                    TotalText.Text = _bonManager.Total.ToString("F2");

                    // ✅ Mesaj de succes
                    StatusText.Text = $"✅ Cantitate actualizată: {bonItemSelectat.Nume} - {cantitateBefore:F3} → {bonItemSelectat.Cantitate:F3}";

                    Logs.Write($"✅ BtnAddCant_Click: {bonItemSelectat.Nume} - cantitate {cantitateBefore:F3} → {bonItemSelectat.Cantitate:F3} (+{cantitateIncrement:F3})");

                    // ✅ OPȚIONAL: Resetează cantitatea la 1
                    TxtCantitateBon.Text = "1.000";
                }
                else
                {
                    StatusText.Text = "❌ Eroare la incrementarea cantității!";
                    Logs.Write($"❌ BtnAddCant_Click: IncrementeazaCantitate returnat false");
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "❌ EROARE la incrementarea cantității!";
                Logs.Write("❌ EROARE BtnAddCant_Click:");
                Logs.Write(ex);
                MessageBox.Show($"Eroare la incrementarea cantității:\n\n{ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Buton - : Decrementează cantitatea articolului selectat cu valoarea din TxtCantitateBon
        /// ✅ VALIDEAZĂ că decrementarea nu depășește cantitatea existentă
        /// </summary>
        private void BtnDelCant_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ✅ VERIFICĂ DACĂ BonManager E INIȚIALIZAT
                if (_bonManager == null)
                {
                    StatusText.Text = "⚠️ BonManager nu este inițializat!";
                    Logs.Write("⚠️ BtnDelCant_Click: BonManager null");
                    return;
                }

                // ✅ VERIFICĂ DACĂ EXISTĂ ARTICOLE ÎN BON
                if (_bonManager.EsteGol)
                {
                    StatusText.Text = "⚠️ Bonul este gol! Selectează un produs mai întâi.";
                    Logs.Write("⚠️ BtnDelCant_Click: Bon gol");
                    return;
                }

                // ✅ VERIFICĂ DACĂ E SELECTAT UN ARTICOL ÎN DATAGRID
                if (BonGrid.SelectedItem == null)
                {
                    StatusText.Text = "⚠️ Selectează un articol din bon!";
                    Logs.Write("⚠️ BtnDelCant_Click: Niciun articol selectat");
                    return;
                }

                var bonItemSelectat = BonGrid.SelectedItem as Models.BonItem;
                if (bonItemSelectat == null)
                {
                    StatusText.Text = "⚠️ Eroare la citirea articolului selectat!";
                    Logs.Write("⚠️ BtnDelCant_Click: Cast la BonItem eșuat");
                    return;
                }

                // ✅ CITEȘTE CANTITATEA DIN TxtCantitateBon
                decimal cantitateDecrement = 1; // valoare implicită

                if (!string.IsNullOrWhiteSpace(TxtCantitateBon.Text))
                {
                    // Suportă atât virgulă cât și punct ca separator zecimal
                    string cantitateText = TxtCantitateBon.Text.Replace(',', '.');

                    if (decimal.TryParse(cantitateText,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal cantitateInput))
                    {
                        if (cantitateInput > 0)
                        {
                            cantitateDecrement = cantitateInput;
                        }
                        else
                        {
                            StatusText.Text = "⚠️ Cantitatea trebuie să fie pozitivă!";
                            Logs.Write($"⚠️ BtnDelCant_Click: Cantitate invalidă ({cantitateInput})");
                            return;
                        }
                    }
                    else
                    {
                        StatusText.Text = "⚠️ Format cantitate invalid!";
                        Logs.Write($"⚠️ BtnDelCant_Click: Parse failed pentru '{TxtCantitateBon.Text}'");
                        return;
                    }
                }

                // ✅ VALIDARE CRITICĂ: VERIFICĂ DACĂ DECREMENTAREA E MAI MARE DECÂT CANTITATEA EXISTENTĂ
                if (cantitateDecrement > bonItemSelectat.Cantitate)
                {
                    StatusText.Text = $"❌ EROARE: Cantitatea de decrementat ({cantitateDecrement:F3}) este mai mare decât cantitatea din bon ({bonItemSelectat.Cantitate:F3})!";

                    Logs.Write($"❌ BtnDelCant_Click: Decrementare invalidă - cerut {cantitateDecrement:F3}, disponibil {bonItemSelectat.Cantitate:F3}");

                    // ✅ OPȚIONAL: Afișează MessageBox pentru atenționare
                    MessageBox.Show(
                        $"Nu poți decrementa cu {cantitateDecrement:F3} bucăți!\n\n" +
                        $"Cantitatea curentă în bon: {bonItemSelectat.Cantitate:F3}\n" +
                        $"Cantitate maximă de decrementat: {bonItemSelectat.Cantitate:F3}",
                        "Cantitate insuficientă",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                // ✅ DECREMENTEAZĂ CANTITATEA
                decimal cantitateBefore = bonItemSelectat.Cantitate;
                decimal cantitateNoua = bonItemSelectat.Cantitate - cantitateDecrement;

                bool success;

                // ✅ VERIFICĂ DACĂ CANTITATEA DEVINE 0 sau negativă → ȘTERGE ARTICOLUL
                if (cantitateNoua <= 0)
                {
                    var result = MessageBox.Show(
                        $"Cantitatea va deveni 0.\n\nVrei să ștergi articolul '{bonItemSelectat.Nume}' din bon?",
                        "Confirmare Ștergere",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        success = _bonManager.StergeArticol(bonItemSelectat);

                        if (success)
                        {
                            StatusText.Text = $"🗑️ Articol șters: {bonItemSelectat.Nume}";
                            Logs.Write($"🗑️ BtnDelCant_Click: Articol șters din bon - {bonItemSelectat.Nume}");
                        }
                    }
                    else
                    {
                        StatusText.Text = "⚠️ Ștergere anulată";
                        return;
                    }
                }
                else
                {
                    // ✅ Decrementează normal
                    success = _bonManager.DecrementeazaCantitate(bonItemSelectat, cantitateDecrement);

                    if (success)
                    {
                        StatusText.Text = $"✅ Cantitate actualizată: {bonItemSelectat.Nume} - {cantitateBefore:F3} → {bonItemSelectat.Cantitate:F3}";

                        Logs.Write($"✅ BtnDelCant_Click: {bonItemSelectat.Nume} - cantitate {cantitateBefore:F3} → {bonItemSelectat.Cantitate:F3} (-{cantitateDecrement:F3})");
                    }
                }

                if (success)
                {
                    // ✅ Actualizează totalul în UI
                    TotalText.Text = _bonManager.Total.ToString("F2");

                    // ✅ OPȚIONAL: Resetează cantitatea la 1
                    TxtCantitateBon.Text = "1.000";
                }
                else
                {
                    StatusText.Text = "❌ Eroare la decrementarea cantității!";
                    Logs.Write($"❌ BtnDelCant_Click: DecrementeazaCantitate returnat false");
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "❌ EROARE la decrementarea cantității!";
                Logs.Write("❌ EROARE BtnDelCant_Click:");
                Logs.Write(ex);
                MessageBox.Show($"Eroare la decrementarea cantității:\n\n{ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>    
        /// Generează număr unic pentru bon    
        /// </summary>    
        private string GenerareNrBon()
        {
            return $"TEMP{DateTime.Now:yyyyMMddHHmmss}";
        }
        #endregion

        /// <summary>    
        /// Incasare bon fiscal
        /// </summary>
        private void Incasare_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ✅ VERIFICĂ DACĂ BONUL ARE PRODUSE
                if (_bonManager == null || _bonManager.EsteGol)
                {
                    MessageBox.Show("Nu există produse pe bon!",
                                    "Atenție",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                // ✅ CALCULEAZĂ TOTAL DIN BONMANAGER
                decimal totalBon = _bonManager.Total;

                // ✅ EXTRAGE CUI DIN BONUL CURENT (dacă există)
                string cuiBon = "";

                Logs.Write($"Incasare_Click: Deschidere fereastră încasare - Total: {totalBon:F2} lei, CUI: {cuiBon}");

                // ✅ DESCHIDE FEREASTRA DE ÎNCASARE
                var incasareWindow = new IncasareWindow(totalBon, cuiBon)
                {
                    Owner = this
                };

                if (incasareWindow.ShowDialog() == true)
                {
                    // ✅ ÎNCASARE FINALIZATĂ CU SUCCES
                    Logs.Write("✅ Încasare finalizată cu succes!");

                    // ✅ GOLEȘTE BONUL CURENT
                    _bonManager.GolesteBon();
                    TotalText.Text = "0.00";
                    _bonCurent = null;

                    StatusText.Text = "Încasare finalizată - Bon nou";

                    MessageBox.Show("Încasare finalizată cu succes!",
                                    "Succes",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                else
                {
                    // Utilizatorul a anulat încasarea
                    Logs.Write("⚠️ Încasare anulată de utilizator");
                    StatusText.Text = "Încasare anulată";
                }
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE în Incasare_Click:");
                Logs.Write(ex);
                MessageBox.Show($"Eroare la încasare:\n\n{ex.Message}",
                                "Eroare",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}
