using LOADER2._1.Properties;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime;
using System.Windows;

namespace LOADER2._1
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Upload_project : Window
    {
        private Settings settings; // Объявление settings

        public Upload_project()
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



        private void OnSourceFolderButtonClick(object sender, RoutedEventArgs e)
        {
            string selectedFolder = SelectFolder();
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                txtBoxDestinationFolder.Text = selectedFolder;
            }
        }

        private string SelectFolder()
        {
            var dialog = new OpenFileDialog();
            dialog.ValidateNames = false;
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            dialog.FileName = "Folder Selection.";
            dialog.Filter = "Folders|no.files";
            if (dialog.ShowDialog() == true)
            {
                return System.IO.Path.GetDirectoryName(dialog.FileName);
            }
            return null;
        }
        private void btn_open_plx_Click(object sender, RoutedEventArgs e)
        {
            string selectedFile = SelectP2dxFile();
            if (!string.IsNullOrEmpty(selectedFile))
            {
                txtbox_p2dx.Text = selectedFile;
                txtBoxDestinationFolder.Text = selectedFile.Replace(".p2dx", ".p2dxdat");
            }
        }

        private string SelectP2dxFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "P2DX Files (*.p2dx)|*.p2dx|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
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



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Поиск файла setting.json
            string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

            if (!File.Exists(settingsFilePath))
            {
                MessageBox.Show("Файл settings.json не найден.");
                return;
            }

            // Чтение данных из setting.json
            string json = File.ReadAllText(settingsFilePath);
            settings = JsonConvert.DeserializeObject<Settings>(json);

            if (settings == null)
            {
                MessageBox.Show("Не удалось прочитать настройки из файла settings.json.");
                return;
            }

            // Установка пути из настроек
            string sourceFolder = txtBoxDestinationFolder.Text;
            string destinationFolder = settings.SourceFolder; // Используем SourceFolder из настроек

            // Копирование файла .p2dx
            string sourceFile = txtbox_p2dx.Text;
            string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));

            if (!File.Exists(sourceFile))
            {
                MessageBox.Show($"Файл {sourceFile} не найден.");
                return;
            }

            try
            {
                File.Copy(sourceFile, destinationFile, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при копировании файла: {ex.Message}");
                return;
            }

            // Копирование связанной папки .p2dxdat
            string sourceFolderName = sourceFile.Replace(".p2dx", ".p2dxdat");
            string destinationFolderName = Path.Combine(destinationFolder, Path.GetFileName(sourceFolderName));

            if (!Directory.Exists(sourceFolderName))
            {
                MessageBox.Show($"Папка {sourceFolderName} не найдена.");
                return;
            }

            try
            {
                DirectoryCopy(sourceFolderName, destinationFolderName, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при копировании папки: {ex.Message}");
                return;
            }

            MessageBox.Show("Файлы успешно скопированы!");
        }





    }
}

