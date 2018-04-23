using LIFLog.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using LIFLog.Helpers;
using LIFLog.ViewModel;
using System.Configuration;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System.Threading.Tasks;
using System.ComponentModel;
using MahApps.Metro;

namespace LIFLog
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        string folder = @"";
        ProgressDialogController controller;
        private System.ComponentModel.BackgroundWorker backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
        private List<LogFile> selectedFiles;
        private LogFileBase lfb;
        private MetroWindow accentThemeTestWindow;

        public MainWindow()
        {
            InitializeComponent();

            InitializeBackgroundWorker();

            // Folder configuration load, will be tested on actual folder management
            String configurationFolder = Properties.Settings.Default.DefaultFolder;

            // folder management, loads files info from saved folder path or ask user for it.
            LoadLogFolder();
            

        }

        #region FirstScreen
        #region Files Load
        /// <summary>
        /// Load the content of the selected files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LoadFilesButton_Click(object sender, RoutedEventArgs e)
        {

            selectedFiles = FileListGridView.SelectedItems.Cast<LogFile>().ToList();
            lfb = new LogFileBase(selectedFiles.Select(a => a.FullPath).Cast<String>().ToList());
            controller = await this.ShowProgressAsync("Traitement en cours...", "Chargement des fichiers, veuillez patienter", true);
            backgroundWorker1.RunWorkerAsync();
        }

        private void InitializeBackgroundWorker()
        {
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;

            backgroundWorker1.DoWork +=
                new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged +=
                new ProgressChangedEventHandler(
            backgroundWorker1_ProgressChanged);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            List<Hit> hitList = lfb.GetLIFData(backgroundWorker1, e);
            e.Result = hitList;
        }

        private void backgroundWorker1_ProgressChanged(object sender,
           ProgressChangedEventArgs e)
        {
            controller.SetProgress(Convert.ToDouble(e.ProgressPercentage));
        }

        private async void backgroundWorker1_RunWorkerCompleted(
            object sender, RunWorkerCompletedEventArgs e)
        {
            await controller.CloseAsync();
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                await this.ShowMessageAsync("Error!", e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                await this.ShowMessageAsync("Canceled!", "Canceled");
            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                List<Hit> hitList = (List<Hit>)e.Result;
                if (hitList.FirstOrDefault() != null)
                {
                    DetailsGridView.ItemsSource = hitList;

                    DetailsTabItem.IsEnabled = true;
                    StatisticsGridView.ItemsSource = null;
                    AgregationGridView.ItemsSource = null;
                    DetailsTabItem.IsSelected = true;
                }
                else
                {
                    await this.ShowMessageAsync("No Data Found!", "No data was found, try selecting another file or change the folder.");
                }

            }
        }
        #endregion



        /// <summary>
        /// loads the prefered configuration. If nothing is found, ask the user
        /// </summary>
        private async void LoadLogFolder()
        {
            String configurationFolder = Properties.Settings.Default.DefaultFolder;
            if (!Directory.Exists(configurationFolder))
            {
                await this.ShowMessageAsync("Sélection du répertoire de logs", "Merci de sélectionner un répertoire contenant des logs de Life is Feudal (MMO)");
                PromptLogFolder();
            }
            else
            {
                CurrentFolderTextBox.Text = configurationFolder;
                folder = configurationFolder;
            }
            LoadFolderDetailGridView();

        }


        #region Log folder management
        #region Folder events
        /// <summary>
        /// handles manual folder change button event, prompting folder from user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeLogFolderButton_Click(object sender, RoutedEventArgs e)
        {
            PromptLogFolder();
            
            DetailsTabItem.IsEnabled = false;
            FolderTabItem.IsSelected = true;
        }

        /// <summary>
        /// loads the files contained in the current folder
        /// </summary>
        private void LoadFolderDetailGridView()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folder);
            IEnumerable<LogFile> info = new FileSystemEnumerable(dirInfo, "*.log", SearchOption.AllDirectories).Cast<FileInfo>().Select(a => new LogFile { Name = a.Name, FullPath = a.FullName, Size = a.Length });
            FileListGridView.ItemsSource = info.ToList<LogFile>();
        }
        #endregion

        /// <summary>
        /// gets the folder from user
        /// </summary>
        private void PromptLogFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                String configurationFolder = Properties.Settings.Default.DefaultFolder;
                if (Directory.Exists(configurationFolder))
                {
                    dialog.SelectedPath = configurationFolder;

                }

                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                switch (result)
                {
                    case System.Windows.Forms.DialogResult.OK:
                        folder = dialog.SelectedPath;
                        CurrentFolderTextBox.Text = folder;
                        break;
                    default:
                        if (Directory.Exists(configurationFolder))
                        {
                            folder = configurationFolder;
                            CurrentFolderTextBox.Text = folder;
                            return;
                        }
                        else
                        {

                            System.Windows.Application.Current.Shutdown();
                        }
                        break;
                }
                if (Directory.Exists(folder)) { 
                    LoadFolderDetailGridView();

                    Properties.Settings.Default.DefaultFolder = folder;
                    Properties.Settings.Default.Save();
                }
            }
        }

        #endregion
        #endregion

        #region Second screen

        private void DetailsGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DetailsGridView.SelectedItems.Count == 0) return;
            List<Hit> selectedHits = DetailsGridView.SelectedItems.Cast<Hit>().ToList();
            StatisticsGridView.ItemsSource = selectedHits
                .GroupBy(x => x.Direction)
                .Select(x => new
                {
                    Direction = x.Key,
                    SumHitpoint = x.Sum(z => z.HitPoint),
                    MinHitpoint = x.Min(z => z.HitPoint),
                    MaxHitpoint = x.Max(z => z.HitPoint),
                    MeanHitpoint = x.Average(z => z.HitPoint),
                    MinSpeed = x.Min(z => z.ImpactSpeed),
                    MaxSpeed = x.Max(z => z.ImpactSpeed),
                    MeanSpeed = x.Average(z => z.ImpactSpeed)
                }).ToList();
            AgregationGridView.ItemsSource = selectedHits
                .GroupBy(x => new { x.Opponent, x.Direction })
                .Select(x => new
                {
                    Opponent = x.Key.Opponent,
                    Direction = x.Key.Direction,
                    SumHitpoint = x.Sum(z => z.HitPoint),
                    MinHitpoint = x.Min(z => z.HitPoint),
                    MaxHitpoint = x.Max(z => z.HitPoint),
                    MeanHitpoint = x.Average(z => z.HitPoint),
                    MinSpeed = x.Min(z => z.ImpactSpeed),
                    MaxSpeed = x.Max(z => z.ImpactSpeed),
                    MeanSpeed = x.Average(z => z.ImpactSpeed)
                }).ToList();
        }
        #endregion

        /// <summary>
        /// miscellanous TODO / theme management
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeStyleButton_Click(object sender, RoutedEventArgs e)
        {
            if (accentThemeTestWindow != null)
            {
                accentThemeTestWindow.Activate();
                return;
            }

            accentThemeTestWindow = new AccentStyleWindow();
            accentThemeTestWindow.Owner = this;
            accentThemeTestWindow.Closed += (o, args) => accentThemeTestWindow = null;
            accentThemeTestWindow.Left = this.Left + this.ActualWidth / 2.0;
            accentThemeTestWindow.Top = this.Top + this.ActualHeight / 2.0;
            accentThemeTestWindow.Show();
        }
    }
}
