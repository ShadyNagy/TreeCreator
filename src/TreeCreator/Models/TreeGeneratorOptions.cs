namespace TreeCreator.Models;

/// <summary>
/// Options for configuring the tree generator.
/// </summary>
public class TreeCreatorOptions
{
	/// <summary>
	/// Gets the directory names to exclude from the tree.
	/// </summary>
	public HashSet<string> ExcludedDirectories { get; } = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Gets the file extensions to exclude from the tree.
	/// </summary>
	public HashSet<string> ExcludedExtensions { get; } = new(StringComparer.OrdinalIgnoreCase);
}