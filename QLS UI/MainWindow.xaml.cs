using sck_server;
using SuperSocket.SocketEngine.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace QLS_UI
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer T_ExecutionTime = new Timer(1000);
        TimeSpan Temps = new TimeSpan();
        private ZoneckServer serveur;

        public MainWindow()
        {
            InitializeComponent();

            txtBox_ip.Text = Properties.Settings.Default.ip;
            txtBox_port.Text = Properties.Settings.Default.port;
        }

        /// <summary>
        /// Lance le serveur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Start_Click(object button, RoutedEventArgs _e)
        {
            try
            {
                serveur = new ZoneckServer(txtBox_ip.Text, Convert.ToInt32(txtBox_port.Text), DebugMessage);

                Properties.Settings.Default.ip = txtBox_ip.Text;
                Properties.Settings.Default.port = txtBox_port.Text;
                Properties.Settings.Default.Save();

                rtb_logs.AppendText(DateTime.Now.ToString() + " - Lancement du serveur \r\n");

                Grid_Setup.Visibility = Visibility.Hidden;
                Grid_Logs.Visibility = Visibility.Visible;

                label_ipport.Text = txtBox_ip.Text + " - " + Convert.ToInt32(txtBox_port.Text);
                T_ExecutionTime.Elapsed += (sender, e) =>
                {
                    Temps = Temps.Add(new TimeSpan(0, 0, 1));

                    Dispatcher.Invoke(() =>
                    {
                        label_temps.Content = "temps : " + Temps.ToString(@"hh\:mm\:ss");
                    });
                };

                T_ExecutionTime.Start();
            }
            catch 
            {
                MessageBox.Show("Le serveur n'a pas réussi à se lancer. Veuillez vérifier les informations.", "Mauvaise(s) information(s)", MessageBoxButton.OK, MessageBoxImage.Error);
                txtBox_port.BorderBrush = Brushes.Red;
                txtBox_ip.BorderBrush = Brushes.Red;
            }
        }

        private void DebugMessage(string obj)
        {
            Dispatcher.Invoke(() =>
            {
                if (obj.Contains(" - Nombre de personne(s) connectée(s) au serveur : "))
                {            
                    label_connecter.Content = "connecté : " + obj.Remove(0, obj.LastIndexOf(':') + 2).Trim();
                }
                if (checkbox_logs.IsChecked == true)
                { 
                    rtb_logs.AppendText(obj + "\r\n");
                    rtb_logs.ScrollToEnd();
                }
            });
        }

        private void Button_StopStartServer_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Content.ToString() == "Stop")
            {
                serveur.StopServer();
                T_ExecutionTime.Stop();

                rtb_logs.AppendText("\n" + DateTime.Now.ToString() + " - Arrêt du serveur" + "\r\n");
                rtb_logs.ScrollToEnd();

                (sender as Button).Content = "Lancer";
            }
            else
            {
                (sender as Button).Content = "Stop";
                try
                {
                    serveur = new ZoneckServer(txtBox_ip.Text, Convert.ToInt32(txtBox_port.Text), DebugMessage);
                    rtb_logs.AppendText("\n" + DateTime.Now.ToString() + " - Lancement du serveur" + "\r\n");
                    rtb_logs.ScrollToEnd();
                }
                catch
                {
                    MessageBox.Show("Le serveur n'a pas réussi à se lancer. Veuillez vérifier les informations.", "Mauvaise(s) information(s)", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
