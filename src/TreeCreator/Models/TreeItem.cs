using System;
using System.Collections.Generic;

namespace TreeCreator.Models;

/// <summary>
/// Represents an item in the directory tree structure.
/// </summary>
public class TreeItem
{
	/// <summary>
	/// Gets the name of the item.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the full path of the item starting from the root.
	/// </summary>
	public string FullPath { get; }

	/// <summary>
	/// Gets the relative path of the item with respect to the root directory.
	/// </summary>
	public string RelativePath { get; }

	/// <summary>
	/// Gets a value indicating whether this item is a folder.
	/// </summary>
	public bool IsFolder { get; }

	/// <summary>
	/// Gets a value indicating whether this item can be expanded (has children).
	/// </summary>
	public bool IsExpand { get; }

	/// <summary>
	/// Gets the child items if this is a folder.
	/// </summary>
	public List<TreeItem> Children { get; } = new List<TreeItem>();

	/// <summary>
	/// Initializes a new instance of the <see cref="TreeItem"/> class.
	/// </summary>
	/// <param name="name">The name of the item.</param>
	/// <param name="fullPath">The full path of the item.</param>
	/// <param name="relativePath">The relative path of the item from the root.</param>
	/// <param name="isFolder">Whether this item is a folder.</param>
	/// <param name="isExpand">Whether this item can be expanded.</param>
	public TreeItem(string name, string fullPath, string relativePath, bool isFolder, bool isExpand)
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
		FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
		RelativePath = relativePath ?? throw new ArgumentNullException(nameof(relativePath));
		IsFolder = isFolder;
		IsExpand = isExpand;
	}

	/// <summary>
	/// Adds a child item to this folder.
	/// </summary>
	/// <param name="child">The child item to add.</param>
	public void AddChild(TreeItem child)
	{
		if (!IsFolder)
			throw new InvalidOperationException("Cannot add children to a file item.");

		Children.Add(child);
	}
}