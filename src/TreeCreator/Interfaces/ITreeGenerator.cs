using TreeCreator.Models;

namespace TreeCreator.Interfaces;

/// <summary>
/// Interface for tree generator implementations.
/// </summary>
public interface ITreeCreator
{
	/// <summary>
	/// Excludes specified directories from the tree.
	/// </summary>
	/// <param name="directoryNames">The directory names to exclude.</param>
	/// <returns>The current instance for method chaining.</returns>
	ITreeCreator ExcludeDirectories(params string[] directoryNames);

	/// <summary>
	/// Excludes specified file extensions from the tree.
	/// </summary>
	/// <param name="extensions">The file extensions to exclude.</param>
	/// <returns>The current instance for method chaining.</returns>
	ITreeCreator ExcludeExtensions(params string[] extensions);

	/// <summary>
	/// Includes only the specified directories in the tree.
	/// </summary>
	/// <param name="directoryNames">The directory names to include.</param>
	/// <returns>The current instance for method chaining.</returns>
	ITreeCreator IncludeOnlyDirectories(params string[] directoryNames);

	/// <summary>
	/// Includes only the specified file extensions in the tree.
	/// </summary>
	/// <param name="extensions">The file extensions to include.</param>
	/// <returns>The current instance for method chaining.</returns>
	ITreeCreator IncludeOnlyExtensions(params string[] extensions);

	/// <summary>
	/// Generates a tree from the specified root path.
	/// </summary>
	/// <param name="rootPath">The root path to generate the tree from.</param>
	/// <returns>The generated tree result.</returns>
	TreeResult Generate(string? rootPath);
}