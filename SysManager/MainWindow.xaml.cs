// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SysManager.Controls;
using SysManager.Models;
using SysManager.Managers;

namespace SysManager
{
    /// <summary>
    /// Partial class - Logica principală MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        #region === VARIABILE PRIVATE ===

        private readonly DbQuery _dbQuery;
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
        #endregion

        #region === INIȚIALIZARE ===

        public MainWindow()
        {
            InitializeComponent();

            Logs.Write("MainWindow: Inițializare fereastră principală");

            this.KeyDown += MainWindow_KeyDown;
            _dbQuery = new DbQuery();

            DataInchiderii.Text = DateTime.Now.ToString("dd.MM.yyyy");

            this.Loaded += MainWindow_Loaded;
            this.SizeChanged += MainWindow_SizeChanged;

            // ✅ Înregistrează event handler-ul (implementarea va fi în MainWindow.Products.cs)
            ProductsPanel.SizeChanged += ProductsPanel_SizeChanged;
            _bonuriAsteptareManager = new BonuriAsteptareManager();
            // ✅ Inițializează BonManager
            InitializeazaBonManager();

            InitializeSearchTimer();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                // ✅ ÎNCARCĂ SETĂRILE DIN BAZA DE DATE
                _grupeSettings = _dbQuery.GetGrupeSettings();
                _produseSettings = _dbQuery.GetProduseSettings();

                // ✅ SETEAZĂ ÎNĂLȚIMEA RÂNDULUI PENTRU GRUPE
                if (_grupeSettings.PanouHeight > 0)
                {
                    this.MainGrid.RowDefinitions[1].Height = new GridLength(_grupeSettings.PanouHeight);
                    Logs.Write($"MainWindow_Loaded: Rând Grupe setat la {_grupeSettings.PanouHeight}px din DB");
                }
                else
                {
                    this.MainGrid.RowDefinitions[1].Height = GridLength.Auto;
                    Logs.Write("MainWindow_Loaded: Rând Grupe setat pe Auto (PANOU_HEIGHT=0)");
                }

                // ✅ ÎNCARCĂ DATELE

                CalculateLayout();
                LoadGestiuni();
                LoadGroups();
                LoadProducts();
            }, System.Windows.Threading.DispatcherPriority.Loaded);
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
                    Height = 32,
                    Margin = new Thickness(2),
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
                        Height = 32,
                        Margin = new Thickness(2),
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
                        Logs.Write($"📦 Selectat: TOATE GESTIUNILE");
                        StatusText.Text = "Afișare: Toate gestiunile";
                    }
                    else
                    {
                        Logs.Write($"📦 Gestiune selectată: {gestiune.DisplayName} (ID: {gestiune.Id})");
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

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _bonManager.GolesteBon();
            TotalText.Text = "0.00";
            StatusText.Text = "Bon anulat";
            //Logs.Write("MainWindow: Bon anulat");
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Procesare încasare...";
            Logs.Write("MainWindow: Încasare inițiată");
            var bonuri = _bonuriAsteptareManager.GetBonuriInAsteptare();
            var dialog = new BonuriAsteptareWindow(bonuri, _bonuriAsteptareManager);
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
                // ✅ Verifică dacă există produse în bon (FOLOSEȘTE BONMANAGER)
                if (_bonManager.EsteGol)
                {
                    MessageBox.Show("Bonul este gol. Adaugă produse înainte de a-l salva în așteptare.",
                        "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Creează obiectul BonAsteptare
                var bon = new BonAsteptare
                {
                    NrBon = GenerareNrBon(),
                    DataCreare = DateTime.Now,
                    Total = _bonManager.Total,  // ✅ FOLOSEȘTE BONMANAGER
                    Observatii = ""
                };

                // ✅ Adaugă produsele DIN BONMANAGER (NU DIN GRID!)
                foreach (var bonItem in _bonManager.Items)
                {
                    bon.Detalii.Add(new BonAsteptareDetaliu
                    {
                        IdProdus = bonItem.IdProdus,
                        DenumireProdus = bonItem.Nume,
                        Cantitate = bonItem.Cantitate,
                        PretUnitar = bonItem.PretBrut,
                        Valoare = bonItem.Total
                    });
                }

                // Salvează în baza de date
                int bonId = _bonuriAsteptareManager.SalveazaBonInAsteptare(bon);

                MessageBox.Show($"Bonul #{bon.NrBon} a fost salvat în așteptare!",
                    "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

                // ✅ GOLEȘTE BONUL PRIN BONMANAGER (NU DIRECT GRID!)
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
                // ✅ GOLEȘTE BONUL PRIN BONMANAGER
                _bonManager.GolesteBon();

                // ✅ ADAUGĂ PRODUSELE PRIN BONMANAGER
                foreach (var detaliu in bon.Detalii)
                {
                    // Creează un obiect Produs din detaliile bonului
                    var produs = new Produs
                    {
                        Id = detaliu.IdProdus,
                        Denumire = detaliu.DenumireProdus,
                        PretBrut = detaliu.PretUnitar,
                        // Completează și alte proprietăți necesare
                    };

                    // Adaugă produsul cu cantitatea salvată
                    _bonManager.AdaugaProdus(produs, detaliu.Cantitate);
                }

                // ✅ Actualizează totalul (ar trebui să se actualizeze automat prin BonManager)
                TotalText.Text = _bonManager.Total.ToString("F2");

                // Salvează referința bonului curent
                _bonCurent = bon;

                StatusText.Text = $"Bon #{bon.NrBon} încărcat din așteptare";
                Logs.Write($"Bon #{bon.NrBon} (ID: {bon.Id}) încărcat în grid ({bon.Detalii.Count} produse)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea bonului: {ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Logs.Write("EROARE la încărcarea bonului:");
                Logs.Write(ex);
            }
        }


        // ═══════════════════════════════════════════════════════════════
        // INIȚIALIZARE BON MANAGER
        // ═══════════════════════════════════════════════════════════════
        private void InitializeazaBonManager()
        {
            _bonManager = new BonManager();

            // ✅ Setează DataGrid
            BonGrid.ItemsSource = _bonManager.Items;

            // ✅ Ascultă evenimentul de modificare total
            _bonManager.TotalModificat += (s, total) =>
            {
                //if (TotalTextBlock != null)
                //{
                //    TotalTextBlock.Text = $"{total:F2} LEI";
                //}
            };

            // ✅ Ascultă evenimentele de adăugare/ștergere (opțional pentru debugging)
            _bonManager.ProdusAdaugat += (s, item) =>
            {
                Logs.Write($"✅ EVENT: Produs adăugat - {item.Nume}");

                // ✅ Scroll automat la ultimul produs
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
        }

        /// <summary>    
        /// Generează număr unic pentru bon    
        /// </summary>    
        private string GenerareNrBon()
        {
            return $"TEMP{DateTime.Now:yyyyMMddHHmmss}";
        }
        #endregion

    }
}