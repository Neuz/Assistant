using System;
using System.Windows.Media;

namespace Assistant.Model;

public class ToolModel
{
    public string? Name { get; set; }
    public Geometry? Icon { get; set; }
    public string? IconColor { get; set; }
    public string? Description { get; set; }
    public string? BinPath { get; set; }

    public Action<ToolModel>? RunAction { get; set; }
    public Action<ToolModel>? HelpAction { get; set; }
}