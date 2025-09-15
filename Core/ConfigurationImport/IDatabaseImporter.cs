using System.IO;
using System.Threading.Tasks;

namespace Core.ConfigurationImport
{
  public interface IDatabaseImporter
  {
    Task ImportAsync(FileInfo z21File);
  }
}