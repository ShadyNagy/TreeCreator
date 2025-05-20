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

		if (isPrintRoot)
			_lines.Add($"{_rootPath}/");
		else
			_lines.Add("/");

		string rootName = Path.GetFileName(rootPath);
		if (string.IsNullOrEmpty(rootName))
			rootName = rootPath;

		Root = new TreeItem(rootName, rootPath, true, true);
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
	/// Creates or gets an item in the tree structure.
	/// </summary>
	/// <param name="path">The full path of the item.</param>
	/// <param name="isFolder">Whether this item is a folder.</param>
	/// <param name="canExpand">Whether this item can be expanded.</param>
	/// <returns>The created or existing tree item.</returns>
	public TreeItem CreateOrGetItem(string path, bool isFolder, bool canExpand)
	{
		if (string.Equals(path, _rootPath, StringComparison.OrdinalIgnoreCase))
			return Root;

		path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

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

		var newItem = new TreeItem(name, path, isFolder, canExpand);
		parent.AddChild(newItem);
		return newItem;
	}
}