using System;
using System.Collections.Generic;
using System.IO;

namespace TreeCreator.Models;

/// <summary>
/// Represents the result of a directory tree generation operation.
/// </summary>
public class TreeResult
{
	private readonly string _rootPath;
	private readonly List<string> _lines = new();

	/// <summary>
	/// Gets the root item of the tree structure.
	/// </summary>
	public TreeItem Root { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TreeResult"/> class.
	/// </summary>
	/// <param name="rootPath">The root path of the directory tree.</param>
	/// <param name="isPrintRoot">False to not print the root in the tree.</param>
	public TreeResult(string rootPath, bool isPrintRoot)
	{
		_rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));

		// Normalize root path
		_rootPath = Path.GetFullPath(_rootPath);

		if (isPrintRoot)
			_lines.Add($"{_rootPath}/");
		else
			_lines.Add("/");

		// Create the root TreeItem
		string rootName = Path.GetFileName(_rootPath);
		if (string.IsNullOrEmpty(rootName)) // Handle root drives
			rootName = _rootPath;

		Root = new TreeItem(rootName, _rootPath, ".", true, true);
	}

	/// <summary>
	/// Appends a line to the tree result.
	/// </summary>
	/// <param name="line">The line to append.</param>
	public void AppendLine(string line) => _lines.Add(line);

	/// <summary>
	/// Gets all the lines in the tree result.
	/// </summary>
	public IReadOnlyList<string> Lines => _lines.AsReadOnly();

	/// <summary>
	/// Returns the string representation of the tree result.
	/// </summary>
	/// <returns>A string representation of the tree result.</returns>
	public override string ToString() => string.Join(Environment.NewLine, _lines);

	/// <summary>
	/// Converts an absolute path to a path relative to the root directory.
	/// </summary>
	/// <param name="path">The absolute path to convert.</param>
	/// <returns>The relative path.</returns>
	public string GetRelativePath(string path)
	{
		// Ensure the path uses the correct directory separator
		path = Path.GetFullPath(path);

		// Handle case where path is the root
		if (string.Equals(path, _rootPath, StringComparison.OrdinalIgnoreCase))
			return ".";

		// Create relative path
		if (path.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
		{
			string relativePath = path.Substring(_rootPath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			return relativePath.Length == 0 ? "." : relativePath;
		}

		// If path is not under the root, use Path.GetRelativePath
		return Path.GetRelativePath(_rootPath, path);
	}

	/// <summary>
	/// Converts a relative path to an absolute path based on the root directory.
	/// </summary>
	/// <param name="relativePath">The relative path to convert.</param>
	/// <returns>The absolute path.</returns>
	public string GetAbsolutePath(string relativePath)
	{
		if (string.IsNullOrEmpty(relativePath) || relativePath == ".")
			return _rootPath;

		return Path.GetFullPath(Path.Combine(_rootPath, relativePath));
	}

	/// <summary>
	/// Creates or gets an item in the tree structure using an absolute path.
	/// </summary>
	/// <param name="path">The full path of the item.</param>
	/// <param name="isFolder">Whether this item is a folder.</param>
	/// <param name="canExpand">Whether this item can be expanded.</param>
	/// <returns>The created or existing tree item.</returns>
	public TreeItem CreateOrGetItem(string path, bool isFolder, bool canExpand)
	{
		path = Path.GetFullPath(path);

		if (string.Equals(path, _rootPath, StringComparison.OrdinalIgnoreCase))
			return Root;

		string relativePath = GetRelativePath(path);

		string? parentPath = Path.GetDirectoryName(path);
		if (string.IsNullOrEmpty(parentPath))
			parentPath = _rootPath;

		string name = Path.GetFileName(path);

		TreeItem parent;
		if (string.Equals(parentPath, _rootPath, StringComparison.OrdinalIgnoreCase))
			parent = Root;
		else
			parent = CreateOrGetItem(parentPath, true, true);

		TreeItem? existingItem = parent.Children.Find(c =>
				string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

		if (existingItem != null)
			return existingItem;

		var newItem = new TreeItem(name, path, relativePath, isFolder, canExpand);
		parent.AddChild(newItem);
		return newItem;
	}

	/// <summary>
	/// Creates or gets an item in the tree structure using a relative path.
	/// </summary>
	/// <param name="relativePath">The relative path of the item with respect to the root.</param>
	/// <param name="isFolder">Whether this item is a folder.</param>
	/// <param name="canExpand">Whether this item can be expanded.</param>
	/// <returns>The created or existing tree item.</returns>
	public TreeItem CreateOrGetItemByRelativePath(string relativePath, bool isFolder, bool canExpand)
	{
		string absolutePath = GetAbsolutePath(relativePath);
		return CreateOrGetItem(absolutePath, isFolder, canExpand);
	}
}