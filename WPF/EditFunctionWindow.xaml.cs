using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Persistence.Database;
using Persistence.Entities;
using Persistence.Enums;

namespace Shell.WPF
{
  /// <summary>
  /// Interaction logic for EditFunctionWindow.xaml
  /// </summary>
  public partial class EditFunctionWindow : Window, INotifyPropertyChanged
  {
    private readonly Database db;

    public event PropertyChangedEventHandler? PropertyChanged;

    private VehicleFunctionEntity vehicleFunctionEntity = new();

    public VehicleFunctionEntity FunctionModel
    {
      get => vehicleFunctionEntity;
      set
      {
        vehicleFunctionEntity = value;
        OnPropertyChanged();
      }
    }

    public EditFunctionWindow(Database _db, VehicleFunctionEntity vehicleFunction)
    {
      DataContext = this;
      InitializeComponent();
      db = _db ?? throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");

      if (vehicleFunction is null)
      {
        throw new ApplicationException($"Paramter '{nameof(vehicleFunction)}' darf nicht null sein!");
      }

      FunctionModel = db.Functions.Include(m => m.Vehicle).ThenInclude(m => m.Functions).FirstOrDefault(e => e.Id == vehicleFunction.Id)
                      ?? throw new ApplicationException($"Funktion  mit der ID '{vehicleFunction.Id} konnte nicht geöffnet werden!");

      Title = FunctionModel.Name ?? "";

      switch (FunctionModel.ButtonType)
      {
        case ButtonType.PushButton:
          RbPushButton.IsChecked = true;
          break;
        case ButtonType.Switch:
          RbSwitch.IsChecked = true;
          break;
        case ButtonType.Timer:
          RbTimer.IsChecked = true;
          break;
      }
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null!)
    {
      PropertyChanged?.Invoke(this, new(name));
    }

    private void TypeRadioButton_Click(object sender, RoutedEventArgs e)
    {
      FunctionModel.ButtonType = (ButtonType)Enum.Parse(typeof(ButtonType), (sender as RadioButton)!.Tag!.ToString()!);
    }
  }
}