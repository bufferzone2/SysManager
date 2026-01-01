// MainWindow.Groups.cs
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using SysManager.Controls;

namespace SysManager
{
    /// <summary>
    /// Partial class pentru gestionarea grupelor de articole
    /// </summary>
    public partial class MainWindow : Window
    {
        #region === CALCUL LAYOUT GRUPE ===

        /// <summary>
        /// Calculează layout-ul pe baza setărilor din baza de date pentru grupe
        /// </summary>
        private void CalculateLayout()
        {
            try
            {
                if (_grupeSettings == null)
                    return;

                var border = GroupsPanel.Parent as System.Windows.Controls.Border;

                // ✅ LĂȚIME DISPONIBILĂ
                double availableWidth = GroupsPanel.ActualWidth;
                if (availableWidth <= 0)
                {
                    if (border != null && border.ActualWidth > 0)
                    {
                        availableWidth = border.ActualWidth - 10;
                    }
                    else
                    {
                        availableWidth = this.ActualWidth - 380 - 30;
                    }
                }

                // ✅ ÎNĂLȚIME PANOU - FOLOSEȘTE DIRECT VALOAREA DIN DB!
                double panouHeight = _grupeSettings.PanouHeight > 0
                    ? _grupeSettings.PanouHeight
                    : 125; // fallback dacă e 0

                Logs.Write($"CalculateLayout: Folosim PANOU_HEIGHT din DB = {panouHeight}px");

                // ✅ DIMENSIUNI BUTOANE
                double buttonWidth = _grupeSettings.Latime;
                double buttonHeight = _grupeSettings.Inaltime + 4;

                // ✅ CALCUL COLOANE
                _columnsPerRow = Math.Max(1, (int)Math.Floor((availableWidth - 10) / buttonWidth));

                // ✅ CALCUL RÂNDURI
                _rowsPerPage = Math.Max(1, (int)Math.Floor(panouHeight / buttonHeight));

                // ✅ SETARE UNIFORMGRID
                GroupsPanel.Columns = _columnsPerRow;
                GroupsPanel.Rows = _rowsPerPage;
                GroupsPanel.Height = _rowsPerPage * buttonHeight;

                _buttonsPerPage = _columnsPerRow * _rowsPerPage;

                Logs.Write($"  → Buton: {_grupeSettings.Latime}x{_grupeSettings.Inaltime}px (+4px margin)");
                Logs.Write($"  → Grid: {_columnsPerRow} col × {_rowsPerPage} rows = {_buttonsPerPage} butoane/pagină");
                Logs.Write($"  → UniformGrid.Height = {GroupsPanel.Height:F0}px");
            }
            catch (Exception ex)
            {
                Logs.Write("EROARE la calcularea layout-ului:");
                Logs.Write(ex);
                _buttonsPerPage = 12;
                _columnsPerRow = 12;
                _rowsPerPage = 1;
            }
        }

        #endregion

        #region === ÎNCĂRCARE GRUPE ===

        /// <summary>
        /// Încarcă butoanele pentru grupe
        /// </summary>
        private void LoadGroups()
        {
            try
            {
                Logs.Write($"LoadGroups: START încărcare grupe pentru gestiune ID={_selectedGestiuneId}");

                _allButtons.Clear();

                // ✅ ÎNCARCĂ GRUPELE PENTRU GESTIUNEA SELECTATĂ
                var grupe = _dbQuery.GetGrupeArticole(gestiuneId: _selectedGestiuneId);

                int totalArticole = grupe.Sum(g => g.NumarArticole);

                Logs.Write($"📊 GRUPE PENTRU GESTIUNE {_selectedGestiuneId}: {grupe.Count} grupe, {totalArticole} articole");

                // ✅ 1) TOATE GRUPELE (primul buton)
                var btnAll = new GroupButton
                {
                    GroupName = "TOATE GRUPELE",
                    ProductCount = $"({totalArticole} art)", // ✅ MODIFICAT: STRING în loc de int
                    GroupColor = new SolidColorBrush(Color.FromRgb(0, 102, 204)),
                    GroupId = 0,
                    Width = _grupeSettings.Latime,
                    Height = _grupeSettings.Inaltime,
                    Margin = new Thickness(2),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    IsSelected = true
                };
                btnAll.Click += Grupa_Click;
                _allButtons.Add(btnAll);

                // ✅ 2) TOATE GRUPELE DIN BAZA DE DATE
                foreach (var grupa in grupe)
                {
                    var btn = new GroupButton
                    {
                        GroupName = grupa.Denumire,
                        ProductCount = $"({grupa.NumarArticole} art)", // ✅ MODIFICAT: STRING în loc de int
                        GroupColor = new SolidColorBrush(Color.FromRgb(0, 176, 240)),
                        GroupId = grupa.Id,
                        Width = _grupeSettings.Latime,
                        Height = _grupeSettings.Inaltime,
                        Margin = new Thickness(2),
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    btn.Click += Grupa_Click;
                    _allButtons.Add(btn);
                }

                Logs.Write($"📊 TOTAL BUTOANE CREATE: {_allButtons.Count} (1 TOATE GRUPELE + {grupe.Count} grupe)");

                // ✅ CALCUL PAGINARE
                _totalPages = (int)Math.Ceiling((double)_allButtons.Count / _buttonsPerPage);
                _currentPage = 0;

                Logs.Write($"📊 PAGINARE CALCULATĂ:");
                Logs.Write($"   → Butoane totale: {_allButtons.Count}");
                Logs.Write($"   → Butoane per pagină: {_buttonsPerPage}");
                Logs.Write($"   → Total pagini: {_totalPages}");

                //TotalProducts.Text = $"{totalArticole} articole";
                //SelectedGroup.Text = "Toate grupele";

                DisplayCurrentPage();

                // ✅ Mesaj în funcție de gestiune
                string gestiuneName = _selectedGestiuneId == 0
                    ? "TOATE GESTIUNILE"
                    : _gestiuni.FirstOrDefault(g => g.Id == _selectedGestiuneId)?.DisplayName ?? "Gestiune";

                StatusText.Text = $"Încărcate {grupe.Count} grupe ({gestiuneName})";
                Logs.Write($"LoadGroups: SUCCESS");
            }
            catch (Exception ex)
            {
                Logs.Write("EROARE la încărcarea grupelor:");
                Logs.Write(ex);
                StatusText.Text = "EROARE la încărcarea grupelor!";
                MessageBox.Show($"Eroare la încărcarea grupelor:\n{ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #endregion

        #region === AFIȘARE PAGINI GRUPE ===

        /// <summary>
        /// Afișează pagina curentă de grupe
        /// </summary>
        private void DisplayCurrentPage()
        {
            try
            {
                GroupsPanel.Children.Clear();

                int start = _currentPage * _buttonsPerPage;
                int end = Math.Min(start + _buttonsPerPage, _allButtons.Count);

                // ✅ ADAUGĂ BUTOANELE
                for (int i = start; i < end; i++)
                {
                    GroupsPanel.Children.Add(_allButtons[i]);
                }

                GroupsPanel.UpdateLayout();

                // ✅ DEBUG: Verifică dimensiuni REALE
                Logs.Write($"🔍 DEBUG DIMENSIUNI DUPĂ ADĂUGARE:");
                Logs.Write($"   GroupsPanel.Children.Count = {GroupsPanel.Children.Count}");
                Logs.Write($"   GroupsPanel.ActualWidth = {GroupsPanel.ActualWidth}");
                Logs.Write($"   GroupsPanel.ActualHeight = {GroupsPanel.ActualHeight}");

                UpdateNavigationButtons();

                Logs.Write($"📄 PAGINA {_currentPage + 1}/{_totalPages}: Afișate {end - start} butoane (index {start}-{end - 1})");
            }
            catch (Exception ex)
            {
                Logs.Write("EROARE la afișarea paginii:");
                Logs.Write(ex);
            }
        }

        /// <summary>
        /// Actualizează butoanele de navigare pentru grupe
        /// </summary>
        private void UpdateNavigationButtons()
        {
            BtnPrevPage.IsEnabled = _currentPage > 0;
            BtnNextPage.IsEnabled = _currentPage < _totalPages - 1;
            PageInfo.Text = $"{_currentPage + 1}/{_totalPages}";
        }

        #endregion

        #region === EVENT HANDLERS GRUPE ===

        /// <summary>
        /// Handler pentru click pe buton grupă
        /// </summary>
        private void Grupa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is GroupButton btn)
                {
                    // ✅ DESELECTEAZĂ TOATE BUTOANELE DE GRUPE
                    foreach (var groupBtn in _allButtons)
                    {
                        groupBtn.IsSelected = false;
                    }

                    // ✅ SELECTEAZĂ BUTONUL CURENT
                    btn.IsSelected = true;
                    _selectedGrupaId = btn.GroupId;

                    // ✅ Actualizează interfața
                    StatusText.Text = $"Grupă: {btn.GroupName} ({btn.ProductCount} produse)";
                    //SelectedGroup.Text = btn.GroupName;

                    // ✅ Mesaj diferit pentru "TOATE GRUPELE"
                    if (btn.GroupId == 0)
                    {
                        Logs.Write($"📂 Selectat: TOATE GRUPELE");
                    }
                    else
                    {
                        Logs.Write($"📂 Grupă selectată: '{btn.GroupName}' (ID: {btn.GroupId})");
                    }

                    // ✅ REÎNCARCĂ PRODUSELE
                    LoadProducts();
                }
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE la selectarea grupei:");
                Logs.Write(ex);
            }
        }

        /// <summary>
        /// Pagina anterioară de grupe
        /// </summary>
        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                DisplayCurrentPage();
            }
        }

        /// <summary>
        /// Pagina următoare de grupe
        /// </summary>
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages - 1)
            {
                _currentPage++;
                DisplayCurrentPage();
            }
        }

        #endregion
    }
}
