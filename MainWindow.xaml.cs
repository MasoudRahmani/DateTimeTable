using DateTimeTable.Logic;
using Microsoft.Data.SqlClient;
using Ookii.Dialogs.Wpf;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

using Microsoft.Win32;
namespace DateTimeTable
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts = new();
        // pending status message if StatusText control not yet initialized
        private string? _pendingStatus;
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
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // If any status was set before controls were ready, apply it now
            if (!string.IsNullOrEmpty(_pendingStatus))
            {
                SetStatus(_pendingStatus);
                _pendingStatus = null;
            }
            // Apply localized resources
            ApplyResources();

            // Initialize control defaults from bound properties
            try
            {
                if (ServersCombo != null)
                    ServersCombo.Text = SelectedServer;
                if (TableNameTextBox != null)
                    TableNameTextBox.Text = TableName;
                if (Integrated != null)
                    Integrated.IsChecked = IntegratedChecked;
                if (StartDatePicker != null && StartDatePicker.SelectedDate == null)
                    StartDatePicker.SelectedDate = new DateTime(1993, 1, 1);
                if (EndDatePicker != null && EndDatePicker.SelectedDate == null)
                    EndDatePicker.SelectedDate = new DateTime(2030, 1, 1);
            }
            catch
            {
                // ignore initialization errors
            }
        }

        #region WPFEvent


        private void Fill_databases(object sender, RoutedEventArgs e)
        {
            // Use async wrapper with timeout to avoid UI blocking
            _ = FillDatabasesAsync();
        }

        private async Task FillDatabasesAsync()
        {
            try
            {
                SetUiBusy("Fetching databases...");
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                _cts = cts;

                var helper = IntegratedChecked ? new SQLConnectionHelper(_srv) : new SQLConnectionHelper(_srv, _user, _passs);

                var task = Task.Run(() => helper.GetDatabases(), cts.Token);
                var finished = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cts.Token));
                if (finished == task)
                {
                    var result = await task;
                    Databases.ItemsSource = result;
                    SetStatus("Databases loaded.");
                }
            }
            catch (OperationCanceledException)
            {
                SetStatus("Operation timed out.");
            }
            catch (Exception ex)
            {
                SetStatus("Error: " + ex.Message);
            }
            finally
            {
                ClearUiBusy();
            }
        }

        #endregion

        private void NotEnoughData(string why)
        {
            using TaskDialog dialog = new();
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

        private bool CheckTableExists()
        {
            SQLConnectionHelper helper;
            if (IntegratedChecked) helper = new SQLConnectionHelper(_srv);
            else helper = new SQLConnectionHelper(_srv, _user, _passs);
            helper.Database = _db;

            using var cnn = helper.GetSqlConnection(ConnectionStringConfig.WithDatabase, true, true);
            cnn.Open();
            var sch = cnn.GetSchema("Tables");

            bool result = false;
            foreach (DataRow r in sch.Rows)
            {
                string tablename = (string)r[2];
                if (tablename == _table)
                {
                    result = true; break;
                }
            }
            cnn.Close();
            return result;
        }

        private void StartToCreate(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_db))
                NotEnoughData("مقدار دیتابیس خالی می باشد.");
            else
            {
                if (CheckTableExists())
                {
                    NotEnoughData("دیتابیس از قبل موجود می باشد، لطفا نام دیگری انتخاب فرمایید.");
                    return;
                }

                _ = StartCreateAsync();
            }
        }

        private async Task StartCreateAsync()
        {
            UpdateLayoutToInformStartofJob("Creating table...");
            try
            {
                DateCreator bl = new(_srv, _db, _table);
                await CalculateDatesAsync(bl, TimeSpan.FromMinutes(5));
                SetStatus("Creation finished.");
            }
            catch (TimeoutException)
            {
                SetStatus("Operation timed out.");
            }
            catch (Exception ex)
            {
                SetStatus("Error: " + ex.Message);
            }
            finally
            {
                UpdateLayoutToInfromJobDone();
            }
        }

        private void UpdateLayoutToInformStartofJob(string status = "Working...")
        {
            Dispatcher.Invoke(() =>
            {
                Progressbar.IsIndeterminate = true;
                MainGrid.IsEnabled = false;
                SetStatus(status);
            });
        }

        private async Task CalculateDatesAsync(DateCreator dc, TimeSpan timeout)
        {
            var start = StartDatePicker.SelectedDate ?? new DateTime(1993, 01, 01);
            var end = EndDatePicker.SelectedDate ?? new DateTime(2030, 01, 01);

            var cts = new CancellationTokenSource(timeout);
            try
            {
                var task = Task.Run(() => dc.Start(start, end), cts.Token);
                var completed = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
                if (completed != task)
                {
                    throw new TimeoutException();
                }
                await task; // propagate exceptions
            }
            finally
            {
                cts.Dispose();
            }
        }
        private void UpdateLayoutToInfromJobDone()
        {
            MainGrid.IsEnabled = true;
            Progressbar.IsIndeterminate = false;
            Progressbar.Maximum = 1;
            Progressbar.Value = 1;
            MessageBox.Show("Done.");
            SetStatus("Ready");
        }

        private void SetUiBusy(string message)
        {
            Dispatcher.Invoke(() =>
            {
                MainGrid.IsEnabled = false;
                Progressbar.IsIndeterminate = true;
                StatusText.Text = message;
            });
        }

        private void ClearUiBusy()
        {
            Dispatcher.Invoke(() =>
            {
                MainGrid.IsEnabled = true;
                Progressbar.IsIndeterminate = false;
                if (StatusText != null)
                    StatusText.Text = "Ready";
                else
                    _pendingStatus = "Ready";
            });
        }

        private void SetStatus(string message)
        {
            // Ensure call runs on UI thread
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SetStatus(message));
                return;
            }

            if (StatusText != null)
            {
                StatusText.Text = message;
            }
            else
            {
                // Control not yet created; remember message for Loaded event
                _pendingStatus = message;
            }
        }

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (LanguageCombo.SelectedItem is ComboBoxItem item)
                {
                    var content = (item.Content ?? string.Empty).ToString();
                    if (content == "فارسی")
                    {
                        CultureInfo.CurrentUICulture = new CultureInfo("fa");
                        CultureInfo.CurrentCulture = new CultureInfo("fa");
                        this.FlowDirection = FlowDirection.RightToLeft;
                    }
                    else
                    {
                        CultureInfo.CurrentUICulture = new CultureInfo("en");
                        CultureInfo.CurrentCulture = new CultureInfo("en");
                        this.FlowDirection = FlowDirection.LeftToRight;
                    }
                    ApplyResources();
                    SetStatus($"Language: {content}");
                }
            }
            catch
            {
                // ignore
            }
        }

        private void ApplyResources()
        {
            try
            {
                var rm = Properties.Resources.ResourceManager;
                var culture = CultureInfo.CurrentUICulture;

                string? header = rm.GetString("HeaderTitle", culture);
                string? subtitle = rm.GetString("Subtitle", culture);
                string? conn = rm.GetString("ConnectionHeader", culture);
                string? browse = rm.GetString("Browse", culture);
                string? testConn = rm.GetString("TestConnection", culture);
                string? create = rm.GetString("CreateTable", culture);
                string? integrated = rm.GetString("IntegratedSecurity", culture);
                string? start = rm.GetString("Start", culture);
                string? end = rm.GetString("End", culture);
                string? desc = rm.GetString("DescriptionText", culture);
                string? tip = rm.GetString("TipText", culture);
                string? servl = rm.GetString("ServerLabel", culture);
                string? dbal = rm.GetString("DatabaseLabel", culture);
                string? secl = rm.GetString("SecurityLabel", culture);
                string? tnl = rm.GetString("TableNameLabel", culture);

                var headerTitle = this.FindName("HeaderTitleText") as TextBlock;
                var headerSub = this.FindName("HeaderSubtitleText") as TextBlock;
                var connHeader = this.FindName("ConnectionHeader") as TextBlock;
                var browseBtn = this.FindName("BrowseServerButton") as Button;
                var testBtn = this.FindName("Testbtn") as Button;
                var createBtn = this.FindName("CreateTableButton") as Button;
                var integratedChk = this.FindName("Integrated") as CheckBox;
                var startLabel = this.FindName("StartLabel") as TextBlock;
                var endLabel = this.FindName("EndLabel") as TextBlock;
                var descBlock = this.FindName("DescriptionBody") as TextBlock;
                var tipBlock = this.FindName("TipTextBlock") as TextBlock;
                var langLabel = this.FindName("LanguageLabel") as TextBlock;

                if(TableNameLabel != null && tnl != null) TableNameLabel.Text = tnl;
                if(ServerLabel != null && servl != null) ServerLabel.Text = servl;
                if(DatabaseLabel != null && dbal != null) DatabaseLabel.Text = dbal;
                if(SecurityLabel != null && secl != null) SecurityLabel.Text = secl;

                if (headerTitle != null && header != null) headerTitle.Text = header;
                if (headerSub != null && subtitle != null) headerSub.Text = subtitle;
                if (connHeader != null && conn != null) connHeader.Text = conn;
                if (browseBtn != null && browse != null) browseBtn.Content = browse;
                if (testBtn != null && testConn != null) testBtn.Content = testConn;
                if (createBtn != null && create != null) createBtn.Content = create;
                if (integratedChk != null && integrated != null) integratedChk.Content = integrated;
                if (startLabel != null && start != null) startLabel.Text = start;
                if (endLabel != null && end != null) endLabel.Text = end;
                if (descBlock != null && desc != null) descBlock.Text = desc;
                if (tipBlock != null && tip != null) tipBlock.Text = tip;
                if (langLabel != null) langLabel.Text = rm.GetString("LanguageLabel", culture) ?? "Language:";

                if (!string.IsNullOrEmpty(header)) this.Title = header;
            }
            catch
            {
                // ignore resource application errors
            }
        }

        private async void BrowseServers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetUiBusy("Searching for SQL Server instances...");
                await Task.Delay(50); // allow UI update

                var list = new System.Collections.Generic.List<string>();

                // Try registry-based discovery for local SQL Server instances (works on Windows)
                try
                {
                    // Prefer 64-bit view then fallback
                    string keyPath = @"SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL";
                    using var key = Registry.LocalMachine.OpenSubKey(keyPath) ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\WOW6432Node\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL");
                    if (key != null)
                    {
                        foreach (var name in key.GetValueNames())
                        {
                            var instance = name;
                            var full = string.Equals(instance, "MSSQLSERVER", StringComparison.OrdinalIgnoreCase) ? Environment.MachineName : Environment.MachineName + "\\" + instance;
                            list.Add(full);
                        }
                    }
                }
                catch
                {
                    // ignore and continue
                }

                if (list.Count > 0)
                {
                    ServersCombo.ItemsSource = list;
                    ServersCombo.IsDropDownOpen = true;
                    ServersCombo.SelectedIndex = 0;
                    SelectedServer = list[0];
                    SetStatus($"Found {list.Count} instances.");
                }
                else
                {
                    SetStatus("No local instances found.");
                }
            }
            catch (Exception ex)
            {
                SetStatus("Error: " + ex.Message);
            }
            finally
            {
                ClearUiBusy();
            }
        }

        /// <summary>
        /// Unchecked Integrated Security
        /// </summary>
        /// <param name="sender">CheckBox</param>
        private void GetCredentialEvent(object sender, RoutedEventArgs e)
        {
            using (CredentialDialog credentialDialog = new())
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

                    bool result = new SQLConnectionHelper(_srv, _user, _passs).CheckCredential();

                    // Normally, you should verify if the credentials are correct before calling ConfirmCredentials.
                    // ConfirmCredentials will save the credentials if and only if the user checked the save checkbox.
                    //if (result)
                    //    credentialDialog.ConfirmCredentials(true);
                    //else credentialDialog.ConfirmCredentials(false);
                    if (!result)
                        NotEnoughData("رمز اشتباه است");

                    //بررسی cancel
                }
            }
        }
    }
}
