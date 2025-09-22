using System.IO.Ports;
using System.Text;
using System.Text.Json;
using Core;

namespace Adapter
{
  public class ArduinoSpeedSensorAdapter : ISpeedSensorPort
  {
    private readonly SerialPort serialPort;
    private readonly StringBuilder buffer = new();
    private bool disposed;
    
    public ArduinoSpeedSensorAdapter(string portName, int baudRate = 9600)
    {
      serialPort = new(portName, baudRate)
                   {
                     DtrEnable = true,
                     RtsEnable = true,
                     NewLine = "\n"
                   };

      serialPort.DataReceived += SerialPort_DataReceived;
      serialPort.Open();
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      string incoming = serialPort.ReadExisting();
      buffer.Append(incoming);
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
            using JsonDocument doc = JsonDocument.Parse(line);
            if (doc.RootElement.TryGetProperty("duration", out JsonElement durationElement) && durationElement.TryGetDecimal(out decimal duration))
            {
              return duration;
            }
          }
          catch (JsonException)
          {
            // Ignore malformed JSON
          }
        }

        await Task.Delay(10, cts.Token);
      }

      return null;
    }

    public void Dispose()
    {
      if (disposed) return;
      disposed = true;

      serialPort.DataReceived -= SerialPort_DataReceived;

      if (serialPort.IsOpen)
      {
        serialPort.Close();
      }

      serialPort.Dispose();
    }
  }
}