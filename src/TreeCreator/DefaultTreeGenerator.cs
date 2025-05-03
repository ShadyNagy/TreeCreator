using System;
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

		var result = new TreeResult(rootPath);
		ProcessDirectory(rootPath, result, "");
		return result;
	}

	private void ProcessDirectory(string path, TreeResult result, string indent)
	{
		var dirInfo = new DirectoryInfo(path);

		// Get all directories that aren't excluded
		var directories = dirInfo.GetDirectories()
				.Where(d => !_options.ExcludedDirectories.Contains(d.Name))
				.OrderBy(d => d.Name)
				.ToList();

		// Get all files that don't have excluded extensions
		var files = dirInfo.GetFiles()
				.Where(f => !_options.ExcludedExtensions.Contains(f.Extension))
				.OrderBy(f => f.Name)
				.ToList();

		// Process directories
		for (int i = 0; i < directories.Count; i++)
		{
			var dir = directories[i];
			bool isLast = (i == directories.Count - 1) && (files.Count == 0);
			result.AppendLine($"{indent}{(isLast ? "└── " : "├── ")}{dir.Name}/");
			string newIndent = indent + (isLast ? "    " : "│   ");
			ProcessDirectory(dir.FullName, result, newIndent);
		}

		// Process files
		for (int i = 0; i < files.Count; i++)
		{
			var file = files[i];
			bool isLast = (i == files.Count - 1);
			result.AppendLine($"{indent}{(isLast ? "└── " : "├── ")}{file.Name}");
		}
	}
}