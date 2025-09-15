using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Core;
using Core.ConfigurationImport;
using Core.ConfigurationImport.Z21New;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Persistence.Database;

namespace Shell.WPF.DatabaseImport
{
  /// <summary>
  /// Interaction logic for DatabaseImportView.xaml
  /// </summary>
  public partial class DatabaseImportView : Window, INotifyPropertyChanged
  {
    private readonly IEnumerable<IDatabaseImporter> databaseImporters;

    public DatabaseImportView(IEnumerable<IDatabaseImporter> databaseImporters)
    {
      this.databaseImporters = databaseImporters;
      DataContext = this;
      InitializeComponent();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Path { get; set; } = "";

    protected void OnPropertyChanged([CallerMemberName] string name = null!)
    {
      PropertyChanged?.Invoke(this, new(name));
    }

    private async void BtnGo_Click(object sender, RoutedEventArgs e)
    {
      IDatabaseImporter z21 = databaseImporters.Single(); // TODO refactor this to support multiple importers.
      await z21.ImportAsync(new(Path));
      MessageBox.Show($"Import Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
      Close();
    }

    private void BtnOpenFileDalog_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog ofp = new();
      ofp.DefaultExt = ".z21";
      ofp.Filter = "Z21 DB FIle (*.z21)|*.z21";
      ofp.ShowDialog();
      BtnImportNow.IsEnabled = !string.IsNullOrWhiteSpace(ofp.FileName);
      TbFileSelector.Text = ofp.FileName;
      Path = ofp.FileName;
    }
  }
}