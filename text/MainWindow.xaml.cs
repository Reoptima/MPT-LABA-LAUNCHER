using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using text;

namespace MPT_Text_Editor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            String time = DateTime.Now.ToString("HH"); // дата в часах
            int Time = Convert.ToInt32(time);
            if (Time < 12)
            {
                Hello.Text = "Доброе утро";
            }
            else if (Time >= 12 && Time < 18)
            {
                Hello.Text = "Добрый день";
            }
            else if (Time > 18)
            {
                Hello.Text = "Добрый вечер";
            }
        }

        private void Grid_MouseDown(Object sender, MouseButtonEventArgs e) // Двигать окно за края
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e) // Перейти в редактор
        {
            Edit edi = new Edit();
            edi.Show();
            Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e) // закрыть программу
        {
            Close();
            Environment.Exit(0);
        }

        private void Collapse_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized; // Свернуть окно

        private void btnMail_Click(object sender, RoutedEventArgs e)
        {
            MailWindow mani = new MailWindow();
            mani.Show();
            Close();
        }

        private void btnBrowser_Click(object sender, RoutedEventArgs e)
        {
            Browser browser = new Browser();
            browser.Show();
        }
    }
}