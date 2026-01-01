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

            // Selectează primul bon automat
            if (_bonuri.Count > 0)
                BonuriListView.SelectedIndex = 0;
        }

        /// <summary>
        /// Double-click pe bon → Încarcă automat
        /// </summary>
        private void BonuriListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IncarcaBon_Click(null, null);
        }

        /// <summary>
        /// Buton "ÎNCARCĂ BON" → Returnează bonul selectat
        /// </summary>
        private void IncarcaBon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BonuriListView.SelectedItem == null)
                {
                    MessageBox.Show("Selectează un bon din listă!",
                        "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var bonSelectat = BonuriListView.SelectedItem as BonAsteptare;

                // Încarcă detaliile complete din DB
                BonSelectat = _manager.IncarcaBon(bonSelectat.Id);

                if (BonSelectat == null)
                {
                    MessageBox.Show("Eroare la încărcarea bonului!",
                        "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
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

                    MessageBox.Show("Bon șters cu succes!",
                        "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

                    Logs.Write($"Bon ID {bonId} șters din așteptare");

                    // Închide fereastra dacă nu mai sunt bonuri
                    if (_bonuri.Count == 0)
                    {
                        MessageBox.Show("Nu mai există bonuri în așteptare.",
                            "Informație", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = false;
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la ștergerea bonului: {ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
