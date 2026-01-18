using System;
using System.Windows;
using System.Windows.Controls;

namespace SysManager.Windows
{
    /// <summary>
    /// Fereastră NON-MODAL pentru căutarea articolelor folosind tastatura alfanumerică
    /// Se închide automat când pierde focus-ul
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

        /// <summary>
        /// Flag pentru a preveni închiderea accidentală
        /// </summary>
        private bool _isSearchInProgress = false;

        #endregion

        #region === CONSTRUCTOR ===

        /// <summary>
        /// Constructor cu text inițial și TextBox target pentru update în timp real
        /// </summary>
        public SearchKeypadWindow(string initialText, TextBox targetTextBox)
        {
            InitializeComponent();
            InitializeKeypad(initialText, targetTextBox);

            // ✅ Poziționează fereastra DUPĂ ce e loaded
            this.Loaded += Window_Loaded;
        }

        #endregion

        #region === INIȚIALIZARE ===

        /// <summary>
        /// Event handler pentru Window Loaded - poziționează fereastra la baza ecranului
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PositionWindowAtBottomCenter();
        }

        /// <summary>
        /// Poziționează fereastra la baza ecranului și centrată orizontal
        /// </summary>
        private void PositionWindowAtBottomCenter()
        {
            try
            {
                // ✅ Obține dimensiunile ecranului de lucru (fără taskbar)
                var workingArea = SystemParameters.WorkArea;

                // ✅ Asigură-te că fereastra are dimensiuni actualizate
                this.UpdateLayout();

                // ✅ Folosește ActualWidth/ActualHeight în loc de Width/Height
                double actualWidth = this.ActualWidth > 0 ? this.ActualWidth : this.Width;
                double actualHeight = this.ActualHeight > 0 ? this.ActualHeight : this.Height;

                // ✅ Calculează poziția X (centrat orizontal)
                double centerX = (workingArea.Width - actualWidth) / 2;
                if (centerX < 0) centerX = 0;

                this.Left = centerX;

                // ✅ Calculează poziția Y (la baza ecranului, cu spațiu de 10px)
                double bottomY = workingArea.Height - actualHeight - 10;
                if (bottomY < 0) bottomY = 0;

                this.Top = bottomY;

                Logs.Write($"SearchKeypadWindow: Poziționat la X={this.Left:F0}, Y={this.Top:F0} (Ecran: {workingArea.Width:F0}x{workingArea.Height:F0}, Fereastră: {actualWidth:F0}x{actualHeight:F0})");
            }
            catch (Exception ex)
            {
                Logs.Write($"SearchKeypadWindow: EROARE la poziționare: {ex.Message}");
            }
        }

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

            // Înregistrează event handler pentru căutare
            Keypad.SearchRequested += Keypad_SearchRequested;

            Keypad.MaxLength = 50;

            Logs.Write($"SearchKeypadWindow: Inițializat cu textul '{initialText}' (NON-MODAL)");
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
                _isSearchInProgress = true;

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

                // ✅ ÎNCHIDE FEREASTRA
                this.Close();
            }
            catch (Exception ex)
            {
                Logs.Write($"SearchKeypadWindow: Eroare la confirmarea căutării: {ex.Message}");
            }
            finally
            {
                _isSearchInProgress = false;
            }
        }

        /// <summary>
        /// Când fereastra pierde focus-ul, se închide automat
        /// DAR nu se închide dacă focus-ul merge la Owner (MainWindow)
        /// </summary>
        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Nu închide dacă e în progress o căutare
            if (_isSearchInProgress)
            {
                return;
            }

            // ✅ VERIFICĂ DACĂ FOCUS-UL A FOST PRELUAT DE OWNER (MainWindow)
            // Dacă da, nu închide fereastra (user a dat click pe buton în MainWindow)
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                // Verifică dacă owner-ul este activ
                if (this.Owner != null && this.Owner.IsActive)
                {
                    // Focus-ul e pe MainWindow, nu închide tastatura
                    Logs.Write("SearchKeypadWindow: Focus pe MainWindow, nu închide");
                    return;
                }

                // Verifică dacă această fereastră încă există și nu e activă
                if (!this.IsActive && this.IsLoaded)
                {
                    Logs.Write("SearchKeypadWindow: Închis automat (pierdere focus)");
                    this.Close();
                }
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        #endregion
    }
}
