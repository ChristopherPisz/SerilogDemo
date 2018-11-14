using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace SerilogExtensions {

	public static class SerilogForContextExtension {

		/// <summary>
		/// Extension method that adds the class name to the source context associated with the logger interface
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="that"></param>
		/// <returns></returns>
		public static ILogger ForMyContext<T>(this ILogger that) =>
			that.ForContext(Constants.SourceContextPropertyName, typeof(T).Name);

		public static ILogger ForMyContextWithTag<T>(this ILogger that, string tag) =>
			that.ForContext(Constants.SourceContextPropertyName, typeof(T).Name).ForContext("Tag", tag);

		/// <summary>
		/// Extension method that adds the class name to the source context associated with the logger interface
		/// For use with static classes
		/// </summary>
		/// <param name="that"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static ILogger ForMyContextWithExplicitType(this ILogger that, Type t) =>
			that.ForContext(Constants.SourceContextPropertyName, t.Name);
	}

	/// <summary>
	/// POD Class, for serialization to and from file, that contains configurable option we will pass to Serilog
	/// </summary>
	public class LogConfiguration {

		public LogConfiguration() {
			DefaultLevel = LogEventLevel.Verbose;
			Enabled = new List<string>();
			Disabled = new List<string>();
			LogLevelsBySource = new Dictionary<string, LogEventLevel>() { {"SomeClass", 0 }, { "OtherClass", 0 }  };
			OutputTemplate = "[{ThreadId} {Level:u3} {SourceContext} {Tag}] {Message:lj}{NewLine}{Exception}";
		}

		/// <summary>
		/// The default logging level
		/// </summary>
		public LogEventLevel DefaultLevel { get; set; }

		/// <summary>
		/// Enable logging by source context class name
		/// </summary>
		public List<string> Enabled { get; set; }

		/// <summary>
		/// Disable logging by source context class name
		/// </summary>
		public List<string> Disabled { get; set; }

		/// <summary>
		/// Configure logging level by source context class name
		/// </summary>
		public Dictionary<string, LogEventLevel> LogLevelsBySource;

		/// <summary>
		/// Determines what each log message will look like. 
		/// Uses Serilog's rules
		/// </summary>
		public string OutputTemplate { get; set; }

		/// <summary>
		/// Overides any previous configuration Serilog is using with one dictated by this class' state
		/// </summary>
		public void ConfigureSerilog() {
			
			var configuration = new LoggerConfiguration()
				.MinimumLevel.ControlledBy(new Serilog.Core.LoggingLevelSwitch(DefaultLevel))
				.Enrich.WithThreadId()
				.Enrich.FromLogContext()
				.WriteTo.TextWriter(Console.Out, outputTemplate: OutputTemplate);

			var filterExpression = new StringBuilder();

			if(Enabled.Count > 0) {

				filterExpression.Append($"@Properties['{Serilog.Core.Constants.SourceContextPropertyName}'] in ['{Enabled[0]}'");
				for(int index = 1; index < Enabled.Count; ++index) {
					filterExpression.Append($",'{Enabled[index]}'");
				}
				filterExpression.Append("]");

				configuration.Filter.ByIncludingOnly(filterExpression.ToString());
			}
			else if(Disabled.Count > 0) {

				filterExpression.Append($"@Properties['{Serilog.Core.Constants.SourceContextPropertyName}'] in ['{Disabled[0]}'");
				for (int index = 1; index < Disabled.Count; ++index) {
					filterExpression.Append($",'{Disabled[index]}'");
				}
				filterExpression.Append("]");

				configuration.Filter.ByExcluding(filterExpression.ToString());
			}

			foreach(var logLevelForSource in LogLevelsBySource) {
				configuration.MinimumLevel.Override(logLevelForSource.Key, logLevelForSource.Value);
			}

			Log.Logger = configuration.CreateLogger();
		}
	}
}
