using DateTimeTable.Logic;
using Ookii.Dialogs.Wpf;
using System;
using System.Threading.Tasks;
using System.Windows;
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
        private string _passs = "";
        private string _user = "";

        public bool IntegratedEnable { get; set; }
        public bool IntegratedChecked { get; set; }

        public string SelectedDatebase { get => _db; set => _db = value; }

        public string SelectedServer { get => _srv; set => _srv = value; }
        public string TableName { get => _table; set => _table = value; }

        public MainWindow()
        {
            IntegratedEnable = true;
            IntegratedChecked = true;
            InitializeComponent();
            this.DataContext = this;

        }


        #region WPFEvent

        private void Fill_databases(object sender, RoutedEventArgs e)
        {
            if (Integrated.IsChecked == false)
                ;//ask for user and pass

            var _helper = new SQLConnectionHelper(_srv, IntegratedChecked);
            Databases.ItemsSource = _helper.GetDatabases();
        }

        #endregion

        private void NotEnoughData(string why)
        {
            using (TaskDialog dialog = new TaskDialog())
            {
                dialog.WindowTitle = ".اطلاعات کافی نمی باشد";
                dialog.MainInstruction = "لطفا اطلاعات را صحیح و کامل وارد فرمایید";
                dialog.Content = ".این خطا به علت اشتباه بودن اطلاعات وارده یا کمبود اطلاعات می باشد\n .می تواند با کلیک بر اطلاعات بیشتر از علت خطا مطلع شوید";
                dialog.ExpandedInformation = why;
                dialog.Footer = "Created By Masoud Rahmani.";
                dialog.FooterIcon = TaskDialogIcon.Shield;
                TaskDialogButton okButton = new TaskDialogButton(ButtonType.Ok);
                TaskDialogButton cancelButton = new TaskDialogButton(ButtonType.Cancel);
                dialog.Buttons.Add(okButton);
                dialog.Buttons.Add(cancelButton);
                TaskDialogButton button = dialog.ShowDialog(this);
            }
        }
        private void StartToCreate(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_db))
            {
                NotEnoughData("مقدار دیتابیس خالی می باشد.");
            }
            else
            {
                UpdateLayoutToInformStartofJob();

                DateCreator bl = new DateCreator(_srv, _db, _table, IntegratedChecked);

                CalculateDates(bl);
            }
        }

        private void UpdateLayoutToInformStartofJob()
        {
            Progressbar.IsIndeterminate = true;
            MainGrid.IsEnabled = false;
        }

        private async void CalculateDates(DateCreator dc)
        {
            var start = StartDatePicker.SelectedDate ?? new DateTime(1993, 01, 01);
            var end = EndDatePicker.SelectedDate ?? new DateTime(2030, 01, 01);
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

        /// <summary>
        /// Unchecked Integrated Security
        /// </summary>
        /// <param name="sender">CheckBox</param>
        private void GetCredentialEvent(object sender, RoutedEventArgs e)
        {
            using (CredentialDialog credentialDialog = new CredentialDialog())
            {
                credentialDialog.MainInstruction = "رمز و پسورد خود را وارد کنید.";
                credentialDialog.Content = "این رمز و پسورد جهت ورود به دیتابیس استفاده میشود و بلافاصله از حافظه پاک میشود.";
                //credentialDialog.ShowSaveCheckBox = true;
                //credentialDialog.ShowUIForSavedCredentials = true;
                //// The target is the key under which the credentials will be stored.
                //// It is recommended to set the target to something following the "Company_Application_Server" pattern.
                //// Targets are per user, not per application, so using such a pattern will ensure uniqueness.
                credentialDialog.Target = "MasoudRahmani_DateTimeCalendarCreator_" + Environment.MachineName + "_" + Environment.UserName;
                if (credentialDialog.ShowDialog())
                {
                    _user = credentialDialog.UserName;
                    _passs = credentialDialog.Password;

                    MessageBox.Show(this, string.Format("You entered the following information:\nUser name: {0}\nPassword: {1}", credentialDialog.Credentials.UserName, credentialDialog.Credentials.Password), "Credential dialog sample");
                    // Normally, you should verify if the credentials are correct before calling ConfirmCredentials.
                    // ConfirmCredentials will save the credentials if and only if the user checked the save checkbox.
                    credentialDialog.ConfirmCredentials(true);
                }
            }
        }
    }
}
