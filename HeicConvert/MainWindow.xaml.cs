using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace HeicConvert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private int _progress;
        private string[] _selectedFiles;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public int Progress
        {
            get {
                return _progress; 
            }

            set
            {
                _progress = value;
                notifyPropertyChanged();
            }
        }

        public string[] SelectedFiles
        {
            get
            {
                return _selectedFiles;
            }

            set
            {
                _selectedFiles = value;
                notifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(_selectedFiles != null &&_selectedFiles.Length > 0)
            {
                lblStatus.Content = $"Converting {SelectedFiles.Length} files...";
                lblStatusSecondary.Content = "Please wait just a moment";

                ConvertHelper.ConvertAll(_selectedFiles).Do(progress =>
                {
                    Progress = progress;
                })
                .Finally(() => Application.Current.Dispatcher.Invoke(() =>
                {
                    lblStatus.Content = $"Done!";
                    lblStatusSecondary.Visibility = Visibility.Collapsed;
                }))
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe();
            }

        }

        
        private void notifyPropertyChanged([CallerMemberName] string propertyName = null) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void btnDirectoryBrowser_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "IPhone Bilddaten (*.heic)|*.heic"
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult != null && dialogResult.Value)
            {
                SelectedFiles = dialog.FileNames;
                lblStatus.Content = $"{SelectedFiles.Length} files are ready to get converted!";
            }
               
        }
    }
}
