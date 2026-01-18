// Locație: SysManager/Controls/BtnUniversal.cs

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SysManager.Controls
{
    /// <summary>
    /// Forme disponibile pentru badge
    /// </summary>
    public enum BadgeShape
    {
        Circle,           // Cerc perfect
        Rectangle,        // Dreptunghi
        RoundedRectangle  // Dreptunghi cu colțuri rotunjite
    }
    /// <summary>
    /// Buton universal personalizat pentru interfața POS
    /// Suportă: nume, detalii, culoare, stare selectată
    /// </summary>
    public class BtnUniversal : Button
    {
        // ═══════════════════════════════════════════════════════════════
        // DEPENDENCY PROPERTIES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Numele afișat pe buton (ex: "TOATE")
        /// </summary>
        public string BtnName
        {
            get { return (string)GetValue(BtnNameProperty); }
            set { SetValue(BtnNameProperty, value); }
        }

        public static readonly DependencyProperty BtnNameProperty =
            DependencyProperty.Register(
                "BtnName",
                typeof(string),
                typeof(BtnUniversal),
                new PropertyMetadata("Buton"));

        /// <summary>
        /// Detalii afișate jos-dreapta (ex: "(20 art)")
        /// </summary>
        public string BtnDetalii
        {
            get { return (string)GetValue(BtnDetaliiProperty); }
            set { SetValue(BtnDetaliiProperty, value); }
        }

        public static readonly DependencyProperty BtnDetaliiProperty =
            DependencyProperty.Register(
                "BtnDetalii",
                typeof(string),
                typeof(BtnUniversal),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Culoarea de fundal a butonului
        /// </summary>
        public Brush BtnColor
        {
            get { return (Brush)GetValue(BtnColorProperty); }
            set { SetValue(BtnColorProperty, value); }
        }

        public static readonly DependencyProperty BtnColorProperty =
            DependencyProperty.Register(
                "BtnColor",
                typeof(Brush),
                typeof(BtnUniversal),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 102, 204)))); // #0066CC

        /// <summary>
        /// Indică dacă butonul este selectat (activat)
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                "IsSelected",
                typeof(bool),
                typeof(BtnUniversal),
                new PropertyMetadata(false));

        /// <summary>
        /// Tag pentru date asociate (ex: ID-ul grupei)
        /// </summary>
        public object BtnTag
        {
            get { return GetValue(BtnTagProperty); }
            set { SetValue(BtnTagProperty, value); }
        }

        public static readonly DependencyProperty BtnTagProperty =
            DependencyProperty.Register(
                "BtnTag",
                typeof(object),
                typeof(BtnUniversal),
                new PropertyMetadata(null));

        /// <summary>
        /// Arată sau ascunde badge-ul (independent de valoarea BtnDetalii)
        /// </summary>
        public bool BadgeVisible
        {
            get { return (bool)GetValue(BadgeVisibleProperty); }
            set { SetValue(BadgeVisibleProperty, value); }
        }

        public static readonly DependencyProperty BadgeVisibleProperty =
            DependencyProperty.Register(
                "BadgeVisible",
                typeof(bool),
                typeof(BtnUniversal),
                new PropertyMetadata(true)); // Default: vizibil

        /// <summary>
        /// Culoarea badge-ului (fundal)
        /// </summary>
        public Brush BadgeColor
        {
            get { return (Brush)GetValue(BadgeColorProperty); }
            set { SetValue(BadgeColorProperty, value); }
        }

        public static readonly DependencyProperty BadgeColorProperty =
            DependencyProperty.Register(
                "BadgeColor",
                typeof(Brush),
                typeof(BtnUniversal),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 68, 68)))); // Default: roșu #FF4444

        /// <summary>
        /// Forma badge-ului (Circle, Rectangle, RoundedRectangle)
        /// </summary>
        public BadgeShape BadgeShape
        {
            get { return (BadgeShape)GetValue(BadgeShapeProperty); }
            set { SetValue(BadgeShapeProperty, value); }
        }

        public static readonly DependencyProperty BadgeShapeProperty =
            DependencyProperty.Register(
                "BadgeShape",
                typeof(BadgeShape),
                typeof(BtnUniversal),
                new PropertyMetadata(BadgeShape.Circle)); // Default: cerc


        /// <summary>
        /// Alinierea verticală a text-ului (Top, Center, Bottom, Stretch)
        /// </summary>
        public VerticalAlignment BtnTextVerticalAlign
        {
            get { return (VerticalAlignment)GetValue(BtnTextVerticalAlignProperty); }
            set { SetValue(BtnTextVerticalAlignProperty, value); }
        }

        public static readonly DependencyProperty BtnTextVerticalAlignProperty =
            DependencyProperty.Register(
                "BtnTextVerticalAlign",
                typeof(VerticalAlignment),
                typeof(BtnUniversal),
                new PropertyMetadata(VerticalAlignment.Center));

        /// <summary>
        /// Alinierea orizontală a text-ului (Left, Center, Right, Stretch)
        /// </summary>
        public HorizontalAlignment BtnTextHorizontalAlign
        {
            get { return (HorizontalAlignment)GetValue(BtnTextHorizontalAlignProperty); }
            set { SetValue(BtnTextHorizontalAlignProperty, value); }
        }

        public static readonly DependencyProperty BtnTextHorizontalAlignProperty =
            DependencyProperty.Register(
                "BtnTextHorizontalAlign",
                typeof(HorizontalAlignment),
                typeof(BtnUniversal),
                new PropertyMetadata(HorizontalAlignment.Center));


        /// <summary>
        /// Alinierea verticală a badge-ului (Top, Center, Bottom, Stretch)
        /// </summary>
        public VerticalAlignment BtnBadgeVerticalAlign
        {
            get { return (VerticalAlignment)GetValue(BtnBadgeVerticalAlignProperty); }
            set { SetValue(BtnBadgeVerticalAlignProperty, value); }
        }

        public static readonly DependencyProperty BtnBadgeVerticalAlignProperty =
            DependencyProperty.Register(
                "BtnBadgeVerticalAlign",
                typeof(VerticalAlignment),
                typeof(BtnUniversal),
                new PropertyMetadata(VerticalAlignment.Bottom));

        /// <summary>
        /// Alinierea orizontală a badge-ului (Left, Center, Right, Stretch)
        /// </summary>
        public HorizontalAlignment BtnBadgeHorizontalAlign
        {
            get { return (HorizontalAlignment)GetValue(BtnBadgeHorizontalAlignProperty); }
            set { SetValue(BtnBadgeHorizontalAlignProperty, value); }
        }

        public static readonly DependencyProperty BtnBadgeHorizontalAlignProperty =
            DependencyProperty.Register(
                "BtnBadgeHorizontalAlign",
                typeof(HorizontalAlignment),
                typeof(BtnUniversal),
                new PropertyMetadata(HorizontalAlignment.Right));

        /// <summary>
        /// Calea către imaginea de fundal (opțional)
        /// </summary>
        public string BtnImagePath
        {
            get { return (string)GetValue(BtnImagePathProperty); }
            set { SetValue(BtnImagePathProperty, value); }
        }

        public static readonly DependencyProperty BtnImagePathProperty =
            DependencyProperty.Register(
                "BtnImagePath",
                typeof(string),
                typeof(BtnUniversal),
                new PropertyMetadata(null)); // Default: null (fără imagine)

        /// <summary>
        /// Opacitatea imaginii de fundal (0.0 - 1.0)
        /// </summary>
        public double BtnImageOpacity
        {
            get { return (double)GetValue(BtnImageOpacityProperty); }
            set { SetValue(BtnImageOpacityProperty, value); }
        }

        public static readonly DependencyProperty BtnImageOpacityProperty =
            DependencyProperty.Register(
                "BtnImageOpacity",
                typeof(double),
                typeof(BtnUniversal),
                new PropertyMetadata(0.3)); // Default: 30% opacitate

        /// <summary>
        /// Modul de afișare a imaginii (Fill, Uniform, UniformToFill, None)
        /// </summary>
        public Stretch BtnImageStretch
        {
            get { return (Stretch)GetValue(BtnImageStretchProperty); }
            set { SetValue(BtnImageStretchProperty, value); }
        }

        public static readonly DependencyProperty BtnImageStretchProperty =
            DependencyProperty.Register(
                "BtnImageStretch",
                typeof(Stretch),
                typeof(BtnUniversal),
                new PropertyMetadata(Stretch.UniformToFill)); // Default: UniformToFill

        /// <summary>
        /// Lățimea imaginii (NaN = stretch pe toată lățimea)
        /// </summary>
        public double BtnImageWidth
        {
            get { return (double)GetValue(BtnImageWidthProperty); }
            set { SetValue(BtnImageWidthProperty, value); }
        }

        public static readonly DependencyProperty BtnImageWidthProperty =
            DependencyProperty.Register(
                "BtnImageWidth",
                typeof(double),
                typeof(BtnUniversal),
                new PropertyMetadata(double.NaN)); // Default: NaN (stretch automat)

        /// <summary>
        /// Înălțimea imaginii (NaN = stretch pe toată înălțimea)
        /// </summary>
        public double BtnImageHeight
        {
            get { return (double)GetValue(BtnImageHeightProperty); }
            set { SetValue(BtnImageHeightProperty, value); }
        }

        public static readonly DependencyProperty BtnImageHeightProperty =
            DependencyProperty.Register(
                "BtnImageHeight",
                typeof(double),
                typeof(BtnUniversal),
                new PropertyMetadata(double.NaN)); // Default: NaN (stretch automat)

        /// <summary>
        /// Alinierea orizontală a imaginii (Left, Center, Right, Stretch)
        /// </summary>
        public HorizontalAlignment BtnImageHorizontalAlign
        {
            get { return (HorizontalAlignment)GetValue(BtnImageHorizontalAlignProperty); }
            set { SetValue(BtnImageHorizontalAlignProperty, value); }
        }

        public static readonly DependencyProperty BtnImageHorizontalAlignProperty =
            DependencyProperty.Register(
                "BtnImageHorizontalAlign",
                typeof(HorizontalAlignment),
                typeof(BtnUniversal),
                new PropertyMetadata(HorizontalAlignment.Center)); // Default: Center

        /// <summary>
        /// Alinierea verticală a imaginii (Top, Center, Bottom, Stretch)
        /// </summary>
        public VerticalAlignment BtnImageVerticalAlign
        {
            get { return (VerticalAlignment)GetValue(BtnImageVerticalAlignProperty); }
            set { SetValue(BtnImageVerticalAlignProperty, value); }
        }

        public static readonly DependencyProperty BtnImageVerticalAlignProperty =
            DependencyProperty.Register(
                "BtnImageVerticalAlign",
                typeof(VerticalAlignment),
                typeof(BtnUniversal),
                new PropertyMetadata(VerticalAlignment.Center)); // Default: Center

        /// <summary>
        /// Familia de font pentru textul butonului
        /// </summary>
        public FontFamily BtnFontFamily
        {
            get { return (FontFamily)GetValue(BtnFontFamilyProperty); }
            set { SetValue(BtnFontFamilyProperty, value); }
        }

        public static readonly DependencyProperty BtnFontFamilyProperty =
            DependencyProperty.Register(
                "BtnFontFamily",
                typeof(FontFamily),
                typeof(BtnUniversal),
                new PropertyMetadata(new FontFamily("Segoe UI"))); // Default: Segoe UI

        /// <summary>
        /// Mărimea fontului pentru textul butonului
        /// </summary>
        public double BtnFontSize
        {
            get { return (double)GetValue(BtnFontSizeProperty); }
            set { SetValue(BtnFontSizeProperty, value); }
        }

        public static readonly DependencyProperty BtnFontSizeProperty =
            DependencyProperty.Register(
                "BtnFontSize",
                typeof(double),
                typeof(BtnUniversal),
                new PropertyMetadata(16.0)); // Default: 16

        /// <summary>
        /// Grosimea fontului (Normal, Bold, etc.)
        /// </summary>
        public FontWeight BtnFontWeight
        {
            get { return (FontWeight)GetValue(BtnFontWeightProperty); }
            set { SetValue(BtnFontWeightProperty, value); }
        }

        public static readonly DependencyProperty BtnFontWeightProperty =
            DependencyProperty.Register(
                "BtnFontWeight",
                typeof(FontWeight),
                typeof(BtnUniversal),
                new PropertyMetadata(FontWeights.Bold)); // Default: Bold

        /// <summary>
        /// Stilul fontului (Normal, Italic, Oblique)
        /// </summary>
        public FontStyle BtnFontStyle
        {
            get { return (FontStyle)GetValue(BtnFontStyleProperty); }
            set { SetValue(BtnFontStyleProperty, value); }
        }

        public static readonly DependencyProperty BtnFontStyleProperty =
            DependencyProperty.Register(
                "BtnFontStyle",
                typeof(FontStyle),
                typeof(BtnUniversal),
                new PropertyMetadata(FontStyles.Normal)); // Default: Normal




        // ═══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════

        static BtnUniversal()
        {
            // Înregistrează stilul default din Generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BtnUniversal),
                new FrameworkPropertyMetadata(typeof(BtnUniversal)));
        }

        public BtnUniversal()
        {
            // Setări default
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        // ═══════════════════════════════════════════════════════════════
        // METODE HELPER
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Setează butonul ca selectat și deselectează restul din același parent
        /// </summary>
        public void SelectExclusive()
        {
            if (this.Parent is Panel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is BtnUniversal btn)
                    {
                        btn.IsSelected = false;
                    }
                }
            }
            this.IsSelected = true;
        }
    }
}