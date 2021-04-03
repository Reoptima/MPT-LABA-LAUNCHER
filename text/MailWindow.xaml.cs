using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using mail;
using Microsoft.Win32;
using MPT_Text_Editor;

namespace text
{
    /// <summary>
    /// Логика взаимодействия для MailWindow.xaml
    /// </summary>
    public partial class MailWindow : Window
    {
        private string userMail;
        private string userPassword;
        private List<string> filePathList = new List<string>();

        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
        private bool debug = false;
        private static string path_TempDirectory = Directory.GetCurrentDirectory() + "\\Temp";
        private static string path_FilesDirectory = path_TempDirectory + "\\sendFile";
        private static string path_SendRtfFile = path_TempDirectory + "\\sendrtf.rtf";
        private static string path_DraftVersion_RtfFile = path_TempDirectory + "\\draftversionrtf.rtf";

        private MailClass draftVersionMail = new MailClass();
        private bool IsDraftVersionMail = false;

        public MailWindow()
        {
            InitializeComponent();
            tb_readme.Text =
                "Инструкция\n\nДля начала работы с программой вам необходимо:\n1) В настройках безопастности дать доступ к вашей почте сторонним устройствам и приложениям\n2) Включить протоколы POP и IMAP чтобы письма отправлялись по DHCP\n3) Войти в аккаунт\n\n\n          PROFIT";
            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            cmbFontSize.ItemsSource = new List<double>() {8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72};
            alignment.ItemsSource = new List<String>() {"Лево", "Центр", "Право", "Ширина"};
            if (Directory.Exists(path_TempDirectory))
            {
                Directory.Delete(path_TempDirectory, true);
            }

            if (debug)
            {
                coreForm.Width = 900;
                coreForm.Height = 500;
                EditMailMenu.Visibility = Visibility.Visible;
                EditMailMenu.IsEnabled = true;
                brdReadme.Visibility = Visibility.Collapsed;

                LoginMenu.Visibility = Visibility.Hidden;
                LoginMenu.IsEnabled = false;

                userMail = "Email";
                userPassword = "Password";

                toMail.Text = userMail;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (LoginEmail.Text == "" || LoginPassword.Password == "") return;
            if (!isValidEmail(LoginEmail.Text))
            {
                showErrorLogin("Неверный формат email");
                return;
            }

            MailAddress from = new MailAddress(LoginEmail.Text, "Tom");
            MailAddress to = new MailAddress(LoginEmail.Text);
            MailMessage m = new MailMessage(from, to);
            m.Subject = "Вход в аккаунт";
            m.Body = "Вы вошли в свой аккаунт через приложение MPT_LABA_LAUNCHER";
            m.IsBodyHtml = true;
            smtp.Credentials = new NetworkCredential(LoginEmail.Text, LoginPassword.Password);
            smtp.EnableSsl = true;
            try
            {
                smtp.Send(m);
                userMail = LoginEmail.Text;
                userPassword = LoginPassword.Password;
                LoginMenu.Visibility = Visibility.Hidden;
                LoginMenu.IsEnabled = false;
                EditMailMenu.Visibility = Visibility.Visible;
                EditMailMenu.IsEnabled = true;
                brdReadme.Visibility = Visibility.Collapsed;
                coreForm.Height = 500;
                coreForm.Width = 900;
                toMail.Text = userMail;
            }
            catch (Exception exception)
            {
                showErrorLogin("Неверный email или пароль");
            }
        }

        private void Grid_MouseDown(Object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void exit_Click(object sender, RoutedEventArgs e) // Закрыть программу
        {
            Close();
            Environment.Exit(0);
        }

        private void Collapse_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized; // Свернуть окошко

        private void MenuExit_Click(object sender, RoutedEventArgs e) // Вернуться на первое окно
        {
            MainWindow mani = new MainWindow();
            mani.Show();
            Close();
        }

        private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontFamily.SelectedItem != null)
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
        }

        private void cmbFontSize_TextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            if (Int32.TryParse(cmbFontSize.Text, out int n))
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text);
            }
        }

        private void Alignment_TextChanged(object sender, SelectionChangedEventArgs e)
        {
            var Alignment = TextAlignment.Left;
            switch (alignment.SelectedValue)
            {
                case "По левой":
                    Alignment = TextAlignment.Left;
                    break;
                case "По центру":
                    Alignment = TextAlignment.Center;
                    break;
                case "По правой":
                    Alignment = TextAlignment.Right;
                    break;
                case "По ширине":
                    Alignment = TextAlignment.Justify;
                    break;
            }

            rtbEditor.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, Alignment);
        }

        private void rtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                object temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
                btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
                temp = rtbEditor.Selection.GetPropertyValue(Inline.FontStyleProperty);
                btnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
                temp = rtbEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty);
                cmbFontFamily.SelectedItem = temp;
                temp = rtbEditor.Selection.GetPropertyValue(Inline.FontSizeProperty);
                cmbFontSize.Text = temp.ToString() != "{DependencyProperty.UnsetValue}" ? temp.ToString() : "";
                temp = rtbEditor.Selection.GetPropertyValue(Paragraph.TextAlignmentProperty);
                switch ((TextAlignment) temp)
                {
                    case TextAlignment.Left:
                        alignment.SelectedIndex = 0;
                        break;
                    case TextAlignment.Center:
                        alignment.SelectedIndex = 1;
                        break;
                    case TextAlignment.Right:
                        alignment.SelectedIndex = 2;
                        break;
                    case TextAlignment.Justify:
                        alignment.SelectedIndex = 3;
                        break;
                }
            }
            catch (Exception exception)
            {
                // ignored
            }
        }

        // ----Отправка письма---- //
        string titl;

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            SendButton.IsEnabled = false;
            ProgressSend.Visibility = Visibility.Collapsed;

            Directory.CreateDirectory(path_TempDirectory);
            FileStream fileStream = new FileStream(path_SendRtfFile, FileMode.Create);
            TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            range.Save(fileStream, DataFormats.Rtf);
            titl = Titleline.Text;
            fileStream.Close();
            if (isValidEmail(whomMail.Text))
            {
                whomMailList.Items.Add(whomMail.Text);
            }

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool send = false;
            SautinSoft.RtfToHtml r = new SautinSoft.RtfToHtml();
            string rtfString = File.ReadAllText(path_SendRtfFile);
            r.ImageStyle.IncludeImageInHtml = true;
            string text = r.ConvertString(rtfString);
            int n = 1;
            foreach (var vari in whomMailList.Items)
            {
                int progressPercentage = Convert.ToInt32(((double) n++ / whomMailList.Items.Count) * 100);
                (sender as BackgroundWorker).ReportProgress(progressPercentage);
                send_mes(titl, text, filePathList, (string) vari);
                send = true;
                System.Threading.Thread.Sleep(1000);
            }

            e.Result = send;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressSend.Value = e.ProgressPercentage;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool) e.Result)
            {
                foreach (var vari in whomMailList.Items)
                {
                    if (!IsEmptyListView(whomMailListLast, (string) vari))
                    {
                        whomMailListLast.Items.Add((string) vari);
                    }
                }

                whomMailList.Items.Clear();
                rtbEditor.Document.Blocks.Clear();
                fileList.Items.Clear();
                showErrorMain("");
            }
            else
            {
                showErrorMain("А кому писать то?");
            }

            ProgressSend.Visibility = Visibility.Hidden;
            SendButton.IsEnabled = true;
        }

        // -------------------- //

        private void Add_user_to_Click(object sender, RoutedEventArgs e)
        {
            add_whomMail();
        }

        private void WhomMail_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) add_whomMail();
            ButtonAdd_user.IsEnabled = isValidEmail(whomMail.Text);
        }

        private void add_whomMail()
        {
            if (!isValidEmail(whomMail.Text))
            {
                showErrorMain("Неверный формат email");
                return;
            }


            if (IsEmptyListView(whomMailList, whomMail.Text))
            {
                whomMail.Text = "";
                showErrorMain("");
                ButtonAdd_user.IsEnabled = false;
                return;
            }

            whomMailList.Items.Add(whomMail.Text);
            whomMail.Text = "";
            showErrorMain("");
            ButtonAdd_user.IsEnabled = false;
        }

        private void whomMailListLast_choiceElement(object sender, SelectionChangedEventArgs e)
        {
            whomMail.Text = whomMailListLast.SelectedItem.ToString();
        }

        private void whomMailList_choiceElement(object sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = true;
        }

        private void fileList_choiceElement(object sender, SelectionChangedEventArgs e)
        {
            DeleteFileButton.IsEnabled = true;
        }

        private void Delete_from_Click(object sender, RoutedEventArgs e)
        {
            whomMailList.Items.Remove(whomMailList.SelectedItem);
            DeleteButton.IsEnabled = false;
        }

        private void FileDrop(object sender, DragEventArgs e)
        {
            showErrorMain("");
            int time = (Int32) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Directory.CreateDirectory(path_FilesDirectory);
            if (e.Data != null)
                foreach (var vari in (string[]) e.Data.GetData(DataFormats.FileDrop))
                {
                    add_file(vari, time);
                }
        }

        private void Add_file_Click(object sender, RoutedEventArgs e)
        {
            showErrorMain("");
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                add_file(dlg.FileName, (Int32) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            }
        }

        private void Del_file_Click(object sender, RoutedEventArgs e)
        {
            filePathList.Remove(((Label) fileList.SelectedItem).Uid);
            fileList.Items.Remove(fileList.SelectedItem);
            DeleteFileButton.IsEnabled = false;
        }

        private void send_mes(string title, string text, List<string> fileList, string to = null)
        {
            MailMessage m = new MailMessage(new MailAddress(userMail), new MailAddress(@to ?? userMail));
            m.Subject = title;
            m.Body = text;
            m.IsBodyHtml = true;
            foreach (var vari in fileList)
            {
                m.Attachments.Add(new Attachment(vari));
            }

            smtp.Credentials = new NetworkCredential(userMail, userPassword);
            smtp.EnableSsl = true;
            smtp.Send(m);
        }

        private void add_file(string path, int time)
        {
            long size = new System.IO.FileInfo(path).Length;
            if (size > 2200 * 1024)
            {
                showErrorMain("файл превышает 2м");
                return;
            }

            Directory.CreateDirectory(path_FilesDirectory + "\\" + time);

            string fileName = path.Split('\\').Last();
            string filePath = path_FilesDirectory + "\\" + time + "\\" + path.Split('\\').Last();
            File.Copy(path, filePath);
            filePathList.Add(filePath);
            fileList.Items.Add(new Label() {Content = fileName, Uid = filePath});
        }

        bool isValidEmail(string email)
        {
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(email, pattern, RegexOptions.IgnoreCase);
            return isMatch.Success;
        }

        private void showErrorLogin(string error)
        {
            errorLabelLogin.Content = error;
        }

        private void showErrorMain(string error)
        {
            errorLabelMain.Content = error;
        }

        private void create_draftVersionMail_Click(object sender, RoutedEventArgs e)
        {
            draftVersionMail.filePathList = filePathList.GetRange(0, filePathList.Count);
            draftVersionMail.whomMailList = new List<string>();
            foreach (var vari in whomMailList.Items)
            {
                draftVersionMail.whomMailList.Add((string) vari);
            }

            Directory.CreateDirectory(path_TempDirectory);
            FileStream fileStream = new FileStream(path_DraftVersion_RtfFile, FileMode.Create);
            TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            range.Save(fileStream, DataFormats.Rtf);
            fileStream.Close();
            draftVersionMail.textPath = path_DraftVersion_RtfFile;
            ButtonLoadDraft.IsEnabled = true;
        }

        private void load_draftVersionMail_Click(object sender, RoutedEventArgs e)
        {
            showErrorMain("");
            filePathList = draftVersionMail.filePathList.GetRange(0, draftVersionMail.filePathList.Count);
            fileList.Items.Clear();
            whomMailList.Items.Clear();
            foreach (var path in filePathList)
            {
                fileList.Items.Add(new Label() {Content = path.Split('\\').Last(), Uid = path});
            }

            foreach (var vari in draftVersionMail.whomMailList)
            {
                whomMailList.Items.Add(vari);
            }

            if (File.Exists(draftVersionMail.textPath))
            {
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                FileStream fileStream = new FileStream(draftVersionMail.textPath, FileMode.Open);
                range.Load(fileStream, DataFormats.Rtf);
                fileStream.Close();
            }
        }

        private void RtbEditor_OnGotFocus(object sender, RoutedEventArgs e)
        {
            cmbFontFamily.IsEnabled = true;
            cmbFontSize.IsEnabled = true;
            alignment.IsEnabled = true;
        }

        private void RtbEditor_OnLostFocus(object sender, RoutedEventArgs e)
        {
            cmbFontFamily.IsEnabled = false;
            cmbFontSize.IsEnabled = false;
            alignment.IsEnabled = false;
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            if (Directory.Exists(path_TempDirectory))
            {
                Directory.Delete(path_TempDirectory, true);
            }
        }

        private bool IsEmptyListView(ListView list, string element)
        {
            foreach (string vari in list.Items)
            {
                if (vari == element)
                {
                    return true;
                }
            }

            return false;
        }
    }
}