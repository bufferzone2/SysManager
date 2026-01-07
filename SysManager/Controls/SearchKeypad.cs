using System;
using System.Windows;
using System.Windows.Controls;

namespace SysManager.Controls
{
    /// <summary>
    /// Tastatură alfanumerică pentru căutare articole
    /// </summary>
    public class SearchKeypad : Control
    {
        #region === DEPENDENCY PROPERTIES ===

        public static readonly DependencyProperty CurrentTextProperty =
            DependencyProperty.Register(nameof(CurrentText), typeof(string), typeof(SearchKeypad),
                new PropertyMetadata("", OnCurrentTextChanged));

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register(nameof(MaxLength), typeof(int), typeof(SearchKeypad),
                new PropertyMetadata(50));

        /// <summary>
        /// Textul curent
        /// </summary>
        public string CurrentText
        {
            get => (string)GetValue(CurrentTextProperty);
            set => SetValue(CurrentTextProperty, value);
        }

        /// <summary>
        /// Lungimea maximă a textului
        /// </summary>
        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        #endregion

        #region === PRIVATE FIELDS ===

        private bool _isUpperCase = false;
        private bool _capsLock = false;

        #endregion

        #region === EVENTS ===

        /// <summary>
        /// Event declanșat când textul se schimbă
        /// </summary>
        public event EventHandler<string> TextChanged;

        /// <summary>
        /// Event declanșat când se apasă Enter (căutare)
        /// </summary>
        public event EventHandler<string> SearchRequested;

        /// <summary>
        /// Event declanșat când se anulează
        /// </summary>
        public event EventHandler Cancelled;

        #endregion

        #region === CONSTRUCTOR ===

        static SearchKeypad()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchKeypad),
                new FrameworkPropertyMetadata(typeof(SearchKeypad)));
        }

        public SearchKeypad()
        {
            this.Loaded += SearchKeypad_Loaded;
        }

        #endregion

        #region === OVERRIDE METHODS ===

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // ✅ CIFRE 0-9
            for (int i = 0; i <= 9; i++)
            {
                if (GetTemplateChild($"Btn{i}") is Button btn)
                {
                    string digit = i.ToString();
                    btn.Click += (s, e) => AppendText(digit);
                }
            }

            // ✅ LITERE q, w, e, r, t, y, u, i, o, p
            string[] row1 = { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p" };
            foreach (var letter in row1)
            {
                if (GetTemplateChild($"Btn{letter.ToUpper()}") is Button btn)
                {
                    string l = letter;
                    btn.Click += (s, e) => AppendLetter(l);
                }
            }

            // ✅ LITERE a, s, d, f, g, h, j, k, l
            string[] row2 = { "a", "s", "d", "f", "g", "h", "j", "k", "l" };
            foreach (var letter in row2)
            {
                if (GetTemplateChild($"Btn{letter.ToUpper()}") is Button btn)
                {
                    string l = letter;
                    btn.Click += (s, e) => AppendLetter(l);
                }
            }

            // ✅ LITERE z, x, c, v, b, n, m
            string[] row3 = { "z", "x", "c", "v", "b", "n", "m" };
            foreach (var letter in row3)
            {
                if (GetTemplateChild($"Btn{letter.ToUpper()}") is Button btn)
                {
                    string l = letter;
                    btn.Click += (s, e) => AppendLetter(l);
                }
            }

            // ✅ BUTON SPAȚIU
            if (GetTemplateChild("BtnSpace") is Button btnSpace)
            {
                btnSpace.Click += (s, e) => AppendText(" ");
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

            // ✅ BUTON CAPS LOCK
            if (GetTemplateChild("BtnCaps") is Button btnCaps)
            {
                btnCaps.Click += (s, e) => ToggleCapsLock();
            }

            // ✅ BUTON SHIFT
            if (GetTemplateChild("BtnShift") is Button btnShift)
            {
                btnShift.Click += (s, e) => ToggleShift();
            }

            // ✅ BUTON ENTER (CĂUTARE)
            if (GetTemplateChild("BtnEnter") is Button btnEnter)
            {
                btnEnter.Click += (s, e) => Search();
            }

            // ✅ BUTON CANCEL
            if (GetTemplateChild("BtnCancel") is Button btnCancel)
            {
                btnCancel.Click += (s, e) => Cancel();
            }
        }

        #endregion

        #region === PRIVATE METHODS ===

        private void SearchKeypad_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentText))
            {
                CurrentText = "";
            }
        }

        private static void OnCurrentTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchKeypad keypad)
            {
                string newText = e.NewValue?.ToString() ?? "";
                keypad.TextChanged?.Invoke(keypad, newText);
            }
        }

        private void AppendLetter(string letter)
        {
            if (CurrentText.Length >= MaxLength)
                return;

            // Determină dacă litera trebuie să fie majusculă
            string finalLetter = (_isUpperCase || _capsLock) ? letter.ToUpper() : letter.ToLower();

            CurrentText += finalLetter;

            // Dacă e Shift (nu Caps Lock), revine la minuscule după o literă
            if (_isUpperCase && !_capsLock)
            {
                _isUpperCase = false;
                UpdateShiftState();
            }

            Logs.Write($"SearchKeypad: Letter '{finalLetter}' → Text: {CurrentText}");
        }

        private void AppendText(string text)
        {
            if (CurrentText.Length >= MaxLength)
                return;

            CurrentText += text;
            Logs.Write($"SearchKeypad: Text '{text}' → Text: {CurrentText}");
        }

        private void Backspace()
        {
            if (string.IsNullOrEmpty(CurrentText))
                return;

            if (CurrentText.Length > 0)
            {
                CurrentText = CurrentText.Substring(0, CurrentText.Length - 1);
            }

            Logs.Write($"SearchKeypad: Backspace → Text: {CurrentText}");
        }

        private void Clear()
        {
            CurrentText = "";
            Logs.Write("SearchKeypad: Clear");
        }

        private void ToggleCapsLock()
        {
            _capsLock = !_capsLock;
            _isUpperCase = _capsLock; // Sincronizează cu Shift
            UpdateShiftState();
            Logs.Write($"SearchKeypad: Caps Lock → {(_capsLock ? "ON" : "OFF")}");
        }

        private void ToggleShift()
        {
            if (!_capsLock)
            {
                _isUpperCase = !_isUpperCase;
                UpdateShiftState();
                Logs.Write($"SearchKeypad: Shift → {(_isUpperCase ? "ON" : "OFF")}");
            }
        }

        private void UpdateShiftState()
        {
            // Actualizează vizual butonul Caps/Shift dacă e nevoie
            // Poți adăuga aici logică pentru a schimba culoarea butonului
        }

        private void Search()
        {
            Logs.Write($"SearchKeypad: Search → Text: {CurrentText}");
            SearchRequested?.Invoke(this, CurrentText);
        }

        private void Cancel()
        {
            Logs.Write("SearchKeypad: Cancel");
            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region === PUBLIC METHODS ===

        /// <summary>
        /// Resetează textul
        /// </summary>
        public void Reset()
        {
            CurrentText = "";
            _isUpperCase = false;
            _capsLock = false;
        }

        /// <summary>
        /// Setează textul inițial
        /// </summary>
        public void SetText(string text)
        {
            CurrentText = text ?? "";
        }

        /// <summary>
        /// Obține textul curent
        /// </summary>
        public string GetText()
        {
            return CurrentText;
        }

        #endregion
    }
}
