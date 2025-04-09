namespace it_tools.ToolDevelopment.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ToolComponentAttribute : Attribute
{
    public bool IsMainComponent { get; set; } = true;
    
    public ToolComponentAttribute(bool isMainComponent = true)
    {
        IsMainComponent = isMainComponent;
    }
}