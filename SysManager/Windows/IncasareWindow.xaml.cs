using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SysManager.Models;

namespace SysManager.Windows
{
    public partial class IncasareWindow : Window
    {
        // Date
        private decimal _totalDePlata;
        private Dictionary<TipMetodaPlata, decimal> _plati;
        private string _cuiBon;

        public IncasareWindow(decimal totalDePlata, string cuiBon = null)
        {
            InitializeComponent();

            _totalDePlata = totalDePlata;
            _cuiBon = cuiBon ?? "";
            _plati = new Dictionary<TipMetodaPlata, decimal>();

            // Inițializare UI
            TxtTotalDePlata.Text = $"{_totalDePlata:F2} lei";
            TxtCuiBon.Text = string.IsNullOrEmpty(_cuiBon) ? "fără CUI" : _cuiBon;
        }

        // ═══════════════════════════════════════════════════════════════
        // EVENT HANDLERS - METODE DE PLATĂ
        // ═══════════════════════════════════════════════════════════════

        private void BtnNumerar_Click(object sender, RoutedEventArgs e)
        {
            //DeschideFereastraIntroducereSuma(TipMetodaPlata.Numerar, "NUMERAR");
            AdaugaPlata(TipMetodaPlata.Numerar, _totalDePlata);
        }

        private void BtnCard_Click(object sender, RoutedEventArgs e)
        {
            //DeschideFereastraIntroducereSuma(TipMetodaPlata.Card, "CARD");
            AdaugaPlata(TipMetodaPlata.Card, _totalDePlata);
        }

        private void BtnBacsis_Click(object sender, RoutedEventArgs e)
        {
            //DeschideFereastraIntroducereSuma(TipMetodaPlata.Bacsis, "BACSIS");
        }


/*        private void BtnAltele_Click(object sender, RoutedEventArgs e)
        {
            // Deschide fereastră cu metode alternative
            var alteleWindow = new MetodePlataAlteleWindow(_totalDePlata, GetRestDePlata());
            if (alteleWindow.ShowDialog() == true)
            {
                // Adaugă metoda selectată
                if (alteleWindow.MetodaSelectata != null && alteleWindow.SumaIntrodusa > 0)
                {
                    AdaugaPlata(alteleWindow.MetodaSelectata.Value, alteleWindow.SumaIntrodusa);
                }
            }
        }*/


        // ═══════════════════════════════════════════════════════════════
        // ADAUGĂ PLATĂ ȘI ACTUALIZEAZĂ UI
        // ═══════════════════════════════════════════════════════════════

        private void AdaugaPlata(TipMetodaPlata tip, decimal suma)
        {
            // Adaugă sau actualizează suma pentru această metodă
            if (_plati.ContainsKey(tip))
                _plati[tip] += suma;
            else
                _plati[tip] = suma;

            // TODO: Salvează încasarea în baza de date
            SalveazaIncasare();

            Logs.Write($"Plată adăugată: {tip} = {suma:F2} lei");
        }


        // ═══════════════════════════════════════════════════════════════
        // SALVEAZĂ ÎNCASARE ÎN BD
        // ═══════════════════════════════════════════════════════════════

        private void SalveazaIncasare()
        {
            try
            {
                Logs.Write("═══ SALVARE ÎNCASARE ═══");
                Logs.Write($"Total de plată: {_totalDePlata:F2}");

                foreach (var plata in _plati)
                {
                    Logs.Write($"  {plata.Key}: {plata.Value:F2} lei");
                }

                // TODO: Insert în baza de date
                // DbQuery.SalveazaIncasare(idBon, _plati, GetRestDePlata());

                Logs.Write("Încasare salvată cu succes!");
            }
            catch (Exception ex)
            {
                Logs.Write($"EROARE la salvare încasare: {ex.Message}");
                MessageBox.Show($"Eroare la salvare încasare: {ex.Message}",
                                "Eroare",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // BUTON RENUNȚ
        // ═══════════════════════════════════════════════════════════════

        private void BtnRenunt_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}