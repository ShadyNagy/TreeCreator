using TreeCreator.Models;

namespace TreeCreator.UnitTests;
public class DefaultTreeCreatorTests
{
	[Fact]
	public void Generate_WithValidDirectory_ReturnsTreeResult()
	{
		// Arrange
		var generator = TreeCreatorFactory.Create();
		var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempDir);

		try
		{
			// Act
			var result = generator.Generate(tempDir);

			// Assert
			Assert.NotNull(result);
			Assert.IsType<TreeResult>(result);
			Assert.Contains(tempDir, result.ToString());
		}
		finally
		{
			Directory.Delete(tempDir);
		}
	}

	[Fact]
	public void Generate_WithNullPath_ThrowsArgumentException()
	{
		// Arrange
		var generator = TreeCreatorFactory.Create();

		// Act & Assert
		Assert.Throws<ArgumentException>(() => generator.Generate(null));
	}

	[Fact]
	public void Generate_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
	{
		// Arrange
		var generator = TreeCreatorFactory.Create();
		var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

		// Act & Assert
		Assert.Throws<DirectoryNotFoundException>(() => generator.Generate(nonExistentPath));
	}
}