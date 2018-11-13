using System;
using System.IO;

using Newtonsoft.Json;
using Serilog;
using SerilogExtensions;

namespace SeriConfigurable {
	public static class MyOptions {
		private static readonly object __lock = new object();
		private static FileSystemWatcher __logLevelWatcher;

		/// <summary>
		/// Allows us to configure Serilog from option in a file
		/// </summary>
		/// <param name="file"></param>
		private static void ReadLogLevel(string file) {

			LogConfiguration configuration = null;
			if (!File.Exists(file)) {
				configuration = new LogConfiguration();
				var jsonAsText = JsonConvert.SerializeObject(configuration);

				using (StreamWriter writer = new StreamWriter(file)) {
					writer.Write(jsonAsText);
				}
			}
			else {
				using (StreamReader reader = new StreamReader(file)) {
					var jsonAsText = reader.ReadToEnd();
					configuration = JsonConvert.DeserializeObject<LogConfiguration>(jsonAsText);
				}
			}

			configuration.ConfigureSerilog();
		}

		public static void SetOptionsPath(string path) {
			lock (__lock) {
				string logLevelFile = Path.Combine(path, "logLevel");
				
				ReadLogLevel(logLevelFile);

				if (__logLevelWatcher != null) {
					__logLevelWatcher.EnableRaisingEvents = false;
					__logLevelWatcher.Dispose();
				}

				__logLevelWatcher = new FileSystemWatcher {
					Path = Path.GetDirectoryName(logLevelFile),
					Filter = Path.GetFileName(logLevelFile),
					NotifyFilter = NotifyFilters.LastWrite
				};

				__logLevelWatcher.Changed += (sender, e) => { ReadLogLevel(e.FullPath); };
				__logLevelWatcher.EnableRaisingEvents = true;
			}
		}
	}
}
