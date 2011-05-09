using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace chatclient {
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login: Window {
        public Login() {
            InitializeComponent();

            DataContext = Properties.Settings.Default;
        }

        private void Login_Click(object sender, RoutedEventArgs e) {
            var mainWindow = new MainWindow(accountName.Text, accountPassword.Password, characterName.Text, realmName.Text);
            mainWindow.Closed += new EventHandler(mainWindow_Closed);

            mainWindow.Show();
            Hide();
        }

        void mainWindow_Closed(object sender, EventArgs e) {
            Show();

            Properties.Settings.Default.Save();
        }
    }
}
