using System;
using System.Collections.Generic;

namespace Data_Mapping.Core.Models;

public partial class ClientSchemaVector
{
    public int Id { get; set; }

    public string CompanyId { get; set; } = null!;

    public string TableName { get; set; } = null!;

    public string? TableDescription { get; set; }

    public string? TableRelations { get; set; }

    public List<string>? TeamName { get; set; }
}
