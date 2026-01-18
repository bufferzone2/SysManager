//BonuriAsteptareWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SysManager.Models;
using SysManager.Managers;

namespace SysManager
{
    /// <summary>
    /// Fereastră pentru gestionarea bonurilor în așteptare
    /// </summary>
    public partial class BonuriAsteptareWindow : Window
    {
        private readonly BonuriAsteptareManager _manager;
        private List<BonAsteptare> _bonuri;
        private BonAsteptare _bonSelectat;

        /// <summary>
        /// Bonul selectat de utilizator
        /// </summary>
        public BonAsteptare BonSelectat { get; private set; }

        public BonuriAsteptareWindow(List<BonAsteptare> bonuri, BonuriAsteptareManager manager)
        {
            InitializeComponent();

            _bonuri = bonuri;
            _manager = manager;

            // Populează lista
            BonuriListView.ItemsSource = _bonuri;
        }

        /// <summary>
        /// Click pe card-ul unui bon → Selectează și încarcă
        /// </summary>
        private void BonCard_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var border = sender as Border;
                if (border == null) return;

                var bonSelectat = border.Tag as BonAsteptare;
                if (bonSelectat == null) return;

                // Salvează selecția
                _bonSelectat = bonSelectat;

                // Încarcă bonul direct (comportament dublu-click)
                IncarcaBonSelectat();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la selectarea bonului: {ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Logs.Write("EROARE selectare bon:");
                Logs.Write(ex);
            }
        }

        /// <summary>
        /// Buton "ÎNCARCĂ BON" → Returnează bonul selectat
        /// </summary>
        private void IncarcaBon_Click(object sender, RoutedEventArgs e)
        {
            if (_bonSelectat == null)
            {
                MessageBox.Show("Selectează un bon din listă apăsând pe el!",
                    "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IncarcaBonSelectat();
        }

        /// <summary>
        /// Încarcă bonul selectat din DB
        /// </summary>
        private void IncarcaBonSelectat()
        {
            try
            {
                if (_bonSelectat == null) return;

                // Încarcă detaliile complete din DB
                BonSelectat = _manager.IncarcaBon(_bonSelectat.Id);

                if (BonSelectat == null)
                {
                    MessageBox.Show("Eroare la încărcarea bonului!",
                        "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    // Șterge din DB bonul pe care l-am incarcat in grid
                    _manager.StergeBon(_bonSelectat.Id);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea bonului: {ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Logs.Write("EROARE încărcare bon din listă:");
                Logs.Write(ex);
            }
        }

        /// <summary>
        /// Buton "🗑️" pe card → Șterge bonul
        /// </summary>
        private void DeleteBon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button == null) return;

                int bonId = Convert.ToInt32(button.Tag);

                var result = MessageBox.Show(
                    "Ești sigur că vrei să ștergi acest bon?\n\nAceastă acțiune este IREVERSIBILĂ!",
                    "Confirmare Ștergere",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Șterge din DB
                    _manager.StergeBon(bonId);

                    // Șterge din listă
                    _bonuri.RemoveAll(b => b.Id == bonId);

                    // Refresh UI
                    BonuriListView.ItemsSource = null;
                    BonuriListView.ItemsSource = _bonuri;

                    // Resetează selecția dacă bonul șters era selectat
                    if (_bonSelectat != null && _bonSelectat.Id == bonId)
                        _bonSelectat = null;

                    Logs.Write($"Bon ID {bonId} șters din așteptare");

                    // Închide fereastra dacă nu mai sunt bonuri
                    if (_bonuri.Count == 0)
                    {
                        DialogResult = false;
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Write("EROARE ștergere bon:");
                Logs.Write(ex);
            }
        }

        /// <summary>
        /// Buton "ANULARE" → Închide fereastra fără modificări
        /// </summary>
        private void Anulare_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // ═══════════════════════════════════════════════════════════════
        // EVENT HANDLERS PENTRU BUTOANE SCROLL
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Buton Scroll SUS - Derulează lista în sus
        /// </summary>
        private void BtnScrollUp_Click(object sender, RoutedEventArgs e)
        {
            if (BonuriScrollViewer != null)
            {
                // Derulează cu un card în sus (aproximativ 100px)
                double scrollAmount = 100;
                double newOffset = Math.Max(0, BonuriScrollViewer.VerticalOffset - scrollAmount);
                BonuriScrollViewer.ScrollToVerticalOffset(newOffset);
            }
        }

        /// <summary>
        /// Buton Scroll JOS - Derulează lista în jos
        /// </summary>
        private void BtnScrollDown_Click(object sender, RoutedEventArgs e)
        {
            if (BonuriScrollViewer != null)
            {
                // Derulează cu un card în jos (aproximativ 100px)
                double scrollAmount = 100;
                double newOffset = Math.Min(
                    BonuriScrollViewer.ScrollableHeight,
                    BonuriScrollViewer.VerticalOffset + scrollAmount
                );
                BonuriScrollViewer.ScrollToVerticalOffset(newOffset);
            }
        }
    }
}
