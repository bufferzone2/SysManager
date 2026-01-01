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
                Logs.Write($"LoadProducts: START încărcare produse (Gestiune={_selectedGestiuneId}, Grupa={_selectedGrupaId})");

                _allProductButtons.Clear();

                // ✅ ÎNCARCĂ PRODUSELE DIN BAZA DE DATE
                var produse = _dbQuery.GetProduse(_selectedGestiuneId, _selectedGrupaId);

                Logs.Write($"📦 PRODUSE GĂSITE: {produse.Count}");

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

                Logs.Write($"📊 PAGINARE PRODUSE:");
                Logs.Write($"   → Total produse: {_allProductButtons.Count}");
                Logs.Write($"   → Produse per pagină: {_productsPerPage}");
                Logs.Write($"   → Total pagini: {_totalProductPages}");

                DisplayProductPage();

                // ✅ UPDATE UI
                //TotalProducts.Text = $"{produse.Count} articole";
                //string grupaName = _selectedGrupaId == 0 ? "toate grupele" : SelectedGroup.Text;
                //StatusText.Text = $"Încărcate {produse.Count} produse ({grupaName})";

                Logs.Write($"LoadProducts: SUCCESS");
            }
            catch (Exception ex)
            {
                Logs.Write("EROARE la încărcarea produselor:");
                Logs.Write(ex);
                StatusText.Text = "EROARE la încărcarea produselor!";
                MessageBox.Show($"Eroare la încărcarea produselor:\n{ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
                    Logs.Write("DisplayProductPage: Niciun produs de afișat");
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

                Logs.Write($"📄 PAGINA PRODUSE {_currentProductPage + 1}/{_totalProductPages}: Afișate {end - start} produse (index {start}-{end - 1})");
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
        /// Handler pentru click pe buton produs - ADAUGĂ ÎN BON
        /// </summary>
        private void Product_Click(object sender, RoutedEventArgs e)
        {
            Logs.Write($"🔘 Product_Click: Event declanșat!"); // ✅ DEBUG

            if (sender is POSButton btn && btn.Tag is Models.Produs produs)
            {
                Logs.Write($"📦 Produs identificat: {produs.Denumire} (ID: {produs.Id}, Preț: {produs.Pret:F2} RON)");

                try
                {
                    // ✅ VERIFICĂ DACĂ BONMANAGER E INIȚIALIZAT
                    if (_bonManager == null)
                    {
                        Logs.Write("⚠️ AVERTISMENT: BonManager nu este inițializat! Reinițializare...");
                        InitializeazaBonManager();
                    }

                    // ✅ ADAUGĂ PRODUSUL ÎN BON PRIN BON MANAGER
                    _bonManager.AdaugaProdus(produs);

                    Logs.Write($"✅ Produs adăugat cu succes în BonManager!");

                    // ✅ Animație vizuală (opțional - comentează dacă dă eroare)
                    try
                    {
                        AnimatieProdusAdaugat(btn);
                    }
                    catch (Exception animEx)
                    {
                        Logs.Write($"⚠️ Eroare animație (non-critical): {animEx.Message}");
                    }

                    // ✅ Actualizează status bar
                    StatusText.Text = $"Adăugat: {produs.Denumire} - {produs.Pret:F2} RON";
                }
                catch (Exception ex)
                {
                    Logs.Write($"❌ EROARE la adăugarea produsului:");
                    Logs.Write(ex);

                    MessageBox.Show($"Eroare la adăugarea produsului!\n\n{ex.Message}",
                                   "Eroare",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Error);

                    StatusText.Text = "EROARE la adăugarea produsului!";
                }
            }
            else
            {
                Logs.Write($"⚠️ Product_Click: sender={sender?.GetType().Name}, Tag={btn?.Tag?.GetType().Name}");

                MessageBox.Show("Eroare: Produsul nu a fost identificat corect!",
                               "Eroare",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);

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
    }
}
