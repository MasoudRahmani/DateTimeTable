using DateTimeTable.Logic;
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

namespace DateTimeTable
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _srv = "localhost";
        private string _db = "";
        private string _table = "DimDate";

        public string Server
        {
            get => _srv;
            set => _srv = value;
        }
        public string TableName { get => _table; set => _table = value; }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

        }


        #region WPFEvent

        private void ServerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _srv = ServerTextBox.Text;
        }
        private void DatabaseSelected(object sender, SelectionChangedEventArgs e)
        {
            _db = Databases.SelectedValue.ToString();
        }
        private void TableName_Changed(object sender, EventArgs e)
        {
            _table = TableNameTextBox.Text.ToString();
        }

        private void Fill_databases(object sender, RoutedEventArgs e)
        {
            if (Integrated.IsChecked == false)
                ;//ask for user and pass

            var _helper = new SQLConnectionHelper(_srv, (bool)Integrated.IsChecked);
            Databases.ItemsSource = _helper.GetDatabases();
        }

        #endregion


        private void StartToCreate(object sender, RoutedEventArgs e)
        {
            UpdateLayoutToInformStartofJob();

            DateCreator bl = new DateCreator(_srv, _db, _table, (bool)Integrated.IsChecked);

            CalculateDates(bl);

        }

        private void UpdateLayoutToInformStartofJob()
        {
            Progressbar.IsIndeterminate = true;
            MainGrid.IsEnabled = false;
        }

        private async void CalculateDates(DateCreator dc)
        {
            var start = (DateTime)StartDatePicker.SelectedDate;
            var end = (DateTime)EndDatePicker.SelectedDate;
            var result = Task.Factory.StartNew(() => { dc.Start(start, end); });
            await result;
            UpdateLayoutToInfromJobDone();

        }
        private void UpdateLayoutToInfromJobDone()
        {
            MainGrid.IsEnabled = true;
            Progressbar.IsIndeterminate = false;
            Progressbar.Maximum = 1;
            Progressbar.Value = 1;
            MessageBox.Show("Done.");
        }
    }
}
