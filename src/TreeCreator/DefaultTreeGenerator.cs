using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TreeCreator.Interfaces;
using TreeCreator.Models;

namespace TreeCreator;

/// <summary>
/// Default implementation of <see cref="ITreeCreator"/>.
/// </summary>
public class DefaultTreeCreator : ITreeCreator
{
	private readonly TreeCreatorOptions _options = new();
	private readonly HashSet<string> _includedPaths = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Excludes specified directories from the tree.
	/// </summary>
	/// <param name="directoryNames">The directory names to exclude.</param>
	/// <returns>The current instance for method chaining.</returns>
	public ITreeCreator ExcludeDirectories(params string[] directoryNames)
	{
		foreach (var dir in directoryNames)
		{
			_options.ExcludedDirectories.Add(dir);
		}
		return this;
	}

	/// <summary>
	/// Excludes specified file extensions from the tree.
	/// </summary>
	/// <param name="extensions">The file extensions to exclude.</param>
	/// <returns>The current instance for method chaining.</returns>
	public ITreeCreator ExcludeExtensions(params string[] extensions)
	{
		foreach (var ext in extensions)
		{
			_options.ExcludedExtensions.Add(ext);
		}
		return this;
	}

	/// <summary>
	/// Includes only the specified directories and their subdirectories in the tree.
	/// </summary>
	/// <param name="directoryNames">The directory names to include.</param>
	/// <returns>The current instance for method chaining.</returns>
	public ITreeCreator IncludeOnlyDirectories(params string[] directoryNames)
	{
		foreach (var dir in directoryNames)
		{
			_options.IncludedDirectories.Add(dir);
		}
		return this;
	}

	/// <summary>
	/// Includes only the specified file extensions in the tree.
	/// </summary>
	/// <param name="extensions">The file extensions to include.</param>
	/// <returns>The current instance for method chaining.</returns>
	public ITreeCreator IncludeOnlyExtensions(params string[] extensions)
	{
		foreach (var ext in extensions)
		{
			// Ensure the extension includes the dot
			var formattedExt = ext.StartsWith(".") ? ext : $".{ext}";
			_options.IncludedExtensions.Add(formattedExt);
		}
		return this;
	}

	/// <summary>
	/// Generates a tree from the specified root path.
	/// </summary>
	/// <param name="rootPath">The root path to generate the tree from.</param>
	/// <returns>The generated tree result.</returns>
	/// <exception cref="ArgumentException">Thrown when the specified path is invalid.</exception>
	/// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist.</exception>
	public TreeResult Generate(string? rootPath)
	{
		if (string.IsNullOrWhiteSpace(rootPath))
			throw new ArgumentException("Root path cannot be empty.", nameof(rootPath));

		if (!Directory.Exists(rootPath))
			throw new DirectoryNotFoundException($"Directory '{rootPath}' does not exist.");

		// If we have included directories, find their full paths first
		if (_options.IncludedDirectories.Count > 0)
		{
			_includedPaths.Clear();
			FindIncludedDirectoryPaths(rootPath);
		}

		var result = new TreeResult(rootPath);
		ProcessDirectory(rootPath, result, "", true);
		return result;
	}

	private void FindIncludedDirectoryPaths(string rootPath)
	{
		var dirInfo = new DirectoryInfo(rootPath);

		// Check if this directory should be included
		if (_options.IncludedDirectories.Contains(dirInfo.Name))
		{
			_includedPaths.Add(dirInfo.FullName);
			return; // No need to search subdirectories as they'll be included anyway
		}

		// Recursively search in subdirectories
		foreach (var subDir in dirInfo.GetDirectories())
		{
			FindIncludedDirectoryPaths(subDir.FullName);
		}
	}

	private bool ShouldIncludeDirectory(DirectoryInfo dirInfo)
	{
		// If no directories are specifically included, include all directories except excluded ones
		if (_options.IncludedDirectories.Count == 0)
		{
			return !_options.ExcludedDirectories.Contains(dirInfo.Name);
		}

		// If this directory is in the included list, include it
		if (_options.IncludedDirectories.Contains(dirInfo.Name))
		{
			return true;
		}

		// If any ancestor of this directory is in the included paths, include it
		string currentPath = dirInfo.FullName;
		return _includedPaths.Any(includedPath =>
				currentPath.StartsWith(includedPath, StringComparison.OrdinalIgnoreCase));
	}

	private void ProcessDirectory(string path, TreeResult result, string indent, bool isRootLevel)
	{
		var dirInfo = new DirectoryInfo(path);

		// Skip this directory and its contents if it's not the root level and should not be included
		if (!isRootLevel && !ShouldIncludeDirectory(dirInfo))
		{
			return;
		}

		// Get directories
		var directories = dirInfo.GetDirectories();
		var filteredDirectories = directories
				.Where(d => !_options.ExcludedDirectories.Contains(d.Name))
				.OrderBy(d => d.Name)
				.ToList();

		// Get files based on include/exclude rules
		var files = dirInfo.GetFiles();
		var filteredFiles = files
				.Where(f => !_options.ExcludedExtensions.Contains(f.Extension))
				.Where(f => _options.IncludedExtensions.Count == 0 || _options.IncludedExtensions.Contains(f.Extension))
				.OrderBy(f => f.Name)
				.ToList();

		// Process directories
		for (int i = 0; i < filteredDirectories.Count; i++)
		{
			var dir = filteredDirectories[i];
			bool isLast = (i == filteredDirectories.Count - 1) && (filteredFiles.Count == 0);

			// Check if we should include this directory or its subdirectories
			bool shouldIncludeThisDir = ShouldIncludeDirectory(dir);

			if (shouldIncludeThisDir)
			{
				result.AppendLine($"{indent}{(isLast ? "└── " : "├── ")}{dir.Name}/");
				string newIndent = indent + (isLast ? "    " : "│   ");
				ProcessDirectory(dir.FullName, result, newIndent, false);
			}
		}

		// Process files
		for (int i = 0; i < filteredFiles.Count; i++)
		{
			var file = filteredFiles[i];
			bool isLast = (i == filteredFiles.Count - 1);
			result.AppendLine($"{indent}{(isLast ? "└── " : "├── ")}{file.Name}");
		}
	}
}