using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace LOADER2._1
{
    public partial class setting : Window
    {
        public setting()
        {
            InitializeComponent();
        }

        private void OnSourceFolderButtonClick(object sender, RoutedEventArgs e)
        {
            string selectedFolder = SelectFolder();
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                txtBoxSourceFolder.Text = selectedFolder;
            }
        }

        private void OnDestinationFolderButtonClick(object sender, RoutedEventArgs e)
        {
            string selectedFolder = SelectFolder();
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                txtBoxDestinationFolder.Text = selectedFolder;
            }
        }

        private string SelectFolder()
        {
            var dialog = new OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection.",
                Filter = "Folders|no.files"
            };

            if (dialog.ShowDialog() == true)
            {
                return System.IO.Path.GetDirectoryName(dialog.FileName);
            }

            return null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Создаем объект для сериализации
            var settingsData = new
            {
                SourceFolder = txtBoxSourceFolder.Text,
                DestinationFolder = txtBoxDestinationFolder.Text,
                CreateNewFolder = CreateNewFolder.IsChecked,
                ReplaceFolder = ReplaceFolder.IsChecked
            };

            // Преобразуем объект в JSON
            string jsonData = JsonConvert.SerializeObject(settingsData);

            // Сохраняем JSON в файл
            try
            {
                File.WriteAllText("settings.json", jsonData);
                MessageBox.Show("Настройки сохранены в файл settings.json");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении настроек: {ex.Message}");
            }
        }

        private void CreateNewFolder_Checked(object sender, RoutedEventArgs e)
        {
            if (CreateNewFolder.IsChecked == true)
            {
                ReplaceFolder.IsChecked = false;
            }
        }

        private void ReplaceFolder_Checked(object sender, RoutedEventArgs e)
        {
            if (ReplaceFolder.IsChecked == true)
            {
                CreateNewFolder.IsChecked = false;
            }
        }
    }
}
