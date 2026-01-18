// MainWindow.Products.cs
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using SysManager.Controls;

namespace SysManager
{
    /// <summary>
    /// Partial class pentru gestionarea produselor
    /// </summary>
    public partial class MainWindow : Window
    {
        #region === EVENT HANDLERS SYSTEM ===

        /// <summary>
        /// Event handler pentru schimbarea dimensiunii ProductsPanel
        /// Recalculează layout-ul când panel-ul își schimbă dimensiunea
        /// </summary>
        private void ProductsPanel_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            // ✅ Verifică dacă s-a schimbat lățimea și panel-ul are dimensiune validă
            if (e.WidthChanged && ProductsPanel.ActualWidth > 0 && _produseSettings != null)
            {
                Logs.Write($"ProductsPanel.SizeChanged: {e.PreviousSize.Width:F0} → {e.NewSize.Width:F0}px");

                // ✅ Recalculează layout-ul
                CalculateProductLayout();

                // ✅ Reafișează pagina curentă cu noul layout
                if (_allProductButtons.Count > 0)
                {
                    DisplayProductPage();
                }
            }
        }

        #endregion

        #region === CALCUL LAYOUT PRODUSE ===

        /// <summary>
        /// Calculează layout-ul pentru panoul de produse
        /// </summary>
        private void CalculateProductLayout()
        {
            try
            {
                if (_produseSettings == null)
                    return;

                // ✅ OBȚINE GRID-UL PĂRINTE O SINGURĂ DATĂ
                var parentGrid = ProductsPanel.Parent as System.Windows.Controls.Grid;

                // ✅ OBȚINE LĂȚIMEA DISPONIBILĂ
                double availableWidth = ProductsPanel.ActualWidth;
                if (availableWidth <= 0)
                {
                    if (parentGrid != null && parentGrid.ActualWidth > 0)
                    {
                        availableWidth = parentGrid.ActualWidth - 10; // Scade padding Border (5+5)
                    }
                    else
                    {
                        availableWidth = this.ActualWidth - 380 - 30;
                    }
                }

                // ✅ OBȚINE ÎNĂLȚIMEA DISPONIBILĂ (CRITICAL!)
                double availableHeight = 0;

                if (parentGrid != null && parentGrid.ActualHeight > 0)
                {
                    // Grid-ul are 2 rânduri: Row0=ProductsPanel, Row1=Bara navigare
                    availableHeight = parentGrid.ActualHeight;

                    // ✅ SCADE ÎNĂLȚIMEA BAREI DE NAVIGARE (Row1 = Auto)
                    // Bara de navigare are Button (Height=35) + Border (Padding=5) + Margin = ~50px
                    availableHeight -= 55; // Bara navigare + margin

                    Logs.Write($"   → Grid părinte: {parentGrid.ActualHeight:F0}px - 55px (bară nav) = {availableHeight:F0}px");
                }
                else
                {
                    // Fallback: calculează pe baza înălțimii ferestrei
                    availableHeight = this.ActualHeight
                        - 40    // Header gestiuni (Row 0)
                        - (_grupeSettings?.PanouHeight ?? 119) // Bara grupe (Row 1)
                        - 30    // Footer (Row 3)
                        - 55;   // Bara navigare produse

                    Logs.Write($"   → Fallback calcul: înălțime fereastră - headers = {availableHeight:F0}px");
                }

                // ✅ SCADE PADDING-UL BORDER-ULUI (5px sus + 5px jos = 10px)
                availableHeight -= 10;

                Logs.Write($"CalculateProductLayout: Zonă disponibilă finală = {availableWidth:F0}x{availableHeight:F0}px");

                // ✅ DIMENSIUNI BUTOANE (cu margin)
                double buttonWidth = _produseSettings.Latime + 2;   // Margin(1,1,1,1)
                double buttonHeight = _produseSettings.Inaltime + 2; // Margin(1,1,1,1)

                // ✅ CALCUL COLOANE ȘI RÂNDURI
                _productColumns = Math.Max(1, (int)Math.Floor(availableWidth / buttonWidth));
                _productRows = Math.Max(1, (int)Math.Floor(availableHeight / buttonHeight));

                // ✅ SETARE UNIFORMGRID
                ProductsPanel.Columns = _productColumns;
                ProductsPanel.Rows = _productRows;

                _productsPerPage = _productColumns * _productRows;

                Logs.Write($"📊 LAYOUT PRODUSE:");
                Logs.Write($"   → Buton: {_produseSettings.Latime}x{_produseSettings.Inaltime}px (+2px margin)");
                Logs.Write($"   → Dimensiune efectivă: {buttonWidth}x{buttonHeight}px");
                Logs.Write($"   → Grid: {_productColumns} col × {_productRows} rows = {_productsPerPage} produse/pagină");
                Logs.Write($"   → UniformGrid: Columns={ProductsPanel.Columns}, Rows={ProductsPanel.Rows}");
            }
            catch (Exception ex)
            {
                Logs.Write("EROARE la calcularea layout-ului produse:");
                Logs.Write(ex);
                _productsPerPage = 195; // Fallback
                _productColumns = 13;
                _productRows = 15;
            }
        }

        #endregion

        #region === ÎNCĂRCARE PRODUSE ===

        /// <summary>
        /// Încarcă produsele pentru gestiunea și grupa selectată
        /// </summary>
        private void LoadProducts()
        {
            try
            {
                // ✅ ASCUNDE OVERLAY-UL când încarcă produse normale
                AscundeMesajNuExistaProduse();

                _allProductButtons.Clear();

                // ✅ ÎNCARCĂ PRODUSELE DIN BAZA DE DATE
                var produse = _dbQuery.GetProduse(_selectedGestiuneId, _selectedGrupaId);

                //Logs.Write($"📦 PRODUSE GĂSITE: {produse.Count}");

                // ✅ RECALCULEAZĂ LAYOUT ÎNAINTE de a crea butoanele
                CalculateProductLayout();


                // ✅ CREEAZĂ BUTOANE PENTRU FIECARE PRODUS
                foreach (var produs in produse)
                {
                    var btn = new POSButton
                    {
                        ProductName = produs.Denumire,
                        Price = $"{produs.PretBrut:F2} RON",
                        ProductColor = new SolidColorBrush(Color.FromRgb(2, 119, 189)), // #0277BD (albastru turcoaz)
                        Width = _produseSettings.Latime,
                        Height = _produseSettings.Inaltime,
                        Margin = new Thickness(1, 1, 1, 1), // ✅ Margin asimetric pentru border vizibil pe dreapta/jos
                        GestiuneName = produs.NumeGestiune, // ✅ SETEAZĂ NUMELE GESTIUNII
                        Tag = produs
                    };

                    // ✅ TODO: Încarcă imaginea dacă există
                    // if (!string.IsNullOrEmpty(produs.ImagineUrl))
                    // {
                    //     btn.ImagePath = new BitmapImage(new Uri(produs.ImagineUrl));
                    // }

                    btn.Click += Product_Click;
                    _allProductButtons.Add(btn);
                }

                // ✅ CALCUL PAGINARE
                _totalProductPages = _productsPerPage > 0
                    ? (int)Math.Ceiling((double)_allProductButtons.Count / _productsPerPage)
                    : 1;
                _currentProductPage = 0;

                //Logs.Write($"📊 PAGINARE PRODUSE:");
                //Logs.Write($"   → Total produse: {_allProductButtons.Count}");
                //Logs.Write($"   → Produse per pagină: {_productsPerPage}");
                //Logs.Write($"   → Total pagini: {_totalProductPages}");

                DisplayProductPage();

                // ✅ UPDATE UI
                //TotalProducts.Text = $"{produse.Count} articole";
                //string grupaName = _selectedGrupaId == 0 ? "toate grupele" : SelectedGroup.Text;
                //StatusText.Text = $"Încărcate {produse.Count} produse ({grupaName})";

                //Logs.Write($"LoadProducts: SUCCESS");
            }
            catch (Exception ex)
            {
                Logs.Write("EROARE la încărcarea produselor:");
                Logs.Write(ex);
                StatusText.Text = "EROARE la încărcarea produselor!";
                //MessageBox.Show($"Eroare la încărcarea produselor:\n{ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region === AFIȘARE PAGINI PRODUSE ===

        /// <summary>
        /// Afișează pagina curentă de produse
        /// </summary>
        private void DisplayProductPage()
        {
            try
            {
                ProductsPanel.Children.Clear();

                if (_allProductButtons.Count == 0)
                {
                    //Logs.Write("DisplayProductPage: Niciun produs de afișat");
                    ProductPageInfo.Text = "0/0";
                    return;
                }

                int start = _currentProductPage * _productsPerPage;
                int end = Math.Min(start + _productsPerPage, _allProductButtons.Count);

                // ✅ ADAUGĂ BUTOANELE PRODUSE
                for (int i = start; i < end; i++)
                {
                    ProductsPanel.Children.Add(_allProductButtons[i]);
                }

                ProductsPanel.UpdateLayout();

                UpdateProductNavigationButtons();

                //Logs.Write($"📄 PAGINA PRODUSE {_currentProductPage + 1}/{_totalProductPages}: Afișate {end - start} produse (index {start}-{end - 1})");
            }
            catch (Exception ex)
            {
                Logs.Write("EROARE la afișarea paginii produse:");
                Logs.Write(ex);
            }
        }

        /// <summary>
        /// Actualizează butoanele de navigare pentru produse
        /// </summary>
        private void UpdateProductNavigationButtons()
        {
            BtnPrevProductPage.IsEnabled = _currentProductPage > 0;
            BtnNextProductPage.IsEnabled = _currentProductPage < _totalProductPages - 1;
            ProductPageInfo.Text = $"{_currentProductPage + 1}/{_totalProductPages}";
        }

        #endregion

        #region === EVENT HANDLERS PRODUSE ===

        /// <summary>
        /// Handler pentru click pe buton produs - ADAUGĂ ÎN BON CU CANTITATEA DIN TEXTBOX
        /// ✅ Citește cantitatea din TxtCantitateBon și calculează prețul total
        /// </summary>
        private void Product_Click(object sender, RoutedEventArgs e)
        {
            //Logs.Write($"🔘 Product_Click: Event declanșat!");
            // ✅ CURĂȚĂ INTERFAȚA DE CĂUTARE (închide tastatura + șterge text)
            ClearSearchInterface();

            if (sender is POSButton btn && btn.Tag is Models.Produs produs)
            {
                //Logs.Write($"📦 Produs identificat: {produs.Denumire} (ID: {produs.Id}, PretBrut: {produs.PretBrut:F2} RON cu TVA)");

                try
                {
                    // ✅ VERIFICĂ DACĂ BONMANAGER E INIȚIALIZAT
                    if (_bonManager == null)
                    {
                        //Logs.Write("⚠️ AVERTISMENT: BonManager nu este inițializat! Reinițializare...");
                        InitializeazaBonManager();
                    }

                    // ═══════════════════════════════════════════════════════════════
                    // ✅ CITEȘTE CANTITATEA DIN TxtCantitateBon
                    // ═══════════════════════════════════════════════════════════════
                    decimal cantitate = 1; // valoare implicită

                    if (!string.IsNullOrWhiteSpace(TxtCantitateBon.Text))
                    {
                        // ✅ Suportă atât virgulă cât și punct ca separator zecimal
                        string cantitateText = TxtCantitateBon.Text.Replace(',', '.');

                        if (decimal.TryParse(cantitateText,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out decimal cantitateInput))
                        {
                            if (cantitateInput > 0)
                            {
                                cantitate = cantitateInput;
                                //Logs.Write($"✅ Cantitate citită din TxtCantitateBon: {cantitate}");
                            }
                            else
                            {
                                //Logs.Write($"⚠️ Cantitate invalidă ({cantitateInput}), folosim 1");
                                TxtCantitateBon.Text = "1";
                                cantitate = 1;
                            }
                        }
                        else
                        {
                           // Logs.Write($"⚠️ Parse failed pentru '{TxtCantitateBon.Text}', folosim 1");
                            TxtCantitateBon.Text = "1";
                            cantitate = 1;
                        }
                    }
                    else
                    {
                        //Logs.Write("⚠️ TxtCantitateBon gol, folosim cantitate = 1");
                        cantitate = 1;
                    }

                    // ═══════════════════════════════════════════════════════════════
                    // ✅ CALCULEAZĂ PREȚUL TOTAL: cantitate × preț_unitar
                    // IMPORTANT: Folosim PretBrut (cu TVA) pentru calcul, NU Pret (fără TVA)!
                    // Exemplu: 0.520 × 10 = 5.20 RON
                    // ═══════════════════════════════════════════════════════════════
                    decimal pretBrutTotal = cantitate * produs.PretBrut;

                    //Logs.Write($"💵 Calcul preț cu TVA (BRUT): {cantitate} × {produs.PretBrut:F2} = {pretBrutTotal:F2} RON ✅");

                    // ═══════════════════════════════════════════════════════════════
                    // ✅ ADAUGĂ PRODUSUL ÎN BON CU CANTITATEA SPECIFICATĂ
                    // BonManager va înmulți automat cantitatea cu prețul unitar
                    // ═══════════════════════════════════════════════════════════════
                    var bonItem = _bonManager.AdaugaProdus(produs, cantitate);

                    //Logs.Write($"✅ Produs adăugat în bon: {produs.Denumire} × {cantitate} = {bonItem.Total:F2} RON");

                    // ═══════════════════════════════════════════════════════════════
                    // ✅ ACTUALIZEAZĂ TOTALUL ÎN UI
                    // ═══════════════════════════════════════════════════════════════
                    TotalText.Text = _bonManager.Total.ToString("F2");
                    //Logs.Write($"💰 Total bon actualizat: {_bonManager.Total:F2} RON");

                    // ═══════════════════════════════════════════════════════════════
                    // ✅ RESETEAZĂ CANTITATEA LA 1 (OPȚIONAL - COMENTEAZĂ DACĂ NU VREI)
                    // ═══════════════════════════════════════════════════════════════
                    TxtCantitateBon.Text = "1.000";

                    // ✅ Animație vizuală (opțional)
                    //try
                    //{
                    //    AnimatieProdusAdaugat(btn);
                    //}
                    //catch (Exception animEx)
                    //{
                    //    Logs.Write($"⚠️ Eroare animație (non-critical): {animEx.Message}");
                    //}

                    // ✅ Actualizează status bar
                    StatusText.Text = $"Adăugat: {produs.Denumire} × {cantitate} = {bonItem.Total:F2} RON";
                    BonGrid.SelectedItem = bonItem;
                    BonGrid.ScrollIntoView(bonItem);
                    BonGrid.Focus();
                }
                catch (Exception ex)
                {
                    Logs.Write($"❌ EROARE la adăugarea produsului:");
                    Logs.Write(ex);

                    //MessageBox.Show($"Eroare la adăugarea produsului!\n\n{ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);

                    StatusText.Text = "EROARE la adăugarea produsului!";
                }
            }
            else
            {
                //Logs.Write($"⚠️ Product_Click: Butonul sau produsul nu a fost identificat corect!");
                //Logs.Write($"   Sender Type: {sender?.GetType().Name ?? "null"}");

                //if (sender is POSButton button)
                //{
                //    Logs.Write($"   Button.Tag Type: {button.Tag?.GetType().Name ?? "null"}");
                //    Logs.Write($"   Button.Tag Value: {button.Tag?.ToString() ?? "null"}");
                //}
                //else
                //{
                //    Logs.Write($"   Sender nu este POSButton!");
                //}

                //MessageBox.Show("Eroare: Produsul nu a fost identificat corect!\n\nVerifică Logs.txt pentru detalii.", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);

                StatusText.Text = "Eroare: Produs invalid!";
            }
        }

        /// <summary>
        /// Animație vizuală când se adaugă un produs (feedback verde)
        /// </summary>
        private void AnimatieProdusAdaugat(POSButton button)
        {
            try
            {
                var colorOriginal = button.ProductColor;

                // ✅ Schimbă temporar culoarea în verde pentru feedback vizual
                button.ProductColor = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // #4CAF50 (verde)

                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(200)
                };

                timer.Tick += (s, args) =>
                {
                    button.ProductColor = colorOriginal; // ✅ Restaurează culoarea originală
                    timer.Stop();
                };

                timer.Start();
            }
            catch (Exception ex)
            {
                // ✅ Ignoră erorile la animație (non-critical)
                Logs.Write($"⚠️ Eroare animație produs (ignorată): {ex.Message}");
            }
        }

        /// <summary>
        /// Pagina anterioară de produse
        /// </summary>
        private void PrevProductPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProductPage > 0)
            {
                _currentProductPage--;
                DisplayProductPage();
                Logs.Write($"◀️ Pagină anterioară produse: {_currentProductPage + 1}/{_totalProductPages}");
            }
        }

        /// <summary>
        /// Pagina următoare de produse
        /// </summary>
        private void NextProductPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProductPage < _totalProductPages - 1)
            {
                _currentProductPage++;
                DisplayProductPage();
                Logs.Write($"▶️ Pagină următoare produse: {_currentProductPage + 1}/{_totalProductPages}");
            }
        }

        #endregion

        #region === CĂUTARE PRODUSE ===

        /// <summary>
        /// Flag pentru a preveni căutări multiple simultane
        /// </summary>
        private bool _isSearching = false;

        /// <summary>
        /// Timer pentru debounce la căutare (așteaptă 300ms după ultima tastare)
        /// </summary>
        private System.Windows.Threading.DispatcherTimer _searchTimer;

        /// <summary>
        /// Inițializează timer-ul de căutare
        /// ⚠️ IMPORTANT: Apelează în constructor MainWindow() și conectează la XAML!
        /// </summary>
        private void InitializeSearchTimer()
        {
            _searchTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300) // ✅ Așteaptă 300ms după ultima tastare
            };
            _searchTimer.Tick += SearchTimer_Tick;

            Logs.Write("✅ InitializeSearchTimer: Timer de căutare inițializat (debounce 300ms)");
        }

        /// <summary>
        /// Event handler pentru TextChanged pe TxtCautareArticolBon
        /// ⚠️ CONECTEAZĂ ÎN XAML: TextChanged="TxtCautareArticolBon_TextChanged"
        /// </summary>
        private void TxtCautareArticolBon_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // ✅ VERIFICĂ DACĂ TIMER-UL E INIȚIALIZAT
            if (_searchTimer == null)
            {
                Logs.Write("⚠️ _searchTimer nu este inițializat, se ignoră evenimentul TextChanged");
                return;
            }

            // ✅ Resetează timer-ul la fiecare tastare (debounce)
            _searchTimer.Stop();
            _searchTimer.Start();
        }


        /// <summary>
        /// Timer tick - execută căutarea efectivă după 300ms de la ultima tastare
        /// </summary>
        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            ExecuteSearch();
        }

        /// <summary>
        /// Execută căutarea de produse
        /// </summary>
        private void ExecuteSearch()
        {
            try
            {
                if (_isSearching)
                {
                    Logs.Write("⚠️ Căutare deja în desfășurare, se ignoră...");
                    return;
                }

                string searchText = TxtCautareArticolBon.Text?.Trim() ?? "";

                // ✅ Dacă textul e gol, ascunde overlay-ul și reîncarcă produsele
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    Logs.Write("🔍 Text căutare gol, ascundere overlay...");
                    AscundeMesajNuExistaProduse(); // ✅ ASCUNDE OVERLAY
                    LoadProducts(); // ✅ REÎNCARCĂ PRODUSE NORMALE
                    return;
                }

                // ✅ Minim 2 caractere pentru căutare
                if (searchText.Length < 2)
                {
                    StatusText.Text = "Introduceți minim 2 caractere pentru căutare";
                    return;
                }

                _isSearching = true;

                Logs.Write($"🔍 CĂUTARE PRODUSE: '{searchText}'");

                // ✅ APELEAZĂ PROCEDURA FIREBIRD
                var produse = _dbQuery.SearchArticole(searchText, _selectedGrupaId);

                // ✅ VERIFICĂ REZULTATELE
                if (produse.Count == 0)
                {
                    // ❌ NU S-AU GĂSIT PRODUSE - ARATĂ OVERLAY
                    AfiseazaMesajNuExistaProduse(searchText);
                }
                else
                {
                    // ✅ PRODUSE GĂSITE - ASCUNDE OVERLAY ȘI AFIȘEAZĂ PRODUSE
                    AscundeMesajNuExistaProduse();
                    AfiseazaProduseGasite(produse);
                }
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE la căutarea produselor:");
                Logs.Write(ex);
                StatusText.Text = "EROARE la căutarea produselor!";
            }
            finally
            {
                _isSearching = false;
            }
        }

        /// <summary>
        /// Afișează mesaj când nu s-au găsit produse
        /// </summary>
        private void AfiseazaMesajNuExistaProduse(string searchText)
        {
            // ✅ ASCUNDE PRODUSELE (dar nu le șterge și nu modifică layout-ul!)
            ProductsPanel.Visibility = Visibility.Collapsed;

            // ✅ AFIȘEAZĂ OVERLAY-UL CU MESAJ
            NoProductsOverlay.Visibility = Visibility.Visible;
            NoProductsSearch.Text = $"Căutare: \"{searchText}\"";

            // ✅ Actualizează UI
            ProductPageInfo.Text = "0/0";
            BtnPrevProductPage.IsEnabled = false;
            BtnNextProductPage.IsEnabled = false;
            StatusText.Text = $"Niciun produs găsit pentru '{searchText}'";

            Logs.Write($"❌ Căutare fără rezultate: '{searchText}' - Overlay afișat");
        }

        /// <summary>
        /// Ascunde mesajul și arată din nou produsele
        /// </summary>
        private void AscundeMesajNuExistaProduse()
        {
            // ✅ ASCUNDE OVERLAY-UL
            NoProductsOverlay.Visibility = Visibility.Collapsed;

            // ✅ ARATĂ PRODUSELE
            ProductsPanel.Visibility = Visibility.Visible;

            Logs.Write("✅ Overlay ascuns, ProductsPanel vizibil");
        }

        /// <summary>
        /// Afișează produsele găsite din căutare
        /// </summary>
        private void AfiseazaProduseGasite(System.Collections.Generic.List<Models.Produs> produse)
        {
            try
            {
                _allProductButtons.Clear();

                Logs.Write($"✅ Afișare {produse.Count} produse găsite din căutare");

                // ✅ RECALCULEAZĂ LAYOUT
                CalculateProductLayout();


                // ✅ CREEAZĂ BUTOANE PENTRU FIECARE PRODUS (CU CULOARE VERDE PENTRU REZULTATE CĂUTARE!)
                foreach (var produs in produse)
                {
                    var btn = new POSButton
                    {
                        ProductName = produs.Denumire,
                        Price = $"{produs.PretBrut:F2} RON",
                        ProductColor = new SolidColorBrush(Color.FromRgb(76, 175, 80)), // ✅ VERDE (#4CAF50) pentru rezultate căutare
                        Width = _produseSettings.Latime,
                        Height = _produseSettings.Inaltime,
                        Margin = new Thickness(1, 1, 1, 1),
                        GestiuneName = produs.NumeGestiune, // ✅ SETEAZĂ NUMELE GESTIUNII
                        Tag = produs
                    };

                    btn.Click += Product_Click;
                    _allProductButtons.Add(btn);
                }

                // ✅ CALCUL PAGINARE
                _totalProductPages = _productsPerPage > 0
                    ? (int)Math.Ceiling((double)_allProductButtons.Count / _productsPerPage)
                    : 1;
                _currentProductPage = 0;

                DisplayProductPage();

                StatusText.Text = $"Găsite {produse.Count} produse";
                Logs.Write($"✅ Afișate {produse.Count} produse din căutare (pagina 1/{_totalProductPages})");
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE la afișarea produselor găsite:");
                Logs.Write(ex);
            }
        }

        #endregion
    }
}
