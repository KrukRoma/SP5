using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PD_311_AsyncAwait
{
    public partial class MainWindow : Window
    {
        private string sourceFilePath;
        private string targetFolderPath;
        private CancellationTokenSource cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                sourceFilePath = dialog.FileName;
                list.Items.Add("Source File: " + sourceFilePath);
            }
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                targetFolderPath = dialog.FileName;
                list.Items.Add("Target Folder: " + targetFolderPath);
            }
        }

        private async void CopyFiles_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(targetFolderPath) || !int.TryParse(txtCopies.Text, out int copies))
            {
                MessageBox.Show("Please select file, folder and specify number of copies.");
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            progressBar.IsIndeterminate = false;
            progressBar.Maximum = copies;
            progressBar.Value = 0;

            try
            {
                await CopyFilesAsync(copies, token);
                MessageBox.Show("Files copied successfully.");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Copying was canceled.");
            }
        }

        private async Task CopyFilesAsync(int copies, CancellationToken token)
        {
            for (int i = 0; i < copies; i++)
            {
                token.ThrowIfCancellationRequested();

                string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
                string extension = Path.GetExtension(sourceFilePath);
                string newFileName = $"{fileName}_{i + 1}{extension}";
                string targetFilePath = Path.Combine(targetFolderPath, newFileName);

                await Task.Run(() => File.Copy(sourceFilePath, targetFilePath, true), token);

                progressBar.Dispatcher.Invoke(() => progressBar.Value++);
                list.Dispatcher.Invoke(() => list.Items.Add($"Copied: {newFileName}"));
            }
        }

        private void StopCopy_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }
    }
}
