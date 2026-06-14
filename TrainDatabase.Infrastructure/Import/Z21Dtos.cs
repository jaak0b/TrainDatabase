using System.ComponentModel.DataAnnotations.Schema;

namespace TrainDatabase.Infrastructure.Import;

// Column names match the Roco Z21 export schema (snake_case) and are read via Dapper.
[Table("vehicles")]
internal sealed class VehicleDto
{
    public long id { get; set; }
    public string name { get; set; } = "";
    public string image_name { get; set; } = "";
    public long type { get; set; }
    public long max_speed { get; set; }
    public long address { get; set; }
    public long active { get; set; }
    public long position { get; set; }
    public string full_name { get; set; } = "";
    public long speed_display { get; set; }
    public string railway { get; set; } = "";
    public long traction_direction { get; set; }
    public string description { get; set; } = "";
    public long dummy { get; set; }
}

[Table("functions")]
internal sealed class FunctionDto
{
    public long id { get; set; }
    public long vehicle_id { get; set; }
    public long button_type { get; set; }
    public string shortcut { get; set; } = "";
    public string time { get; set; } = "";
    public long position { get; set; }
    public string image_name { get; set; } = "";
    public long function { get; set; }
    public long show_function_number { get; set; }
    public long is_configured { get; set; }
}
