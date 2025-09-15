using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Core;
using Core.ConfigurationImport.Z21New;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Persistence.Database;

namespace Shell.WPF.Import
{
  /// <summary>
  /// Interaction logic for DatabaseImportView.xaml
  /// </summary>
  public partial class DatabaseImportView : Window, INotifyPropertyChanged
  {
    private readonly Database db;

    public DatabaseImportView(IServiceProvider provider)
    {
      DataContext = this;
      InitializeComponent();
      db = provider.GetService<Database>()!;
      LogService = provider.GetService<LogEventBus>()!;
    }

    public LogEventBus LogService { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Path { get; set; } = "";

    protected void OnPropertyChanged([CallerMemberName] string name = null!)
    {
      PropertyChanged?.Invoke(this, new(name));
    }

    private async void BtnGo_Click(object sender, RoutedEventArgs e)
    {
      Z21NewDatabaseImporter z21 = new(db);
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