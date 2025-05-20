namespace TreeCreator.App
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Directory Tree Generator");
			Console.WriteLine("------------------------");

			string directoryPath = GetDirectoryPath(args);
			if (string.IsNullOrWhiteSpace(directoryPath))
			{
				Console.WriteLine("Error: Directory path cannot be empty.");
				return;
			}

			if (!Directory.Exists(directoryPath))
			{
				Console.WriteLine($"Error: Directory '{directoryPath}' does not exist.");
				return;
			}

			try
			{
				Console.WriteLine("Select filtering mode:");
				Console.WriteLine("1. Default with common exclusions");
				Console.WriteLine("2. Include only specific directories");
				Console.WriteLine("3. Custom filtering");

				Console.Write("Your choice (1-3): ");
				if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 3)
				{
					Console.WriteLine("Invalid choice. Using default filtering.");
					choice = 1;
				}

				var treeCreator = TreeCreatorFactory.Create();

				switch (choice)
				{
					case 1:
						// Default filtering with common exclusions
						treeCreator
								.ExcludeDirectories("bin", "obj", ".vs", "packages", "node_modules")
								.ExcludeExtensions(".dll", ".exe", ".pdb", ".cache", ".suo", ".user");
						Console.WriteLine("\nUsing default exclusions");
						break;

					case 2:
						// Include only specific directories demonstration
						Console.WriteLine("\nEnter directory names to include (comma-separated):");
						string[] dirsToInclude = Console.ReadLine().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

						treeCreator.IncludeOnlyDirectories(dirsToInclude);
						Console.WriteLine($"\nIncluding only: {string.Join(", ", dirsToInclude)}");
						break;

					case 3:
						// Custom filtering
						Console.WriteLine("\nUse directory inclusions? (y/n):");
						bool useInclusions = Console.ReadLine().Trim().ToLower() == "y";

						if (useInclusions)
						{
							Console.WriteLine("Enter directory names to include (comma-separated):");
							string[] includeDirs = Console.ReadLine().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
							treeCreator.IncludeOnlyDirectories(includeDirs);
							Console.WriteLine($"Including only: {string.Join(", ", includeDirs)}");
						}
						else
						{
							Console.WriteLine("Enter directory names to exclude (comma-separated):");
							string[] excludeDirs = Console.ReadLine().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
							treeCreator.ExcludeDirectories(excludeDirs);
							Console.WriteLine($"Excluding: {string.Join(", ", excludeDirs)}");
						}

						Console.WriteLine("\nFilter by extensions? (y/n):");
						bool filterExtensions = Console.ReadLine().Trim().ToLower() == "y";

						if (filterExtensions)
						{
							Console.WriteLine("Include only specific extensions? (y/n):");
							bool useExtInclusions = Console.ReadLine().Trim().ToLower() == "y";

							if (useExtInclusions)
							{
								Console.WriteLine("Enter extensions to include (comma-separated, with or without dots):");
								string[] includeExts = Console.ReadLine().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
								treeCreator.IncludeOnlyExtensions(includeExts);
								Console.WriteLine($"Including only extensions: {string.Join(", ", includeExts)}");
							}
							else
							{
								Console.WriteLine("Enter extensions to exclude (comma-separated, with or without dots):");
								string[] excludeExts = Console.ReadLine().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
								treeCreator.ExcludeExtensions(excludeExts);
								Console.WriteLine($"Excluding extensions: {string.Join(", ", excludeExts)}");
							}
						}
						break;
				}

				// Generate the tree and display it
				var tree = treeCreator.Generate(directoryPath);

				Console.WriteLine("\nDirectory Tree:");
				Console.WriteLine("---------------");
				Console.WriteLine(tree.ToString());

				Console.WriteLine("\nPress any key to exit...");
				Console.ReadKey();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error generating tree: {ex.Message}");
			}
		}

		private static string GetDirectoryPath(string[] args)
		{
			if (args.Length > 0)
				return args[0];

			Console.Write("Enter directory path: ");
			return Console.ReadLine()!;
		}
	}
}