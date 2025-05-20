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
	/// <param name="directoryPaths">The directory paths to include.</param>
	/// <returns>The current instance for method chaining.</returns>
	public ITreeCreator IncludeOnlyDirectories(params string[] directoryPaths)
	{
		foreach (var dirPath in directoryPaths)
		{
			// Normalize the path to use standard directory separators
			string normalizedPath = NormalizePath(dirPath);
			_options.IncludedDirectories.Add(normalizedPath);
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
			var formattedExt = ext.StartsWith(".") ? ext : $".{ext}";
			_options.IncludedExtensions.Add(formattedExt);
		}
		return this;
	}

	/// <summary>
	/// Generates a tree from the specified root path.
	/// </summary>
	/// <param name="rootPath">The root path to generate the tree from.</param>
	/// <param name="isPrintRoot">False to not print the root in the tree.</param>
	/// <returns>The generated tree result.</returns>
	/// <exception cref="ArgumentException">Thrown when the specified path is invalid.</exception>
	/// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist.</exception>
	public TreeResult Generate(string? rootPath, bool isPrintRoot = true)
	{
		if (string.IsNullOrWhiteSpace(rootPath))
			throw new ArgumentException("Root path cannot be empty.", nameof(rootPath));

		if (!Directory.Exists(rootPath))
			throw new DirectoryNotFoundException($"Directory '{rootPath}' does not exist.");

		if (_options.IncludedDirectories.Count > 0)
		{
			_includedPaths.Clear();
			FindMatchingPaths(rootPath);
		}

		var result = new TreeResult(rootPath, isPrintRoot);
		ProcessDirectory(rootPath, result, "", true);
		return result;
	}

	private string NormalizePath(string path)
	{
		return path.Replace('\\', '/').TrimEnd('/');
	}

	private void FindMatchingPaths(string rootPath)
	{
		Queue<string> directories = new Queue<string>();
		directories.Enqueue(rootPath);

		while (directories.Count > 0)
		{
			string currentDir = directories.Dequeue();
			string normalizedCurrentDir = NormalizePath(currentDir);

			var dirInfo = new DirectoryInfo(currentDir);
			if (_options.IncludedDirectories.Contains(dirInfo.Name))
			{
				_includedPaths.Add(currentDir);
				continue;
			}

			foreach (var includedPath in _options.IncludedDirectories)
			{
				if (normalizedCurrentDir.EndsWith("/" + includedPath, StringComparison.OrdinalIgnoreCase) ||
						normalizedCurrentDir.EndsWith(includedPath, StringComparison.OrdinalIgnoreCase))
				{
					_includedPaths.Add(currentDir);
					break;
				}

				string[] includedPathParts = includedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
				if (includedPathParts.Length > 1)
				{
					string[] currentPathParts = normalizedCurrentDir.Split('/', StringSplitOptions.RemoveEmptyEntries);

					bool isMatch = IsPathMatch(currentPathParts, includedPathParts);
					if (isMatch)
					{
						_includedPaths.Add(currentDir);
						break;
					}
				}
			}

			if (TryGetDirectories(currentDir, out DirectoryInfo[] subdirectories))
			{
				foreach (var subDir in subdirectories)
				{
					directories.Enqueue(subDir.FullName);
				}
			}
		}
	}

	private bool IsPathMatch(string[] pathParts, string[] includePathParts)
	{
		if (includePathParts.Length == 0)
			return false;

		int startIndex = -1;
		for (int i = 0; i < pathParts.Length; i++)
		{
			if (string.Equals(pathParts[i], includePathParts[0], StringComparison.OrdinalIgnoreCase))
			{
				startIndex = i;
				break;
			}
		}

		if (startIndex == -1 || startIndex + includePathParts.Length > pathParts.Length)
			return false;

		for (int i = 0; i < includePathParts.Length; i++)
		{
			if (!string.Equals(pathParts[startIndex + i], includePathParts[i], StringComparison.OrdinalIgnoreCase))
				return false;
		}

		return true;
	}

	private bool ShouldIncludeDirectory(DirectoryInfo dirInfo)
	{
		if (_options.IncludedDirectories.Count == 0)
		{
			return !_options.ExcludedDirectories.Contains(dirInfo.Name);
		}

		if (_options.IncludedDirectories.Contains(dirInfo.Name))
		{
			return true;
		}

		string currentPath = dirInfo.FullName;
		return _includedPaths.Any(includedPath =>
										currentPath.StartsWith(includedPath, StringComparison.OrdinalIgnoreCase) ||
										includedPath.StartsWith(currentPath, StringComparison.OrdinalIgnoreCase));
	}

	private void ProcessDirectory(string path, TreeResult result, string indent, bool isRootLevel)
	{
		var dirInfo = new DirectoryInfo(path);

		if (!isRootLevel && !ShouldIncludeDirectory(dirInfo))
		{
			return;
		}

		TryGetDirectories(path, out var directories);

		var filteredDirectories = directories
				.Where(d => !_options.ExcludedDirectories.Contains(d.Name))
				.OrderBy(d => d.Name)
				.ToList();

		TryGetFiles(path, out var files);

		var filteredFiles = files
				.Where(f => !_options.ExcludedExtensions.Contains(f.Extension))
				.Where(f => _options.IncludedExtensions.Count == 0 || _options.IncludedExtensions.Contains(f.Extension))
				.OrderBy(f => f.Name)
				.ToList();

		for (int i = 0; i < filteredDirectories.Count; i++)
		{
			var dir = filteredDirectories[i];
			bool isLast = (i == filteredDirectories.Count - 1) && (filteredFiles.Count == 0);

			bool shouldIncludeThisDir = ShouldIncludeDirectory(dir);

			if (shouldIncludeThisDir)
			{
				result.AppendLine($"{indent}{(isLast ? "└── " : "├── ")}{dir.Name}/");
				string newIndent = indent + (isLast ? "    " : "│   ");

				bool canExpand = DirectoryHasVisibleContents(dir);
				result.CreateOrGetItem(dir.FullName, true, canExpand);

				ProcessDirectory(dir.FullName, result, newIndent, false);
			}
		}

		for (int i = 0; i < filteredFiles.Count; i++)
		{
			var file = filteredFiles[i];
			bool isLast = (i == filteredFiles.Count - 1);

			result.AppendLine($"{indent}{(isLast ? "└── " : "├── ")}{file.Name}");

			result.CreateOrGetItem(file.FullName, false, false);
		}
	}

	/// <summary>
	/// Determines if a directory has any visible contents according to the current filtering options.
	/// </summary>
	/// <param name="dir">The directory to check.</param>
	/// <returns>True if the directory has visible contents, false otherwise.</returns>
	private bool DirectoryHasVisibleContents(DirectoryInfo dir)
	{
		if (TryGetFiles(dir.FullName, out var files))
		{
			if (files.Any(f =>
					!_options.ExcludedExtensions.Contains(f.Extension) &&
					(_options.IncludedExtensions.Count == 0 || _options.IncludedExtensions.Contains(f.Extension))))
			{
				return true;
			}
		}

		if (TryGetDirectories(dir.FullName, out var subdirs))
		{
			if (subdirs.Any(d => ShouldIncludeDirectory(d)))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Safely attempts to get directories, handling long paths.
	/// </summary>
	/// <param name="path">The directory path.</param>
	/// <param name="directories">The retrieved directories if successful.</param>
	/// <returns>True if successful, false otherwise.</returns>
	private bool TryGetDirectories(string path, out DirectoryInfo[] directories)
	{
		try
		{
			string longPathSafePath = path.Length > 250 && !path.StartsWith(@"\\?\") && Path.DirectorySeparatorChar == '\\'
							? @"\\?\" + path
							: path;

			var dir = new DirectoryInfo(longPathSafePath);
			directories = dir.GetDirectories();
			return true;
		}
		catch (PathTooLongException ex)
		{
			Console.WriteLine($"Path too long: {path}. Error: {ex.Message}");
			directories = Array.Empty<DirectoryInfo>();
			return false;
		}
		catch (Exception ex)
		{
			directories = Array.Empty<DirectoryInfo>();
			return false;
		}
	}

	/// <summary>
	/// Safely attempts to get files, handling long paths.
	/// </summary>
	/// <param name="path">The directory path.</param>
	/// <param name="files">The retrieved files if successful.</param>
	/// <returns>True if successful, false otherwise.</returns>
	private bool TryGetFiles(string path, out FileInfo[] files)
	{
		try
		{
			string longPathSafePath = path.Length > 250 && !path.StartsWith(@"\\?\") && Path.DirectorySeparatorChar == '\\'
							? @"\\?\" + path
							: path;

			var dir = new DirectoryInfo(longPathSafePath);
			files = dir.GetFiles();
			return true;
		}
		catch (PathTooLongException ex)
		{
			Console.WriteLine($"Path too long: {path}. Error: {ex.Message}");
			files = Array.Empty<FileInfo>();
			return false;
		}
		catch (Exception ex)
		{
			files = Array.Empty<FileInfo>();
			return false;
		}
	}
}