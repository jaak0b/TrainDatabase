using System.IO.Ports;
using System.Text;
using System.Text.Json;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Infrastructure.Hardware;

/// <summary>
/// Reads measured durations from an Arduino speed sensor over a serial port.
/// The Arduino emits JSON lines of the form <c>{"duration": 123.4}</c>.
/// </summary>
public sealed class ArduinoSpeedSensorAdapter : ISpeedSensorPort
{
    private readonly SerialPort serialPort;
    private readonly StringBuilder buffer = new();
    private bool disposed;

    public ArduinoSpeedSensorAdapter(string portName, int baudRate = 9600)
    {
        serialPort = new SerialPort(portName, baudRate)
        {
            DtrEnable = true,
            RtsEnable = true,
            NewLine = "\n",
        };

        serialPort.DataReceived += SerialPort_DataReceived;
        serialPort.Open();
    }

    public async Task<decimal?> ReadDurationAsync(TimeSpan timeout)
    {
        using CancellationTokenSource cts = new(timeout);

        while (!cts.IsCancellationRequested)
        {
            string buffered = buffer.ToString();
            int newlineIndex = buffered.IndexOf('\n');
            if (newlineIndex >= 0)
            {
                string line = buffered[..newlineIndex].Trim();
                buffer.Remove(0, newlineIndex + 1);

                try
                {
                    using JsonDocument document = JsonDocument.Parse(line);
                    if (document.RootElement.TryGetProperty("duration", out JsonElement duration)
                        && duration.TryGetDecimal(out decimal value))
                    {
                        return value;
                    }
                }
                catch (JsonException)
                {
                    // Ignore malformed JSON lines.
                }
            }

            try
            {
                await Task.Delay(10, cts.Token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        return null;
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) =>
        buffer.Append(serialPort.ReadExisting());

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        serialPort.DataReceived -= SerialPort_DataReceived;
        if (serialPort.IsOpen)
        {
            serialPort.Close();
        }

        serialPort.Dispose();
    }
}
