using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace SysManager
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
            Loaded += SplashScreen_Loaded;
        }

        private async void SplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1));
            this.BeginAnimation(Window.OpacityProperty, fadeIn);

            await StartInitializationAsync();
        }

        private async Task StartInitializationAsync()
        {
            try
            {
                txtStatus.Text = "Verific conexiunea la baza de date...";
                await UpdateProgressAsync(30);

                bool dbOk = await Task.Run(() =>
                {
                    try
                    {
                        using (var conn = DbConnectionFactory.GetOpenConnection())
                            return conn.State == System.Data.ConnectionState.Open;
                    }
                    catch (Exception ex)
                    {
                        // 🟩 Salvăm eroarea în log
                        Logs.Write($"Eroare la conectarea bazei de date: {ex.Message}");
                        return false;
                    }
                });

                if (!dbOk)
                {
                    await ShowErrorAsync("Nu s-a putut stabili conexiunea cu baza de date Firebird.\nVerifică dacă serverul este pornit sau fișierul .fdb există.");
                    return;
                }

                txtStatus.Text = "Inițializare interfață...";
                await UpdateProgressAsync(100);
                await Task.Delay(500);

                // Fade-out și lansare MainWindow
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.7));
                this.BeginAnimation(Window.OpacityProperty, fadeOut);
                await Task.Delay(700);

                var main = new MainWindow();
                main.Show();

                Close();
            }
            catch (Exception ex)
            {
                Logs.Write($"Eroare generală la inițializare: {ex.Message}");
                await ShowErrorAsync("Eroare la inițializare: " + ex.Message);
            }
        }

        private async Task ShowErrorAsync(string message)
        {
            txtStatus.Text = "Eroare la inițializare";
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
            btnClose.Visibility = Visibility.Visible;
            await UpdateProgressAsync(100);
        }

        private async Task UpdateProgressAsync(int targetValue)
        {
            for (int i = (int)progressBar.Value; i <= targetValue; i++)
            {
                progressBar.Value = i;
                await Task.Delay(15);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
