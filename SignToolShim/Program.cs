using System;
using System.Diagnostics;

namespace SignToolShim
{
	internal class Program
	{
		static int Main(string[] args)
		{
			if (args.Length != 3 || !args[0].Equals("sign", StringComparison.OrdinalIgnoreCase) || !args[1].Equals("shim", StringComparison.OrdinalIgnoreCase))
			{
				Console.Error.WriteLine("Error: Only 'sign shim' command is supported");
				return 1;
			}

			var filePath = args[2];
			if (!filePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine($"[SignToolShim] Skipping non-exe file: {filePath}");
				return 0;
			}

			if (!HasSigningConfiguration())
			{
				Console.WriteLine($"[SignToolShim] Skipping signing because DigiCert Software Trust Manager environment variables are not configured: {filePath}");
				return 0;
			}

			var smctlArgs = $"sign --simple --keypair-alias=key_1409653344 --input=\"{filePath}\"";
			var processInfo = new ProcessStartInfo
			{
				FileName = "smctl.exe",
				Arguments = smctlArgs,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			};

			Console.WriteLine($"[SignToolShim] Signing {filePath}");
			using (var process = Process.Start(processInfo))
			{
				Console.Write(process.StandardOutput.ReadToEnd());
				Console.Error.Write(process.StandardError.ReadToEnd());
				process.WaitForExit();
				return process.ExitCode;
			}
		}

		private static bool HasSigningConfiguration()
		{
			return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SM_HOST"))
				&& !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SM_API_KEY"))
				&& !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SM_CLIENT_CERT_FILE"))
				&& !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SM_CLIENT_CERT_PASSWORD"));
		}
	}
}
