using System;
using System.Reflection;

/// <summary>
/// Attribute to denote the project root directory.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
internal class TemplateMetadataPathAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateMetadataPathAttribute" /> class.
    /// </summary>
    /// <param name="templateMetadataPath">Path to the commands template metadata file.</param>
    public TemplateMetadataPathAttribute(string templateMetadataPath)
    {
        TemplateMetadataPath = templateMetadataPath;
    }

    /// <summary>
    /// Gets the project root directory for the executing assembly.
    /// </summary>
    public static string ThisAssemblyTemplateMetadataPath
    {
        get
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var attr = thisAssembly.GetCustomAttribute<TemplateMetadataPathAttribute>();
            return attr!.TemplateMetadataPath;
        }
    }

    /// <summary>
    /// Gets the project root directory.
    /// </summary>
    private string TemplateMetadataPath { get; }
}
