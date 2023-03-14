using Beamable.Server;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Beamable.Spoons
{
	public class Program
	{
		/// <summary>
		/// The entry point for the <see cref="Spoons"/> service.
		/// </summary>
		public static async Task Main()
		{
			// Put your code in the Spoons class
			
			// load environment variables from local file
			LoadEnvironmentVariables();
			
			// run the Microservice code
			await MicroserviceBootstrapper.Start<Spoons>();
		}
		
		static void LoadEnvironmentVariables(string filePath=".env")
		{
			if (!File.Exists(filePath))
			{
				throw new Exception($"No environment file found at path=[{filePath}] curr=[{Environment.CurrentDirectory}]");
			}

			foreach (var line in File.ReadAllLines(filePath))
			{
				var parts = line.Split(
					'=',
					StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length != 2)
					continue;

				Environment.SetEnvironmentVariable(parts[0], parts[1]);
			}
		}
	}
}
