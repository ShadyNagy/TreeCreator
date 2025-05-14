using System;
using System.Collections.Generic;

namespace TreeCreator.Models;

/// <summary>
/// Represents the result of a directory tree generation operation.
/// </summary>
public class TreeResult
{
	private readonly string _rootPath;
	private readonly List<string> _lines = new();

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
}