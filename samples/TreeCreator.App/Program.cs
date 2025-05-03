using TreeCreator;

namespace TreeCreator.App;
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
			var tree = TreeCreatorFactory.Create()
				.ExcludeDirectories("bin", "obj", "Debug", "Release", ".vs", "packages", "node_modules")
				.ExcludeExtensions(".dll", ".exe", ".pdb", ".cache", ".suo", ".user", ".baml", ".resources")
				.Generate(directoryPath);

			Console.WriteLine("\nDirectory Tree:");
			Console.WriteLine("---------------");
			Console.WriteLine(tree.ToString());

			Console.WriteLine("\nNote: Excluded directories: bin, obj, Debug, Release, .vs, packages, node_modules");
			Console.WriteLine("      Excluded extensions: .dll, .exe, .pdb, .cache, .suo, .user, .baml, .resources");

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