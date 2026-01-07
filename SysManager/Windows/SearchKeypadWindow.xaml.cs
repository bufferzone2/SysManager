using System;
using System.Windows;
using System.Windows.Controls;

namespace SysManager.Windows
{
    /// <summary>
    /// Fereastră pentru căutarea articolelor folosind tastatura alfanumerică
    /// Actualizează TextBox-ul în TIMP REAL pe măsură ce utilizatorul tastează
    /// </summary>
    public partial class SearchKeypadWindow : Window
    {
        #region === PROPRIETĂȚI ===

        /// <summary>
        /// Textul introdus de utilizator
        /// </summary>
        public string EnteredText { get; private set; }

        /// <summary>
        /// TextBox-ul pe care îl actualizăm în timp real (dacă este setat)
        /// </summary>
        private TextBox _targetTextBox;

        #endregion

        #region === CONSTRUCTOR ===

        /// <summary>
        /// Constructor implicit - pornește gol
        /// </summary>
        public SearchKeypadWindow()
        {
            InitializeComponent();
            InitializeKeypad("", null);
        }

        /// <summary>
        /// Constructor cu text inițial
        /// </summary>
        public SearchKeypadWindow(string initialText)
        {
            InitializeComponent();
            InitializeKeypad(initialText, null);
        }

        /// <summary>
        /// Constructor cu text inițial și TextBox target pentru update în timp real
        /// </summary>
        public SearchKeypadWindow(string initialText, TextBox targetTextBox)
        {
            InitializeComponent();
            InitializeKeypad(initialText, targetTextBox);
        }

        #endregion

        #region === INIȚIALIZARE ===

        /// <summary>
        /// Inițializează tastatura cu textul inițial și event handlers
        /// </summary>
        private void InitializeKeypad(string initialText, TextBox targetTextBox)
        {
            _targetTextBox = targetTextBox;

            // Setează textul inițial
            Keypad.SetText(initialText ?? "");

            // ✅ ADAUGĂ EVENT HANDLER PENTRU SCHIMBAREA TEXTULUI ÎN TIMP REAL
            Keypad.TextChanged += Keypad_TextChanged;

            // Înregistrează event handlers existente
            Keypad.SearchRequested += Keypad_SearchRequested;
            Keypad.Cancelled += Keypad_Cancelled;

            Keypad.MaxLength = 50;

            Logs.Write($"SearchKeypadWindow: Inițializat cu textul '{initialText}'");
            if (_targetTextBox != null)
            {
                Logs.Write($"SearchKeypadWindow: TextBox target setat pentru update în timp real");
            }
        }

        #endregion

        #region === EVENT HANDLERS ===

        /// <summary>
        /// Event handler pentru schimbarea textului în timp real
        /// </summary>
        private void Keypad_TextChanged(object sender, string newText)
        {
            try
            {
                // ✅ ACTUALIZEAZĂ TEXTBOX-UL ÎN TIMP REAL
                if (_targetTextBox != null)
                {
                    _targetTextBox.Dispatcher.Invoke(() =>
                    {
                        _targetTextBox.Text = newText;
                    });

                    Logs.Write($"SearchKeypad: TextBox actualizat în timp real → {newText}");
                }
            }
            catch (Exception ex)
            {
                Logs.Write($"SearchKeypad: EROARE la actualizarea TextBox în timp real: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler pentru Enter (căutare) pe tastatură
        /// </summary>
        private void Keypad_SearchRequested(object sender, string text)
        {
            try
            {
                // Salvează textul
                EnteredText = text;

                // ✅ ACTUALIZEAZĂ TEXTBOX-UL FINAL
                if (_targetTextBox != null)
                {
                    _targetTextBox.Dispatcher.Invoke(() =>
                    {
                        _targetTextBox.Text = text;
                    });
                }

                Logs.Write($"SearchKeypadWindow: Căutare confirmată: '{text}'");

                // Închide fereastra cu succes
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Logs.Write($"SearchKeypadWindow: Eroare la confirmarea căutării: {ex.Message}");
                MessageBox.Show($"Eroare la procesarea textului:\n{ex.Message}",
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
            Logs.Write("SearchKeypadWindow: Anulat de utilizator");

            // Nu salvează textul
            EnteredText = null;

            // ✅ RESTAUREAZĂ TEXTUL INIȚIAL ÎN TEXTBOX (dacă a fost setat)
            if (_targetTextBox != null && !string.IsNullOrEmpty(_targetTextBox.Text))
            {
                string initialText = _targetTextBox.Text;
                _targetTextBox.Dispatcher.Invoke(() =>
                {
                    _targetTextBox.Text = initialText;
                });
            }

            // Închide fereastra fără succes
            DialogResult = false;
            Close();
        }

        #endregion

        #region === METODE STATICE HELPER ===

        /// <summary>
        /// Metodă statică pentru afișarea rapidă a tastaturii CU UPDATE ÎN TIMP REAL
        /// </summary>
        public static string ShowDialog(Window owner, TextBox targetTextBox, string initialText = "", string title = "CĂUTARE ARTICOL")
        {
            var window = new SearchKeypadWindow(initialText, targetTextBox)
            {
                Owner = owner,
                Title = title
            };

            bool? result = window.ShowDialog();

            return result == true ? window.EnteredText : null;
        }

        /// <summary>
        /// Metodă statică pentru afișarea tastaturii FĂRĂ TextBox target (backward compatibility)
        /// </summary>
        public static string ShowDialog(Window owner, string initialText = "", string title = "CĂUTARE ARTICOL")
        {
            var window = new SearchKeypadWindow(initialText, null)
            {
                Owner = owner,
                Title = title
            };

            bool? result = window.ShowDialog();

            return result == true ? window.EnteredText : null;
        }

        #endregion
    }
}
