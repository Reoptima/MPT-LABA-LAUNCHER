using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using MPT_Text_Editor;

namespace text
{
    /// <summary>
    /// Логика взаимодействия для Edit.xaml
    /// </summary>
    public partial class Edit : Window
    {
        public Edit()
        {
            InitializeComponent();
            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source); // импортируем шрифты
            cmbFontSize.ItemsSource = new List<double>()
                {8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72}; // предлогаемые варинты выбора шрифта
            cmbMargin.ItemsSource = new List<double>() {1, 1.5, 2, 3, 4}; // предлогаемые варинты выбора отспупа
        }

        public String path = ""; // объявляем глобальную переменную пути, чтобы сохранять или открывать файл

        private void Grid_MouseDown(Object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void
            rtbEditor_SelectionChanged(object sender, RoutedEventArgs e) // жирный, курсивный и подчёркнутый текст
        {
            try
            {
                object temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
                btnBold.IsChecked =
                    (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold)); // жирный шрифт
                temp = rtbEditor.Selection.GetPropertyValue(Inline.FontStyleProperty);
                btnItalic.IsChecked =
                    (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic)); // курсив
                temp = rtbEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
                btnUnderline.IsChecked =
                    (temp != DependencyProperty.UnsetValue) &&
                    (temp.Equals(TextDecorations.Underline)); // подчёркивание
                temp = rtbEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty); // выбор шрифта
                cmbFontFamily.SelectedItem = temp;
                temp = rtbEditor.Selection.GetPropertyValue(Inline.FontSizeProperty); // размер шрифта
                cmbFontSize.Text = temp.ToString();
            }
            catch (System.Exception)
            {
            } // так надо, чтоб не вылетало из-за сам не знаю чего, но если жать всё подрят - оно вылетит :)
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e) // открыть файл
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                path = dlg.FileName;
                FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Load(fileStream, DataFormats.Rtf);
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e) //сохранить файл
        {
            TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            try
            {
                if (path != "") // если файл не был раньше сохранён нигде, то он будет "сохранён как"
                {
                    FileStream fileStream = new FileStream(path, FileMode.Create);
                    range.Save(fileStream, DataFormats.Rtf);
                    return;
                }

                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
                if (dlg.ShowDialog() == true
                ) // если файл уже был где-то сохранён, то при нажатии кнопки сохранить, он просто сохранится в том же самом месте.
                {
                    FileStream fileStream = new FileStream(dlg.FileName, FileMode.Create);
                    range.Save(fileStream, DataFormats.Rtf);
                }
            }
            catch (System.Exception)
            {
            } // шоб не вылетало с ошибкой "файл уже открыт где-то
        }

        private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e) // Выбор шрифта
        {
            if (cmbFontFamily.SelectedItem != null)
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
        }

        private void cmbFontSize_TextChanged(object sender, TextChangedEventArgs e) // Размер шрифта
        {
            try
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text);
            }
            catch (System.Exception)
            {
            } // чтоб при значении размера шрифта 0 Double не вылетел с криками "Я ТАК НЕ УМЕЮ". Шрифт при этом не будет меняться и останется каким и был.
        }

        private void cmbMargin_TextChanged(object sender, TextChangedEventArgs e) // Размер шрифта
        {
            try
            {
                rtbEditor.Margin = new Thickness(Convert.ToDouble(cmbMargin.SelectedValue));
                this.DataContext = cmbMargin.SelectedValue;
            }
            catch (System.Exception)
            {
            } // чтоб при значении размера шрифта 0 Double не вылетел с криками "Я ТАК НЕ УМЕЮ". Шрифт при этом не будет меняться и останется каким и был.
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

        private void Prav_Click(object sender, RoutedEventArgs e) // Открыть вкладку правка
        {
            Prav.Visibility = Visibility.Visible; // Включить менюшку "правки"
            Izm.Visibility = Visibility.Hidden; // Скрыть менюшку "изменений"
            Pravv.Foreground = Brushes.White; // Всё снизу меняет цвет кнопки правка, чтобы она выглядела как включенная
            Pravv.Background = new SolidColorBrush(Color.FromRgb(255, 166, 0));
            IzmButt.Foreground = Brushes.LightGray;
            IzmButt.Background = new SolidColorBrush(Color.FromRgb(191, 124, 0));
        }

        private void IzmButt_Click(object sender, RoutedEventArgs e) // Открыть вкладку изменений
        {
            Prav.Visibility = Visibility.Hidden; // Скрыть менюшку "правки"
            Izm.Visibility = Visibility.Visible; // Включть менюшку "изменений"
            IzmButt.Foreground =
                Brushes.White; // Всё снизу меняет цвет кнопки изменений, чтобы она выглядела как включенная
            IzmButt.Background = new SolidColorBrush(Color.FromRgb(255, 166, 0));
            Pravv.Foreground = Brushes.LightGray;
            Pravv.Background = new SolidColorBrush(Color.FromRgb(191, 124, 0));
        }

        private void ClrPcker_Background_SelectedColorChanged(object sender, EventArgs e) // Палитра из вкладки правка
        {
            var Text_Color =
                ClrPcker_Background.SelectedColor.ToString(); // приводим полученный цвет в текстовый формат
            Char[]
                ColorChar =
                {
                    '{', '}'
                }; // из полученного текстового формата в виде например {#FFA500} удаляем скобочки, чтобы Foregraund понял цвет в формате HEX
            string Colorval = Text_Color.TrimEnd(ColorChar); //убираем ковычки из значения цвета
            rtbEditor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty,
                value: Colorval); // меняем foreground выделенного текста
        }

        private void Leftalig_Click(object sender, RoutedEventArgs e) =>
            EditingCommands.AlignLeft.Execute(null, rtbEditor); // выравнимание по левому краю

        private void Rightalig_Click(object sender, RoutedEventArgs e) =>
            EditingCommands.AlignRight.Execute(null, rtbEditor); // по правому краю

        private void Centalig_Click(object sender, RoutedEventArgs e) =>
            EditingCommands.AlignCenter.Execute(null, rtbEditor); // по центру

        private void Justtalig_Click(object sender, RoutedEventArgs e) =>
            EditingCommands.AlignJustify.Execute(null, rtbEditor); // выровнять по ширине

        private void Print_Click(object sender, RoutedEventArgs e) // отправить на предпросмотр к печати
        {
            PrintDialog pd = new PrintDialog();
            if ((pd.ShowDialog() == true))
            {
                pd.PrintDocument((((IDocumentPaginatorSource) rtbEditor.Document).DocumentPaginator),
                    "printing as paginator"); // я не знаю что тут и как, но оно работает, почти.
            }
        }
    }
}