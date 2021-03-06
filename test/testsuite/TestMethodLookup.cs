using System;
using System.Text;
using NUnit.Framework;

using Mono.Debugger;
using Mono.Debugger.Languages;
using Mono.Debugger.Frontend;
using Mono.Debugger.Test.Framework;

namespace Mono.Debugger.Tests
{
	[DebuggerTestFixture]
	public class TestMethodLookup : DebuggerTestFixture
	{
		public TestMethodLookup ()
			: base ("TestMethodLookup")
		{ }

		const int LineMain = 250;
		const int LineWriteLine = 253;

		int BreakpointWriteLine;
		StringBuilder Failures = new StringBuilder ();
		int CountFailures;

		protected void AssertListAndBreak (string method)
		{
			try {
				AssertExecute ("list " + method);
				int bpt = AssertBreakpoint (method);
				AssertExecute ("delete " + bpt);
			} catch (AssertionException ex) {
				++CountFailures;
				Failures.Append (ex.Message + "\n");
			}
		}

		protected void AssertListAndBreakAmbiguous (string method)
		{
			string message = "Ambiguous method `" + method + "'; need to use full name";
			AssertExecuteException ("list " + method, message);
			AssertExecuteException ("break " + method, message);
		}

		protected void AssertListAndBreakAmbiguous (string method, string name)
		{
			string message = "Ambiguous method `" + name + "'; need to use full name";
			AssertExecuteException ("list " + method, message);
			AssertExecuteException ("break " + method, message);
		}

		[Test]
		[Category("ManagedTypes")]
		public void Main ()
		{
			Process process = Start ();
			Assert.IsTrue (process.IsManaged);
			Assert.IsTrue (process.MainThread.IsStopped);
			Thread thread = process.MainThread;

			AssertStopped (thread, "X.Main()", LineMain);

			BreakpointWriteLine = AssertBreakpoint (LineWriteLine);

			AssertExecute ("continue");
			AssertHitBreakpoint (thread, BreakpointWriteLine, "X.Main()", LineWriteLine);

			AssertListAndBreak ("Hello");
			AssertListAndBreak ("X.Hello");
			AssertListAndBreak ("Hello()");
			AssertListAndBreak ("X.Hello()");

			AssertListAndBreak ("Simple");
			AssertListAndBreak ("Simple(Foo.Bar.Test)");

			AssertListAndBreak ("StaticHello");
			AssertListAndBreak ("StaticHello()");

			AssertListAndBreak ("Overloaded()");
			AssertListAndBreak ("Overloaded(int)");
			AssertListAndBreak ("Overloaded(Root)");
			AssertListAndBreak ("Overloaded(Foo.Bar.Test)");

			AssertListAndBreak ("StaticOverloaded()");
			AssertListAndBreak ("StaticOverloaded(int)");
			AssertListAndBreak ("StaticOverloaded(Root)");
			AssertListAndBreak ("StaticOverloaded(Foo.Bar.Test)");

			AssertListAndBreak ("Root.Hello");
			AssertListAndBreak ("Root.Hello()");

			AssertListAndBreak ("Root.Simple");
			AssertListAndBreak ("Root.Simple(Foo.Bar.Test)");

			AssertListAndBreak ("Root.StaticHello");
			AssertListAndBreak ("Root.StaticHello()");

			AssertListAndBreakAmbiguous ("Root.Overloaded");
			AssertListAndBreakAmbiguous ("Root.StaticOverloaded");

			AssertListAndBreak ("Root.Overloaded()");
			AssertListAndBreak ("Root.Overloaded(int)");
			AssertListAndBreak ("Root.Overloaded(Root)");
			AssertListAndBreak ("Root.Overloaded(Foo.Bar.Test)");

			AssertListAndBreak ("Root.StaticOverloaded()");
			AssertListAndBreak ("Root.StaticOverloaded(int)");
			AssertListAndBreak ("Root.StaticOverloaded(Root)");
			AssertListAndBreak ("Root.StaticOverloaded(Foo.Bar.Test)");

			AssertListAndBreak ("Foo.Bar.Test.Hello");
			AssertListAndBreak ("Foo.Bar.Test.Hello()");

			AssertListAndBreak ("Foo.Bar.Test.Simple");
			AssertListAndBreak ("Foo.Bar.Test.Simple(Foo.Bar.Test)");

			AssertListAndBreak ("Foo.Bar.Test.StaticHello");
			AssertListAndBreak ("Foo.Bar.Test.StaticHello()");

			AssertListAndBreakAmbiguous ("Foo.Bar.Test.Overloaded");
			AssertListAndBreakAmbiguous ("Foo.Bar.Test.StaticOverloaded");

			AssertListAndBreak ("Foo.Bar.Test.Overloaded()");
			AssertListAndBreak ("Foo.Bar.Test.Overloaded(int)");
			AssertListAndBreak ("Foo.Bar.Test.Overloaded(Root)");
			AssertListAndBreak ("Foo.Bar.Test.Overloaded(Foo.Bar.Test)");

			AssertListAndBreak ("Foo.Bar.Test.StaticOverloaded()");
			AssertListAndBreak ("Foo.Bar.Test.StaticOverloaded(int)");
			AssertListAndBreak ("Foo.Bar.Test.StaticOverloaded(Root)");
			AssertListAndBreak ("Foo.Bar.Test.StaticOverloaded(Foo.Bar.Test)");

			AssertPrint (thread, "StaticHello()", "(bool) true");
			AssertPrint (thread, "StaticOverloaded()", "(bool) true");
			AssertPrint (thread, "StaticOverloaded(3)", "(bool) true");
			AssertPrint (thread, "StaticOverloaded(root)", "(bool) true");
			AssertPrint (thread, "StaticOverloaded(test)", "(bool) true");

			AssertPrintException (thread, "Hello()",
					      "Cannot invoke instance method `X.Hello()' with a " +
					      "type reference.");
			AssertPrintException (thread, "Simple(test)",
					      "Cannot invoke instance method " +
					      "`X.Simple(Foo.Bar.Test)' with a type reference.");
			AssertPrintException (thread, "Overloaded()",
					      "Cannot invoke instance method " +
					      "`X.Overloaded()' with a type reference.");
			AssertPrintException (thread, "Overloaded(3)",
					      "Cannot invoke instance method " +
					      "`X.Overloaded(int)' with a type reference.");
			AssertPrintException (thread, "Overloaded(root)",
					      "Cannot invoke instance method " +
					      "`X.Overloaded(Root)' with a type reference.");
			AssertPrintException (thread, "Overloaded(test)",
					      "Cannot invoke instance method " +
					      "`X.Overloaded(Foo.Bar.Test)' with a type reference.");

			AssertPrint (thread, "root.Hello()", "(bool) true");
			AssertPrint (thread, "root.Simple(test)", "(bool) true");
			AssertPrint (thread, "root.Overloaded()", "(bool) true");
			AssertPrint (thread, "root.Overloaded(3)", "(bool) true");
			AssertPrint (thread, "root.Overloaded(root)", "(bool) true");
			AssertPrint (thread, "root.Overloaded(test)", "(bool) true");
			AssertPrint (thread, "Root.Overloaded(\"Hello\")", "(bool) true");

			AssertPrintException (thread, "Root.Hello()",
					      "Cannot invoke instance method `Root.Hello()' with a " +
					      "type reference.");
			AssertPrintException (thread, "Root.Simple(test)",
					      "Cannot invoke instance method " +
					      "`Root.Simple(Foo.Bar.Test)' with a type reference.");
			AssertPrintException (thread, "Root.Overloaded()",
					      "Cannot invoke instance method " +
					      "`Root.Overloaded()' with a type reference.");
			AssertPrintException (thread, "Root.Overloaded(3)",
					      "Cannot invoke instance method " +
					      "`Root.Overloaded(int)' with a type reference.");
			AssertPrintException (thread, "Root.Overloaded(root)",
					      "Cannot invoke instance method " +
					      "`Root.Overloaded(Root)' with a type reference.");
			AssertPrintException (thread, "Root.Overloaded(test)",
					      "Cannot invoke instance method " +
					      "`Root.Overloaded(Foo.Bar.Test)' with a type reference.");

			AssertPrint (thread, "Root.StaticHello()", "(bool) true");
			AssertPrint (thread, "Root.StaticOverloaded()", "(bool) true");
			AssertPrint (thread, "Root.StaticOverloaded(3)", "(bool) true");
			AssertPrint (thread, "Root.StaticOverloaded(root)", "(bool) true");
			AssertPrint (thread, "Root.StaticOverloaded(test)", "(bool) true");

			AssertPrint (thread, "test.Hello()", "(bool) true");
			AssertPrint (thread, "test.Simple(test)", "(bool) true");
			AssertPrint (thread, "test.Overloaded()", "(bool) true");
			AssertPrint (thread, "test.Overloaded(3)", "(bool) true");
			AssertPrint (thread, "test.Overloaded(root)", "(bool) true");
			AssertPrint (thread, "test.Overloaded(test)", "(bool) true");
			AssertPrint (thread, "Foo.Bar.Test.Overloaded(\"Hello\")",
				     "(bool) true");

			AssertPrint (thread, "Foo.Bar.Test.StaticHello()",
				     "(bool) true");
			AssertPrint (thread, "Foo.Bar.Test.StaticOverloaded()",
				     "(bool) true");
			AssertPrint (thread, "Foo.Bar.Test.StaticOverloaded(3)",
				     "(bool) true");
			AssertPrint (thread, "Foo.Bar.Test.StaticOverloaded(root)",
				     "(bool) true");
			AssertPrint (thread, "Foo.Bar.Test.StaticOverloaded(test)",
				     "(bool) true");

			AssertPrint (thread, "new Root ()", "(Root) { }");
			AssertPrint (thread, "new Root (3)", "(Root) { }");
			AssertPrint (thread, "new Root (root)", "(Root) { }");
			AssertPrint (thread, "new Root (test)", "(Root) { }");
			AssertPrint (thread, "new Foo.Bar.Test ()", "(Foo.Bar.Test) { }");
			AssertPrint (thread, "new Foo.Bar.Test (3)", "(Foo.Bar.Test) { }");
			AssertPrint (thread, "new Foo.Bar.Test (root)", "(Foo.Bar.Test) { }");
			AssertPrint (thread, "new Foo.Bar.Test (test)", "(Foo.Bar.Test) { }");

			AssertListAndBreak ("-get Property");
			AssertListAndBreak ("-get StaticProperty");
			AssertListAndBreak ("-get Root.Property");
			AssertListAndBreak ("-get Root.StaticProperty");
			AssertListAndBreak ("-get Foo.Bar.Test.Property");
			AssertListAndBreak ("-get Foo.Bar.Test.StaticProperty");

			AssertListAndBreak ("-ctor X");
			AssertListAndBreak ("-ctor X ()");

			AssertListAndBreak ("-ctor Root ()");
			AssertListAndBreak ("-ctor Root (int)");
			AssertListAndBreak ("-ctor Root (Root)");
			AssertListAndBreak ("-ctor Root (Foo.Bar.Test)");

			AssertListAndBreak ("-ctor Foo.Bar.Test ()");
			AssertListAndBreak ("-ctor Foo.Bar.Test (int)");
			AssertListAndBreak ("-ctor Foo.Bar.Test (Root)");
			AssertListAndBreak ("-ctor Foo.Bar.Test (Foo.Bar.Test)");

			// AssertListAndBreakAmbiguous ("-ctor Root", "Root..ctor");
			// AssertListAndBreakAmbiguous ("-ctor Foo.Bar.Test", "Foo.Bar.Test..ctor");

			AssertExecute ("continue");
			AssertTargetOutput ("Root");
			AssertTargetOutput ("Foo.Bar.Test");
			AssertTargetExited (thread.Process);

			if (CountFailures > 0)
				Assert.Fail ("{0} failures:\n{1}", CountFailures, Failures.ToString ());
		}
	}
}
