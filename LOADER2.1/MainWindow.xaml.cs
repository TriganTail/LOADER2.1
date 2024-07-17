using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace LOADER2._1
{
    public partial class MainWindow : Window
    {
        private Settings settings;
        private string folderPath;
        private string destinationFolder;
        private bool createNewFolder;
        private bool replaceFolder;
        private Button draggableButton;
        private bool isDragging = false;
        private Point clickPosition;
        private Canvas mainCanvas;


        public MainWindow()
        {
            InitializeComponent();
            
        }
        private class Settings
        {
            public string SourceFolder { get; set; }
            public string DestinationFolder { get; set; }
            public bool CreateNewFolder { get; set; }
            public bool ReplaceFolder { get; set; }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            mainCanvas = new Canvas();
            mainCanvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            mainCanvas.VerticalAlignment = VerticalAlignment.Stretch;
            Panel.SetZIndex(mainCanvas, -1); 
            (this.Content as Grid).Children.Add(mainCanvas);

            string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

            if (File.Exists(settingsFilePath))
            {
                string json = File.ReadAllText(settingsFilePath);
                settings = JsonConvert.DeserializeObject<Settings>(json);

                if (settings != null)
                {
                    folderPath = settings.SourceFolder;
                    destinationFolder = settings.DestinationFolder;
                    createNewFolder = settings.CreateNewFolder;
                    replaceFolder = settings.ReplaceFolder;

                    List<string> filesList = FindP2DXFiles(folderPath);
                    dataListView.ItemsSource = filesList;
                }
                else
                {
                    MessageBox.Show("Failed to load settings from settings.json.");
                }
            }
            else
            {
                setting setting = new setting();
                setting.Show();
                setting.Closed += (s, args) =>
                {
                    this.Window_Loaded(sender, e);
                };
                return;
            }

            AddDraggableButton(); 
        }


        private void AddDraggableButton()
        {
            
            draggableButton = new Button
            {
                Width = 65,
                Height = 65,
                Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/update.png")))
            };

            
            Canvas.SetLeft(draggableButton, 703 );
            Canvas.SetTop(draggableButton, 42);

            
            draggableButton.MouseLeftButtonDown += DraggableButton_MouseLeftButtonDown;
            draggableButton.MouseLeftButtonUp += DraggableButton_MouseLeftButtonUp;
            draggableButton.MouseMove += DraggableButton_MouseMove;
            draggableButton.Click += DraggableButton_Click;

            
            mainCanvas.Children.Add(draggableButton);
        }


        private void DraggableButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            clickPosition = e.GetPosition(mainCanvas);
            draggableButton.CaptureMouse();
        }

        private void DraggableButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            draggableButton.ReleaseMouseCapture();
        }

        private void DraggableButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var mousePos = e.GetPosition(mainCanvas);
                var offset = mousePos - clickPosition;

                var newLeft = Canvas.GetLeft(draggableButton) + offset.X;
                var newTop = Canvas.GetTop(draggableButton) + offset.Y;

                
                if (newLeft < 0) newLeft = 0;
                if (newTop < 0) newTop = 0;
                if (newLeft + draggableButton.Width > mainCanvas.ActualWidth) newLeft = mainCanvas.ActualWidth - draggableButton.Width;
                if (newTop + draggableButton.Height > mainCanvas.ActualHeight) newTop = mainCanvas.ActualHeight - draggableButton.Height;

                Canvas.SetLeft(draggableButton, newLeft);
                Canvas.SetTop(draggableButton, newTop);

                clickPosition = mousePos;
            }
        }

        private void DraggableButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateFileList();
        }

        private void UpdateFileList()
        {
            if (settings != null)
            {
                List<string> filesList = FindP2DXFiles(folderPath);
                dataListView.ItemsSource = filesList;
            }
            else
            {
                MessageBox.Show("Settings not loaded.");
            }
        }


        private List<string> FindP2DXFiles(string folderPath)
        {
            var filesList = new List<string>(); 

            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath, "*.p2dx");

                foreach (string file in files)
                {
                    filesList.Add(Path.GetFileName(file)); 
                }
            }
            else
            {
                MessageBox.Show("Путь к папке не существует.");
            }

            return filesList; 
        }



        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = searchTextBox.Text.ToLower();

            ICollectionView view = CollectionViewSource.GetDefaultView(dataListView.ItemsSource);

            if (string.IsNullOrEmpty(searchText))
            {

            }
            else
            {
               
                view.Filter = item => ((string)item).ToLower().Contains(searchText);
            }
        }
        private void AlphabeticalCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (alphabeticalCheckBox.IsChecked == true)
            {
                dateCheckBox.IsChecked = false;
            }
            ApplySorting();
        }

        private void DateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (dateCheckBox.IsChecked == true)
            {
                alphabeticalCheckBox.IsChecked = false;
            }
            ApplySorting();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplySorting();
        }

        private void ApplySorting()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(dataListView.ItemsSource);

            if (alphabeticalCheckBox.IsChecked == true)
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
            }
            else if (dateCheckBox.IsChecked == true)
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            setting setting = new setting();
            setting.Show(); 
            setting.Closed += (s, args) =>
            {
                this.Window_Loaded(sender, e);
            };
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите выгрузить из вашего компьютера проект на сервер?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Window Upload_project = new Upload_project();
                Upload_project.Show();
                Upload_project.Closed += (s, args) =>
                {
                    this.Window_Loaded(sender, e); 
                };
            }
            else if (result == MessageBoxResult.No)
            {
                MessageBox.Show("Была нажата кнопка 'Нет'", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите скачать проект с сервера на ваш компьютер?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                
                CopyFiles(createNewFolder);
            }
            else if (result == MessageBoxResult.No)
            {
                return;
            }
        }

        private void CopyFiles(bool createNewFolder)
        {
            try
            {
                string newFolderName = "";
                int totalFiles = dataListView.SelectedItems.Count;
                int copiedFiles = 0;

                foreach (var selectedItem in dataListView.SelectedItems)
                {
                    string selectedItemName = selectedItem.ToString();
                    string sourcePath = Path.Combine(settings.SourceFolder, selectedItemName);
                    string destinationPath = Path.Combine(settings.DestinationFolder, selectedItemName);
                    

                    if (File.Exists(sourcePath))
                    {
                        
                        if (createNewFolder)
                        {
                            newFolderName = Path.GetFileNameWithoutExtension(selectedItemName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                            destinationPath = Path.Combine(settings.DestinationFolder, newFolderName);
                            Directory.CreateDirectory(destinationPath);
                        }
                        destinationPath = Path.Combine(destinationPath, Path.GetFileName(sourcePath));
                        if (File.Exists(destinationPath))
                        {
                            File.Delete(destinationPath);
                        }
                        File.Copy(sourcePath, destinationPath);
                        string relatedFolderName = selectedItemName.Replace(".p2dx", ".p2dxdat");
                        string relatedFolderPath = Path.Combine(settings.SourceFolder, relatedFolderName);
                        string destinationFolderPath = Path.Combine(settings.DestinationFolder, newFolderName, relatedFolderName);

                        if (Directory.Exists(relatedFolderPath))
                        {
                            DirectoryCopy(relatedFolderPath, destinationFolderPath, true);
                        }
                    }
                    else if (Directory.Exists(sourcePath))
                    {
                        DirectoryCopy(sourcePath, destinationPath, true);
                    }

                    copiedFiles++;

                    double progressPercentage = ((double)copiedFiles / totalFiles) * 100;
                    Loading.Value = (int)progressPercentage;
                }

                MessageBox.Show("Копирование выполнено!");
                Loading.Value = 0;

                
                MessageBoxResult result = MessageBox.Show("Хотите открыть папку назначения?", "Открыть папку", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start("explorer.exe", settings.DestinationFolder + "\\" + newFolderName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Есть проблема: {ex.Message}");
                Loading.Value = 0;
            }
        }


        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            try
            {
                if (File.Exists(sourceDirName))
                {
                    string destFileName = Path.Combine(destDirName, Path.GetFileName(sourceDirName));
                    File.Copy(sourceDirName, destFileName);
                    return;
                }
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);
                FileInfo[] files = dir.GetFiles();
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, false);
                }
                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Есть проблема: {ex.Message}");
            }
        }

        private void searchTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string searchText = searchTextBox.Text.ToLower();

            ICollectionView view = CollectionViewSource.GetDefaultView(dataListView.ItemsSource);
            if (string.IsNullOrEmpty(searchText))
            {
                view.Filter = null;
            }
        }
    }
}
