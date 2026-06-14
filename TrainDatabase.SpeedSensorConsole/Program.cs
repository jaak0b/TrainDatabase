using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using TrainDatabase.Composition;
using TrainDatabase.Core.Ports;

// Headless smoke-test harness: builds the real composition root, applies migrations, and
// lists the data + serial ports so the Infrastructure wiring can be exercised without a GUI.
IServiceProvider services = await Bootstrapper.InitializeAsync();

IVehicleRepository vehicles = services.GetRequiredService<IVehicleRepository>();
ISerialDeviceProvider serial = services.GetRequiredService<ISerialDeviceProvider>();

AnsiConsole.MarkupLine("[green]Database initialized.[/]");

IReadOnlyCollection<TrainDatabase.Core.Domain.Vehicle> all = vehicles.FullTextSearchVehicles(null);
AnsiConsole.MarkupLine($"Vehicles in database: [yellow]{all.Count}[/]");

string[] ports = serial.GetPortNames().ToArray();
AnsiConsole.MarkupLine(ports.Length == 0
    ? "[grey]No serial ports detected.[/]"
    : $"Serial ports: [yellow]{string.Join(", ", ports)}[/]");
