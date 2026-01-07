using System;
using System.Windows;
using System.Windows.Controls;

namespace SysManager.Windows
{
    /// <summary>
    /// Fereastră pentru introducerea cantității folosind tastatura numerică
    /// Actualizează TextBox-ul în TIMP REAL pe măsură ce utilizatorul tastează
    /// </summary>
    public partial class NumericKeypadWindow : Window
    {
        #region === PROPRIETĂȚI ===

        /// <summary>
        /// Valoarea introdusă de utilizator
        /// </summary>
        public decimal? EnteredValue { get; private set; }

        /// <summary>
        /// TextBox-ul pe care îl actualizăm în timp real (dacă este setat)
        /// </summary>
        private TextBox _targetTextBox;

        /// <summary>
        /// Titlul ferestrei (opțional)
        /// </summary>
        //public string WindowTitle
        //{
        //    get => TitleText.Text;
        //    set => TitleText.Text = value;
        //}

        #endregion

        #region === CONSTRUCTOR ===

        /// <summary>
        /// Constructor implicit - pornește cu valoarea 1
        /// </summary>
        public NumericKeypadWindow()
        {
            InitializeComponent();
            InitializeKeypad(1, null);
        }

        /// <summary>
        /// Constructor cu valoare inițială
        /// </summary>
        /// <param name="initialValue">Valoarea inițială afișată</param>
        public NumericKeypadWindow(decimal initialValue)
        {
            InitializeComponent();
            InitializeKeypad(initialValue, null);
        }

        /// <summary>
        /// Constructor cu valoare inițială și titlu custom
        /// </summary>
        /// <param name="initialValue">Valoarea inițială</param>
        /// <param name="title">Titlul ferestrei</param>
        public NumericKeypadWindow(decimal initialValue, string title)
        {
            InitializeComponent();
            //WindowTitle = title;
            InitializeKeypad(initialValue, null);
        }

        /// <summary>
        /// Constructor cu valoare inițială, titlu și TextBox target pentru update în timp real
        /// </summary>
        /// <param name="initialValue">Valoarea inițială</param>
        /// <param name="title">Titlul ferestrei</param>
        /// <param name="targetTextBox">TextBox-ul care va fi actualizat în timp real</param>
        public NumericKeypadWindow(decimal initialValue, string title, TextBox targetTextBox)
        {
            InitializeComponent();
            //WindowTitle = title;
            InitializeKeypad(initialValue, targetTextBox);
        }

        #endregion

        #region === INIȚIALIZARE ===

        /// <summary>
        /// Inițializează tastatura cu valoarea inițială și event handlers
        /// </summary>
        private void InitializeKeypad(decimal initialValue, TextBox targetTextBox)
        {
            _targetTextBox = targetTextBox;

            // ✅ SETEAZĂ VALOAREA INIȚIALĂ ȘI MARCHEAZĂ PENTRU SELECȚIE
            Keypad.SetValueAndSelectOnFirstKey(initialValue);

            // ✅ ADAUGĂ EVENT HANDLER PENTRU SCHIMBAREA VALORII ÎN TIMP REAL
            Keypad.ValueChanged += Keypad_ValueChanged;

            // Înregistrează event handlers existente
            Keypad.ValueEntered += Keypad_ValueEntered;
            Keypad.Cancelled += Keypad_Cancelled;

            // Permite zecimale, interzice negative (pentru cantități)
            Keypad.AllowDecimal = true;
            Keypad.AllowNegative = false;
            Keypad.MaxLength = 10;

            Logs.Write($"NumericKeypadWindow: Inițializat cu valoarea {initialValue} (selectat)");
            if (_targetTextBox != null)
            {
                Logs.Write($"NumericKeypadWindow: TextBox target setat pentru update în timp real");
            }
        }

        #endregion

        #region === EVENT HANDLERS ===

        /// <summary>
        /// Event handler pentru schimbarea valorii în timp real
        /// </summary>
        private void Keypad_ValueChanged(object sender, string newValue)
        {
            try
            {
                // ✅ ACTUALIZEAZĂ TEXTBOX-UL ÎN TIMP REAL
                if (_targetTextBox != null)
                {
                    _targetTextBox.Dispatcher.Invoke(() =>
                    {
                        _targetTextBox.Text = newValue;
                    });

                    Logs.Write($"NumericKeypad: TextBox actualizat în timp real → {newValue}");
                }
            }
            catch (Exception ex)
            {
                Logs.Write($"NumericKeypad: EROARE la actualizarea TextBox în timp real: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler pentru Enter pe tastatură
        /// </summary>
        private void Keypad_ValueEntered(object sender, decimal value)
        {
            try
            {
                // Validează valoarea (nu permite 0 sau negative pentru cantități)
                if (value <= 0)
                {
                    MessageBox.Show("Cantitatea trebuie să fie mai mare decât 0!",
                        "Valoare invalidă",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    Keypad.SetValue(1); // Resetează la 1
                    return;
                }

                // Salvează valoarea
                EnteredValue = value;

                // ✅ ACTUALIZEAZĂ TEXTBOX-UL FINAL (cu format F3)
                if (_targetTextBox != null)
                {
                    _targetTextBox.Dispatcher.Invoke(() =>
                    {
                        _targetTextBox.Text = value.ToString("F3");
                    });
                }

                Logs.Write($"NumericKeypadWindow: Valoare confirmată: {value}");

                // Închide fereastra cu succes
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Logs.Write($"NumericKeypadWindow: Eroare la confirmarea valorii: {ex.Message}");
                MessageBox.Show($"Eroare la procesarea valorii:\n{ex.Message}",
                    "Eroare",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Event handler pentru Cancel pe tastatură
        /// </summary>
        private void Keypad_Cancelled(object sender, EventArgs e)
        {
            Logs.Write("NumericKeypadWindow: Anulat de utilizator");

            // Nu salvează valoarea
            EnteredValue = null;

            // ✅ RESTAUREAZĂ VALOAREA INIȚIALĂ ÎN TEXTBOX (dacă a fost setată)
            if (_targetTextBox != null)
            {
                decimal initialValue = 1;
                if (decimal.TryParse(_targetTextBox.Text, out decimal val))
                {
                    initialValue = val;
                }

                _targetTextBox.Dispatcher.Invoke(() =>
                {
                    _targetTextBox.Text = initialValue.ToString("F3");
                });
            }

            // Închide fereastra fără succes
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Event handler pentru butonul X (închide)
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Logs.Write("NumericKeypadWindow: Închis prin butonul X");

            // Tratează ca și Cancel
            Keypad_Cancelled(this, EventArgs.Empty);
        }

        #endregion

        #region === METODE STATICE HELPER ===

        /// <summary>
        /// Metodă statică pentru afișarea rapidă a tastaturii CU UPDATE ÎN TIMP REAL
        /// </summary>
        /// <param name="owner">Fereastra părinte</param>
        /// <param name="targetTextBox">TextBox-ul care va fi actualizat în timp real</param>
        /// <param name="initialValue">Valoarea inițială (default: 1)</param>
        /// <param name="title">Titlul ferestrei (default: "INTRODUCERE CANTITATE")</param>
        /// <returns>Valoarea introdusă sau null dacă s-a anulat</returns>

        public static decimal? ShowDialog(Window owner, TextBox targetTextBox, decimal initialValue = 1,
            string title = "INTRODUCERE CANTITATE", Point? position = null)
        {
            var window = new NumericKeypadWindow(initialValue, title, targetTextBox)
            {
                Owner = owner
            };

            // Dacă s-a specificat o poziție, folosește WindowStartupLocation.Manual
            if (position.HasValue)
            {
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = position.Value.X;
                window.Top = position.Value.Y;
            }

            bool? result = window.ShowDialog();

            return result == true ? window.EnteredValue : null;
        }

        /// <summary>
        /// Metodă statică pentru afișarea tastaturii FĂRĂ TextBox target (backward compatibility)
        /// </summary>
        public static decimal? ShowDialog(Window owner, decimal initialValue = 1, string title = "INTRODUCERE CANTITATE")
        {
            var window = new NumericKeypadWindow(initialValue, title, null)
            {
                Owner = owner
            };

            bool? result = window.ShowDialog();

            return result == true ? window.EnteredValue : null;
        }

        #endregion
    }
}
