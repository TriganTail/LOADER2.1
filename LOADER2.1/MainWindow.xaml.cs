using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace LOADER2._1
{
    public partial class MainWindow : Window
    {
        private Settings settings;
        private string folderPath;
        private string destinationFolder;
        private bool createNewFolder;
        private bool replaceFolder;
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
            // Поиск файла setting.json
            string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

            if (File.Exists(settingsFilePath))
            {
                // Чтение данных из setting.json
                string json = File.ReadAllText(settingsFilePath);
                settings = JsonConvert.DeserializeObject<Settings>(json);

                if (settings != null)
                {
                    // Установка пути из настроек
                    folderPath = settings.SourceFolder;
                    destinationFolder = settings.DestinationFolder;
                    createNewFolder = settings.CreateNewFolder;
                    replaceFolder = settings.ReplaceFolder;

                    // Получаем список файлов
                    List<string> filesList = FindP2DXFiles(folderPath);

                    // Привязка списка файлов к источнику данных ListView
                    dataListView.ItemsSource = filesList;
                }
                else
                {
                    MessageBox.Show("Failed to load settings from settings.json.");
                }
            }
            else
            {
                // Открытие окна настроек, если файл setting.json не найден
                setting setting = new setting();
                setting.Show();
                setting.Closed += (s, args) =>
                {
                    this.Window_Loaded(sender, e); // Вызываем метод Window_Loaded после закрытия окна setting
                };
                return;
            }
        }

        private List<string> FindP2DXFiles(string folderPath)
        {
            var filesList = new List<string>(); // Переименовываем переменную

            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath, "*.p2dx");

                foreach (string file in files)
                {
                    filesList.Add(Path.GetFileName(file)); // Используем новое имя переменной
                }
            }
            else
            {
                MessageBox.Show("Путь к папке не существует.");
            }

            return filesList; // Используем новое имя переменной
        }



        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = searchTextBox.Text.ToLower();

            ICollectionView view = CollectionViewSource.GetDefaultView(dataListView.ItemsSource);

            if (string.IsNullOrEmpty(searchText))
            {
                // Если строка поиска пустая, сбрасываем фильтрацию и отображаем все элементы
                view.Filter = null;
            }
            else
            {
                // Применяем фильтр к элементам в ListView
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
            setting.Show(); // Отобразить окно setting
            setting.Closed += (s, args) =>
            {
                this.Window_Loaded(sender, e); // Вызываем метод Window_Loaded после закрытия окна setting
            };
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите выгрузить из вашего компьютера проект на сервер?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Window Upload_project = new Upload_project();
                Upload_project.Show();
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
                foreach (var selectedItem in dataListView.SelectedItems)
                {
                    string selectedItemName = selectedItem.ToString(); // Получаем текст элемента ListView
                    string sourcePath = Path.Combine(settings.SourceFolder, selectedItemName);
                    string destinationPath = Path.Combine(settings.DestinationFolder, selectedItemName);

                    if (File.Exists(sourcePath))
                    {
                        string newFolderName = "";
                        if (createNewFolder)
                        {
                            newFolderName = Path.GetFileNameWithoutExtension(selectedItemName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                            destinationPath = Path.Combine(settings.DestinationFolder, newFolderName);
                            Directory.CreateDirectory(destinationPath);
                        }

                        destinationPath = Path.Combine(destinationPath, Path.GetFileName(sourcePath)); // Добавляем имя файла к пути назначения

                        if (File.Exists(destinationPath))
                        {
                            File.Delete(destinationPath);
                        }

                        File.Copy(sourcePath, destinationPath);

                        // Копируем связанную папку
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
                        DirectoryCopy(sourcePath, destinationPath, true); // Копируем всю папку, включая все подпапки и файлы
                    }
                }

                MessageBox.Show("Files copied successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
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
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }





    }
}
