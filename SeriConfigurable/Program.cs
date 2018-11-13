using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Serilog;
using SerilogExtensions;
using Serilog.Sinks;

namespace SeriConfigurable {

	public class SomeClass {
		private static Serilog.ILogger _log = Serilog.Log.Logger.ForMyContext<SomeClass>();

		public SomeClass() {
			_log.Debug("Constructed");
		}

		public virtual void Foo() {
			_log.Verbose("Doing Verbose Stuff");
			_log.Information("Doing Information Stuff");
			_log.Debug("Doing Debug Stuff");
			_log.Warning("Doing Warning Stuff");
			_log.Error("Doing Error Stuff");
			_log.Fatal("Doing Fatal Stuff");

			var dummyData = new byte[] { 0x01, 0x03, 0xFF, 0x6E, 0xFF };
			StringBuilder hex = new StringBuilder(dummyData.Length * 6);
			foreach (byte oneByte in dummyData)
				hex.AppendFormat("0x{0:x2}, ", oneByte);

			_log.Verbose(string.Format("Received {0} bytes of data: {1}", dummyData.Length, hex.ToString()));
		}
	}

	public class OtherClass {
		private static Serilog.ILogger _log = Serilog.Log.Logger.ForMyContext<OtherClass>();

		public OtherClass() {
			_log.Debug("Constructed");
		}

		public void Foo() {
			_log.Verbose("Doing Verbose Stuff");
			_log.Information("Doing Information Stuff");
			_log.Debug("Doing Debug Stuff");
			_log.Warning("Doing Warning Stuff");
			_log.Error("Doing Error Stuff");
			_log.Fatal("Doing Fatal Stuff");
		}
	}

	public class DerivedClass : SomeClass {
		private static Serilog.ILogger _log = Serilog.Log.Logger.ForMyContextWithTag<DerivedClass>("Poop");

		public DerivedClass() {
			_log.Debug("Constructed");
		}

		public override void Foo() {

			_log.Verbose("Doing Verbose Stuff");
			_log.Information("Doing Information Stuff");
			_log.Debug("Doing Debug Stuff");
			_log.Warning("Doing Warning Stuff");
			_log.Error("Doing Error Stuff");
			_log.Fatal("Doing Fatal Stuff");

			try {
				MakeExceptions();
			}
			catch(Exception e) {
				_log.Error(e, "Bad Things");
			}
		}

		public void MakeExceptions() {
			var inner = new BadImageFormatException("You made us look at x86 things");
			var e = new ArgumentException("All of your arguments are bad. You skipped philosophy class", inner);
			throw e;
		}
	}

	class Program {
		static void Main(string[] args) {

			MyOptions.SetOptionsPath(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

			var someClass = new SomeClass();
			someClass.Foo();

			var otherClass = new OtherClass();
			otherClass.Foo();

			var derived = new DerivedClass();
			derived.Foo();
		}
	}
}
