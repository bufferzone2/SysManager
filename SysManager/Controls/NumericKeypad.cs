using System;
using System.Windows;
using System.Windows.Controls;

namespace SysManager.Controls
{
    /// <summary>
    /// Tastatură numerică pentru introducere valori
    /// Actualizează TextBox-ul în timp real pe măsură ce utilizatorul tastează
    /// </summary>
    public class NumericKeypad : Control
    {
        #region === DEPENDENCY PROPERTIES ===

        public static readonly DependencyProperty CurrentValueProperty =
            DependencyProperty.Register(nameof(CurrentValue), typeof(string), typeof(NumericKeypad),
                new PropertyMetadata("0", OnCurrentValueChanged));

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register(nameof(MaxLength), typeof(int), typeof(NumericKeypad),
                new PropertyMetadata(10));

        public static readonly DependencyProperty AllowDecimalProperty =
            DependencyProperty.Register(nameof(AllowDecimal), typeof(bool), typeof(NumericKeypad),
                new PropertyMetadata(true));

        public static readonly DependencyProperty AllowNegativeProperty =
            DependencyProperty.Register(nameof(AllowNegative), typeof(bool), typeof(NumericKeypad),
                new PropertyMetadata(false));

        /// <summary>
        /// Valoarea curentă afișată
        /// </summary>
        public string CurrentValue
        {
            get => (string)GetValue(CurrentValueProperty);
            set => SetValue(CurrentValueProperty, value);
        }

        /// <summary>
        /// Lungimea maximă a valorii
        /// </summary>
        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        /// <summary>
        /// Permite zecimale (punct)
        /// </summary>
        public bool AllowDecimal
        {
            get => (bool)GetValue(AllowDecimalProperty);
            set => SetValue(AllowDecimalProperty, value);
        }

        /// <summary>
        /// Permite valori negative (minus)
        /// </summary>
        public bool AllowNegative
        {
            get => (bool)GetValue(AllowNegativeProperty);
            set => SetValue(AllowNegativeProperty, value);
        }

        #endregion

        #region === PRIVATE FIELDS ===

        /// <summary>
        /// Flag pentru a indica dacă trebuie să înlocuiască valoarea la prima tastă
        /// </summary>
        private bool _selectAllOnFirstKey = false;

        #endregion

        #region === EVENTS ===

        /// <summary>
        /// Event declanșat când valoarea se schimbă (în timp real)
        /// </summary>
        public event EventHandler<string> ValueChanged;

        /// <summary>
        /// Event declanșat când se apasă Enter
        /// </summary>
        public event EventHandler<decimal> ValueEntered;

        /// <summary>
        /// Event declanșat când se anulează (Cancel)
        /// </summary>
        public event EventHandler Cancelled;

        #endregion

        #region === CONSTRUCTOR ===

        static NumericKeypad()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericKeypad),
                new FrameworkPropertyMetadata(typeof(NumericKeypad)));
        }

        public NumericKeypad()
        {
            this.Loaded += NumericKeypad_Loaded;
        }

        #endregion

        #region === OVERRIDE METHODS ===

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // ✅ BUTOANE CIFRE (0-9)
            for (int i = 0; i <= 9; i++)
            {
                if (GetTemplateChild($"Btn{i}") is Button btn)
                {
                    int digit = i; // Capture variable for closure
                    btn.Click += (s, e) => AppendDigit(digit.ToString());
                }
            }

            // ✅ BUTON PUNCT
            if (GetTemplateChild("BtnDot") is Button btnDot)
            {
                btnDot.Click += (s, e) => AppendDot();
            }

            // ✅ BUTON MINUS
            if (GetTemplateChild("BtnMinus") is Button btnMinus)
            {
                btnMinus.Click += (s, e) => ToggleMinus();
            }

            // ✅ BUTON BACKSPACE
            if (GetTemplateChild("BtnBackspace") is Button btnBackspace)
            {
                btnBackspace.Click += (s, e) => Backspace();
            }

            // ✅ BUTON CLEAR
            if (GetTemplateChild("BtnClear") is Button btnClear)
            {
                btnClear.Click += (s, e) => Clear();
            }

            // ✅ BUTON ENTER
            if (GetTemplateChild("BtnEnter") is Button btnEnter)
            {
                btnEnter.Click += (s, e) => Enter();
            }

            // ✅ BUTON CANCEL
            if (GetTemplateChild("BtnCancel") is Button btnCancel)
            {
                btnCancel.Click += (s, e) => Cancel();
            }
        }

        #endregion

        #region === PRIVATE METHODS ===

        private void NumericKeypad_Loaded(object sender, RoutedEventArgs e)
        {
            // Inițializează cu "0" dacă e gol
            if (string.IsNullOrWhiteSpace(CurrentValue))
            {
                CurrentValue = "0";
            }
        }

        private static void OnCurrentValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericKeypad keypad)
            {
                // Validare valoare
                string newValue = e.NewValue?.ToString() ?? "0";

                // Înlocuiește virgula cu punct
                newValue = newValue.Replace(',', '.');

                if (newValue != e.NewValue?.ToString())
                {
                    keypad.CurrentValue = newValue;
                }

                // ✅ DECLANȘEAZĂ EVENT ValueChanged în timp real
                keypad.ValueChanged?.Invoke(keypad, newValue);
            }
        }

        /// <summary>
        /// Adaugă o cifră la valoarea curentă
        /// </summary>
        private void AppendDigit(string digit)
        {
            // ✅ DACĂ E PRIMA TASTĂ DUPĂ SETARE, ÎNLOCUIEȘTE VALOAREA
            if (_selectAllOnFirstKey)
            {
                CurrentValue = digit;
                _selectAllOnFirstKey = false;
                Logs.Write($"NumericKeypad: Prima cifră după selecție {digit} → Value: {CurrentValue}");
                return;
            }

            if (CurrentValue.Length >= MaxLength)
                return;

            // Dacă valoarea e "0", înlocuiește cu cifra
            if (CurrentValue == "0" || CurrentValue == "-0")
            {
                if (CurrentValue.StartsWith("-"))
                    CurrentValue = "-" + digit;
                else
                    CurrentValue = digit;
            }
            else
            {
                CurrentValue += digit;
            }

            Logs.Write($"NumericKeypad: Digit {digit} → Value: {CurrentValue}");
        }

        /// <summary>
        /// Adaugă punct zecimal
        /// </summary>
        private void AppendDot()
        {
            // ✅ DACĂ E PRIMA TASTĂ DUPĂ SETARE, ÎNCEPE CU "0."
            if (_selectAllOnFirstKey)
            {
                CurrentValue = "0.";
                _selectAllOnFirstKey = false;
                Logs.Write($"NumericKeypad: Prima tastă (punct) → Value: {CurrentValue}");
                return;
            }

            if (!AllowDecimal)
                return;

            // Nu permite mai mult de un punct
            if (CurrentValue.Contains("."))
                return;

            if (CurrentValue.Length >= MaxLength)
                return;

            CurrentValue += ".";
            Logs.Write($"NumericKeypad: Dot → Value: {CurrentValue}");
        }

        /// <summary>
        /// Toggle semn minus
        /// </summary>
        private void ToggleMinus()
        {
            // ✅ MINUS ANULEAZĂ SELECȚIA
            _selectAllOnFirstKey = false;

            if (!AllowNegative)
                return;

            if (CurrentValue.StartsWith("-"))
            {
                CurrentValue = CurrentValue.Substring(1);
            }
            else
            {
                CurrentValue = "-" + CurrentValue;
            }

            Logs.Write($"NumericKeypad: Minus → Value: {CurrentValue}");
        }

        /// <summary>
        /// Șterge ultima cifră (Backspace)
        /// </summary>
        private void Backspace()
        {
            // ✅ BACKSPACE ANULEAZĂ SELECȚIA ȘI ȘTERGE ULTIMA CIFRĂ (NU TOT)
            if (_selectAllOnFirstKey)
            {
                _selectAllOnFirstKey = false;
                // Nu face return - continuă să șteargă ultima cifră
            }

            if (CurrentValue.Length <= 1 || (CurrentValue.StartsWith("-") && CurrentValue.Length <= 2))
            {
                CurrentValue = "0";
            }
            else
            {
                CurrentValue = CurrentValue.Substring(0, CurrentValue.Length - 1);
            }

            Logs.Write($"NumericKeypad: Backspace → Value: {CurrentValue}");
        }

        /// <summary>
        /// Golește valoarea (Clear)
        /// </summary>
        private void Clear()
        {
            CurrentValue = "0";
            _selectAllOnFirstKey = false;
            Logs.Write("NumericKeypad: Clear → Value: 0");
        }

        /// <summary>
        /// Confirmă valoarea (Enter)
        /// </summary>
        private void Enter()
        {
            try
            {
                // Înlocuiește virgula cu punct pentru parsing
                string valueToParse = CurrentValue.Replace(',', '.');

                if (decimal.TryParse(valueToParse,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal value))
                {
                    Logs.Write($"NumericKeypad: Enter → Value: {value}");
                    ValueEntered?.Invoke(this, value);
                }
                else
                {
                    Logs.Write($"NumericKeypad: Enter FAILED → Invalid value: {CurrentValue}");
                    MessageBox.Show($"Valoare invalidă: {CurrentValue}", "Eroare",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Logs.Write($"NumericKeypad: Enter ERROR → {ex.Message}");
                MessageBox.Show($"Eroare la procesarea valorii: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Anulează introducerea
        /// </summary>
        private void Cancel()
        {
            Logs.Write("NumericKeypad: Cancel");
            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region === PUBLIC METHODS ===

        /// <summary>
        /// Resetează tastatura la valoarea inițială
        /// </summary>
        public void Reset()
        {
            CurrentValue = "0";
        }

        /// <summary>
        /// Setează o valoare inițială
        /// </summary>
        public void SetValue(decimal value)
        {
            CurrentValue = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Setează o valoare inițială și marchează pentru selecție la prima tastă
        /// Când utilizatorul apasă prima cifră, valoarea va fi înlocuită complet
        /// </summary>
        public void SetValueAndSelectOnFirstKey(decimal value)
        {
            CurrentValue = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            _selectAllOnFirstKey = true;
        }

        /// <summary>
        /// Obține valoarea ca decimal
        /// </summary>
        public decimal? GetValue()
        {
            string valueToParse = CurrentValue.Replace(',', '.');

            if (decimal.TryParse(valueToParse,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal value))
            {
                return value;
            }

            return null;
        }

        #endregion
    }
}
