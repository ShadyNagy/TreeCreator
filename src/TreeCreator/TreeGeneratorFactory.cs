using TreeCreator.Interfaces;

namespace TreeCreator;

/// <summary>
/// Factory for creating tree generators.
/// </summary>
public static class TreeCreatorFactory
{
	/// <summary>
	/// Creates a new tree generator.
	/// </summary>
	/// <returns>A new tree generator.</returns>
	public static ITreeCreator Create() => new DefaultTreeCreator();
}