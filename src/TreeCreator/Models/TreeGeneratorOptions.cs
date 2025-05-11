using System;
using System.Collections.Generic;

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

	/// <summary>
	/// Gets the directory names to include in the tree. If not empty, only these directories will be included.
	/// </summary>
	public HashSet<string> IncludedDirectories { get; } = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Gets the file extensions to include in the tree. If not empty, only files with these extensions will be included.
	/// </summary>
	public HashSet<string> IncludedExtensions { get; } = new(StringComparer.OrdinalIgnoreCase);
}