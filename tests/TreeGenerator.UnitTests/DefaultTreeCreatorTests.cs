using System;
using System.IO;
using System.Linq;
using TreeCreator;
using TreeCreator.Models;

namespace TreeCreator.UnitTests
{
	public class DefaultTreeCreatorTests
	{
		private string CreateTempDirectory()
		{
			var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempDir);
			return tempDir;
		}

		private void CleanupTempDirectory(string path)
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
		}

		[Fact]
		public void Generate_WithDirectoryStructure_ReturnsCorrectTreeStructure()
		{
			// Arrange
			var generator = TreeCreatorFactory.Create();
			var tempDir = CreateTempDirectory();

			try
			{
				// Create a simple directory structure
				var subDir1 = Path.Combine(tempDir, "dir1");
				var subDir2 = Path.Combine(tempDir, "dir2");
				var file1 = Path.Combine(tempDir, "file1.txt");
				var file2 = Path.Combine(subDir1, "file2.txt");

				Directory.CreateDirectory(subDir1);
				Directory.CreateDirectory(subDir2);
				File.WriteAllText(file1, "test");
				File.WriteAllText(file2, "test");

				// Act
				var result = generator.Generate(tempDir);

				// Assert
				Assert.NotNull(result);
				Assert.NotNull(result.Root);
				Assert.Equal(Path.GetFileName(tempDir), result.Root.Name);
				Assert.Equal(3, result.Root.Children.Count); // 2 dirs + 1 file

				// Check the files and directories are correctly represented
				Assert.Contains(result.Root.Children, c => c.Name == "dir1" && c.IsFolder);
				Assert.Contains(result.Root.Children, c => c.Name == "dir2" && c.IsFolder);
				Assert.Contains(result.Root.Children, c => c.Name == "file1.txt" && !c.IsFolder);

				// Check the nested file
				var dir1 = result.Root.Children.First(c => c.Name == "dir1");
				Assert.Single(dir1.Children);
				Assert.Equal("file2.txt", dir1.Children[0].Name);

				// Check the string representation has the expected components
				string stringResult = result.ToString();
				Assert.Contains("dir1", stringResult);
				Assert.Contains("dir2", stringResult);
				Assert.Contains("file1.txt", stringResult);
				Assert.Contains("file2.txt", stringResult);
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void ExcludeDirectories_ExcludesSpecifiedDirectories()
		{
			// Arrange
			var generator = TreeCreatorFactory.Create()
					.ExcludeDirectories("dir1");

			var tempDir = CreateTempDirectory();

			try
			{
				// Create a simple directory structure
				var subDir1 = Path.Combine(tempDir, "dir1");
				var subDir2 = Path.Combine(tempDir, "dir2");

				Directory.CreateDirectory(subDir1);
				Directory.CreateDirectory(subDir2);

				// Act
				var result = generator.Generate(tempDir);

				// Assert
				Assert.NotNull(result);
				Assert.NotNull(result.Root);

				// Should only have dir2 (dir1 was excluded)
				Assert.Single(result.Root.Children);
				Assert.Equal("dir2", result.Root.Children[0].Name);

				// Check the string representation doesn't contain excluded dir
				string stringResult = result.ToString();
				Assert.DoesNotContain("dir1", stringResult);
				Assert.Contains("dir2", stringResult);
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void ExcludeExtensions_ExcludesSpecifiedFileExtensions()
		{
			// Arrange
			var generator = TreeCreatorFactory.Create()
					.ExcludeExtensions(".txt");

			var tempDir = CreateTempDirectory();

			try
			{
				// Create some files with different extensions
				var txtFile = Path.Combine(tempDir, "file1.txt");
				var jsonFile = Path.Combine(tempDir, "file2.json");

				File.WriteAllText(txtFile, "test");
				File.WriteAllText(jsonFile, "{}");

				// Act
				var result = generator.Generate(tempDir);

				// Assert
				Assert.NotNull(result);
				Assert.NotNull(result.Root);

				// Should only have json file (txt was excluded)
				Assert.Single(result.Root.Children);
				Assert.Equal("file2.json", result.Root.Children[0].Name);

				// Check the string representation
				string stringResult = result.ToString();
				Assert.DoesNotContain("file1.txt", stringResult);
				Assert.Contains("file2.json", stringResult);
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void IncludeOnlyDirectories_OnlyIncludesSpecifiedDirectories()
		{
			// Arrange
			var generator = TreeCreatorFactory.Create();

			var tempDir = CreateTempDirectory();

			try
			{
				// Create multiple directories
				var subDir1 = Path.Combine(tempDir, "dir1");
				var subDir2 = Path.Combine(tempDir, "dir2");
				var subDir3 = Path.Combine(tempDir, "dir3");

				Directory.CreateDirectory(subDir1);
				Directory.CreateDirectory(subDir2);
				Directory.CreateDirectory(subDir3);

				// Act - only include dir1
				generator.IncludeOnlyDirectories("dir1");
				var result = generator.Generate(tempDir);

				// Assert
				Assert.NotNull(result);
				Assert.NotNull(result.Root);

				// Should only have dir1
				Assert.Single(result.Root.Children);
				Assert.Equal("dir1", result.Root.Children[0].Name);

				// Check the string representation
				string stringResult = result.ToString();
				Assert.Contains("dir1", stringResult);
				Assert.DoesNotContain("dir2", stringResult);
				Assert.DoesNotContain("dir3", stringResult);
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void IncludeOnlyExtensions_OnlyIncludesSpecifiedFileExtensions()
		{
			// Arrange
			var generator = TreeCreatorFactory.Create();

			var tempDir = CreateTempDirectory();

			try
			{
				// Create files with different extensions
				var txtFile = Path.Combine(tempDir, "file1.txt");
				var jsonFile = Path.Combine(tempDir, "file2.json");
				var csFile = Path.Combine(tempDir, "file3.cs");

				File.WriteAllText(txtFile, "test");
				File.WriteAllText(jsonFile, "{}");
				File.WriteAllText(csFile, "class {}");

				// Act - only include .txt and .cs
				generator.IncludeOnlyExtensions("txt", "cs");
				var result = generator.Generate(tempDir);

				// Assert
				Assert.NotNull(result);
				Assert.NotNull(result.Root);

				// Should have txt and cs files (2 total)
				Assert.Equal(2, result.Root.Children.Count);
				Assert.Contains(result.Root.Children, c => c.Name == "file1.txt");
				Assert.Contains(result.Root.Children, c => c.Name == "file3.cs");

				// Check the string representation
				string stringResult = result.ToString();
				Assert.Contains("file1.txt", stringResult);
				Assert.DoesNotContain("file2.json", stringResult);
				Assert.Contains("file3.cs", stringResult);
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void MethodChaining_WorksCorrectly()
		{
			// Arrange
			var tempDir = CreateTempDirectory();

			try
			{
				// Create a simple directory structure
				var subDir1 = Path.Combine(tempDir, "dir1");
				var subDir2 = Path.Combine(tempDir, "dir2");
				var txtFile = Path.Combine(tempDir, "file1.txt");
				var jsonFile = Path.Combine(tempDir, "file2.json");

				Directory.CreateDirectory(subDir1);
				Directory.CreateDirectory(subDir2);
				File.WriteAllText(txtFile, "test");
				File.WriteAllText(jsonFile, "{}");

				// Act - chain multiple methods
				var result = TreeCreatorFactory.Create()
						.ExcludeDirectories("dir1")
						.IncludeOnlyExtensions("json")
						.Generate(tempDir);

				// Assert
				Assert.NotNull(result);
				Assert.NotNull(result.Root);

				// Should only have dir2 (dir1 excluded) and file2.json (only .json included)
				Assert.Equal(2, result.Root.Children.Count);
				Assert.Contains(result.Root.Children, c => c.Name == "dir2");
				Assert.Contains(result.Root.Children, c => c.Name == "file2.json");

				// Check the string representation
				string stringResult = result.ToString();
				Assert.DoesNotContain("dir1", stringResult);
				Assert.Contains("dir2", stringResult);
				Assert.DoesNotContain("file1.txt", stringResult);
				Assert.Contains("file2.json", stringResult);
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void Generate_WithDeepNestedStructure_GeneratesCorrectTree()
		{
			// Arrange
			var generator = TreeCreatorFactory.Create();
			var tempDir = CreateTempDirectory();

			try
			{
				// Create a nested directory structure
				var level1 = Path.Combine(tempDir, "level1");
				var level2 = Path.Combine(level1, "level2");
				var level3 = Path.Combine(level2, "level3");

				Directory.CreateDirectory(level1);
				Directory.CreateDirectory(level2);
				Directory.CreateDirectory(level3);

				var deepFile = Path.Combine(level3, "deepFile.txt");
				File.WriteAllText(deepFile, "test");

				// Act
				var result = generator.Generate(tempDir);

				// Assert
				Assert.NotNull(result);
				Assert.NotNull(result.Root);

				// Navigate through the tree structure
				Assert.Single(result.Root.Children);
				var dir1 = result.Root.Children[0];
				Assert.Equal("level1", dir1.Name);

				Assert.Single(dir1.Children);
				var dir2 = dir1.Children[0];
				Assert.Equal("level2", dir2.Name);

				Assert.Single(dir2.Children);
				var dir3 = dir2.Children[0];
				Assert.Equal("level3", dir3.Name);

				Assert.Single(dir3.Children);
				var fileNode = dir3.Children[0];
				Assert.Equal("deepFile.txt", fileNode.Name);

				// Check the string representation
				string stringResult = result.ToString();
				Assert.Contains("level1", stringResult);
				Assert.Contains("level2", stringResult);
				Assert.Contains("level3", stringResult);
				Assert.Contains("deepFile.txt", stringResult);
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void Generate_WithEmptyDirectory_GeneratesCorrectTree()
		{
			// Arrange
			var generator = TreeCreatorFactory.Create();
			var tempDir = CreateTempDirectory();

			try
			{
				// Act (don't add any files or subdirectories)
				var result = generator.Generate(tempDir);

				// Assert
				Assert.NotNull(result);
				Assert.NotNull(result.Root);

				// Empty directory should have no children
				Assert.Empty(result.Root.Children);

				// String representation should just have the root directory
				string stringResult = result.ToString();
				Assert.Contains(tempDir, stringResult);
				Assert.Equal(1, stringResult.Split(Environment.NewLine).Length);
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void Generate_WithPrintRootFalse_DoesNotPrintRootInOutput()
		{
			// Arrange
			var generator = TreeCreatorFactory.Create();
			var tempDir = CreateTempDirectory();

			try
			{
				var file = Path.Combine(tempDir, "file.txt");
				File.WriteAllText(file, "test");

				// Act
				var result = generator.Generate(tempDir, isPrintRoot: false);

				// Assert
				string stringResult = result.ToString();

				// Should not contain the full path to tempDir, but should still have the file
				Assert.DoesNotContain(tempDir, stringResult);
				Assert.Contains("/", stringResult); // Root shown as just '/'
				Assert.Contains("file.txt", stringResult);
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void TreeResult_GetRelativePath_ReturnsCorrectPaths()
		{
			// Arrange
			var tempDir = CreateTempDirectory();

			try
			{
				var result = new TreeResult(tempDir, true);

				// Paths to test
				var samePath = tempDir;
				var subDirPath = Path.Combine(tempDir, "subdir");
				var filePath = Path.Combine(tempDir, "file.txt");

				// Act & Assert
				Assert.Equal(".", result.GetRelativePath(samePath));
				Assert.Equal("subdir", result.GetRelativePath(subDirPath));
				Assert.Equal("file.txt", result.GetRelativePath(filePath));
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void TreeResult_GetAbsolutePath_ReturnsCorrectPaths()
		{
			// Arrange
			var tempDir = CreateTempDirectory();

			try
			{
				var result = new TreeResult(tempDir, true);

				// Relative paths to test
				var emptyPath = "";
				var dotPath = ".";
				var subDirPath = "subdir";
				var filePath = "file.txt";

				// Act & Assert
				Assert.Equal(tempDir, result.GetAbsolutePath(emptyPath));
				Assert.Equal(tempDir, result.GetAbsolutePath(dotPath));
				Assert.Equal(Path.Combine(tempDir, "subdir"), result.GetAbsolutePath(subDirPath));
				Assert.Equal(Path.Combine(tempDir, "file.txt"), result.GetAbsolutePath(filePath));
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void TreeItem_AddChild_AddsChildToFolderItem()
		{
			// Arrange
			var folderItem = new TreeItem("folder", "/folder", "folder", true, true);
			var fileItem = new TreeItem("file.txt", "/folder/file.txt", "folder/file.txt", false, false);

			// Act
			folderItem.AddChild(fileItem);

			// Assert
			Assert.Single(folderItem.Children);
			Assert.Equal(fileItem, folderItem.Children[0]);
		}

		[Fact]
		public void TreeItem_AddChild_ThrowsWhenAddingToFileItem()
		{
			// Arrange
			var fileItem = new TreeItem("file.txt", "/file.txt", "file.txt", false, false);
			var otherItem = new TreeItem("other.txt", "/other.txt", "other.txt", false, false);

			// Act & Assert
			Assert.Throws<InvalidOperationException>(() => fileItem.AddChild(otherItem));
		}

		[Fact]
		public void TreeItem_Constructor_ThrowsOnNullParameters()
		{
			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => new TreeItem(null, "/path", "path", false, false));
			Assert.Throws<ArgumentNullException>(() => new TreeItem("name", null, "path", false, false));
			Assert.Throws<ArgumentNullException>(() => new TreeItem("name", "/path", null, false, false));
		}

		[Fact]
		public void ExcludeDirectories_WorksWithMultipleDirectories()
		{
			// Arrange
			var generator = TreeCreatorFactory.Create()
					.ExcludeDirectories("dir1", "dir3");

			var tempDir = CreateTempDirectory();

			try
			{
				// Create multiple directories
				var subDir1 = Path.Combine(tempDir, "dir1");
				var subDir2 = Path.Combine(tempDir, "dir2");
				var subDir3 = Path.Combine(tempDir, "dir3");

				Directory.CreateDirectory(subDir1);
				Directory.CreateDirectory(subDir2);
				Directory.CreateDirectory(subDir3);

				// Act
				var result = generator.Generate(tempDir);

				// Assert
				Assert.NotNull(result);
				Assert.NotNull(result.Root);

				// Should only have dir2 (dir1 and dir3 excluded)
				Assert.Single(result.Root.Children);
				Assert.Equal("dir2", result.Root.Children[0].Name);

				// Check the string representation
				string stringResult = result.ToString();
				Assert.DoesNotContain("dir1", stringResult);
				Assert.Contains("dir2", stringResult);
				Assert.DoesNotContain("dir3", stringResult);
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void ExcludeExtensions_WorksWithMultipleExtensions()
		{
			// Arrange
			var generator = TreeCreatorFactory.Create()
					.ExcludeExtensions(".txt", ".json");

			var tempDir = CreateTempDirectory();

			try
			{
				// Create files with different extensions
				var txtFile = Path.Combine(tempDir, "file1.txt");
				var jsonFile = Path.Combine(tempDir, "file2.json");
				var csFile = Path.Combine(tempDir, "file3.cs");

				File.WriteAllText(txtFile, "test");
				File.WriteAllText(jsonFile, "{}");
				File.WriteAllText(csFile, "class {}");

				// Act
				var result = generator.Generate(tempDir);

				// Assert
				Assert.NotNull(result);
				Assert.NotNull(result.Root);

				// Should only have the .cs file (txt and json excluded)
				Assert.Single(result.Root.Children);
				Assert.Equal("file3.cs", result.Root.Children[0].Name);

				// Check the string representation
				string stringResult = result.ToString();
				Assert.DoesNotContain("file1.txt", stringResult);
				Assert.DoesNotContain("file2.json", stringResult);
				Assert.Contains("file3.cs", stringResult);
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void Generate_CaseInsensitiveFiltering_WorksCorrectly()
		{
			// Arrange
			var generator = TreeCreatorFactory.Create()
					.ExcludeDirectories("DIR1")  // Using uppercase
					.ExcludeExtensions(".TXT");  // Using uppercase

			var tempDir = CreateTempDirectory();

			try
			{
				// Create directories and files with lowercase names
				var subDir1 = Path.Combine(tempDir, "dir1");
				var subDir2 = Path.Combine(tempDir, "dir2");
				var txtFile = Path.Combine(tempDir, "file.txt");
				var jsonFile = Path.Combine(tempDir, "file.json");

				Directory.CreateDirectory(subDir1);
				Directory.CreateDirectory(subDir2);
				File.WriteAllText(txtFile, "test");
				File.WriteAllText(jsonFile, "{}");

				// Act
				var result = generator.Generate(tempDir);

				// Assert
				Assert.NotNull(result);

				// Check the string representation
				string stringResult = result.ToString();
				Assert.DoesNotContain("dir1", stringResult); // Should be excluded despite case difference
				Assert.Contains("dir2", stringResult);
				Assert.DoesNotContain("file.txt", stringResult); // Should be excluded despite case difference
				Assert.Contains("file.json", stringResult);
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void TreeResult_CreateOrGetItem_CreatesAndRetrievesItems()
		{
			// Arrange
			var tempDir = CreateTempDirectory();

			try
			{
				var result = new TreeResult(tempDir, true);

				// Act - Create new items
				var dirPath = Path.Combine(tempDir, "dir");
				var filePath = Path.Combine(dirPath, "file.txt");

				var dirItem = result.CreateOrGetItem(dirPath, true, true);
				var fileItem = result.CreateOrGetItem(filePath, false, false);

				// Get existing items
				var existingDirItem = result.CreateOrGetItem(dirPath, true, true);
				var existingFileItem = result.CreateOrGetItem(filePath, false, false);

				// Assert
				Assert.NotNull(dirItem);
				Assert.NotNull(fileItem);

				Assert.Equal("dir", dirItem.Name);
				Assert.Equal("file.txt", fileItem.Name);

				// Should be the same instances
				Assert.Same(dirItem, existingDirItem);
				Assert.Same(fileItem, existingFileItem);

				// Check parent-child relationships
				Assert.Contains(result.Root.Children, c => c.Name == "dir");
				Assert.Contains(dirItem.Children, c => c.Name == "file.txt");
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}

		[Fact]
		public void Generate_WithSpecialCharactersInNames_HandlesCorrectly()
		{
			// Arrange
			var generator = TreeCreatorFactory.Create();
			var tempDir = CreateTempDirectory();

			try
			{
				// Create files and directories with special characters
				var specialChars = new[]
				{
										"folder with spaces",
										"folder-with-hyphens",
										"folder_with_underscores",
										"folder.with.dots"
								};

				foreach (var name in specialChars)
				{
					var path = Path.Combine(tempDir, name);
					Directory.CreateDirectory(path);
					File.WriteAllText(Path.Combine(path, $"{name}-file.txt"), "test");
				}

				// Act
				var result = generator.Generate(tempDir);

				// Assert
				Assert.NotNull(result);

				// Check that all the special names appear in the output
				string stringResult = result.ToString();
				foreach (var name in specialChars)
				{
					Assert.Contains(name, stringResult);
					Assert.Contains($"{name}-file.txt", stringResult);
				}
			}
			finally
			{
				CleanupTempDirectory(tempDir);
			}
		}
	}
}