﻿// Conventions that should be followed throughout the codebase, based off of existing conventions. Feel free to suggest changes
// Most of these conventions from MS docs should be followed: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
//     Exceptions and highlights are outlined below
//
// PascalCase for classes/structs/records and functions, delegates, namespaces and public/internal member variables as well as properties
// camelCase for private member variables, method parameters, local variables
//     Prefix private member variables with an underscore e.g. `_privateMemberVar`
// Prefix interfaces with I, e.g. IFileType
// Explicit access modifiers for everything
//     Everything is `internal` unless it is `private` (although for overridden methods they may have to be public)
// Use `var` for declaring variables only when the type is obvious
// Space after the `//` and `///` of comments (before the actual comment text)
// Indent using TABS, align using SPACES (Indent in comments with spaces too)
// Braces on the same line, e.g. // TODO Revisit, cause this is not usual C# style and therefore not what editors default to but I vastly prefer it
//
//     if(flag) {
//         // Code
//     } else {
//         // Code
//     }
//
// Instead of
//
//     if(flag)
//     {
//         // Code
//     }
//
// Also no space between keywords and any arguments in brackets after them e.e. `if(flag)` not `if (flag)` and `switch(thing)` not `switch (thing)`
//
// For notes on writing performant C# and for C# resources: https://willbanksy-pkb.notion.site/C-edef060a627f4f2babe13346a11e5962

using System.Diagnostics;
using HoneyScoop.FileHandling;
using HoneyScoop.FileHandling.FileTypes;
using HoneyScoop.Searching;
using HoneyScoop.Searching.RegexImpl;
using HoneyScoop.Util;

namespace HoneyScoop;

internal static class MainClass {
	/// <summary>
	/// The entry point into the program. Handles program arguments, and initialises state to perform work depending on the arguments
	///
	/// Also using the Main function for testing rn (probably need a better solution... Unit Test project?)
	/// </summary>
	/// <param name="args"></param>
	public static void Main(string[] args) {
		// Taking in Command line arguments
		// Works only after running ParseArgs, which sets the CLI arguments as required
		
		CommandLineArguments argParser = new CommandLineArguments();
		List<string> specifiedFileTypes = argParser.ParseArgs(args);

		HoneyScoop controller = HoneyScoop.Instance();
		controller.Initialise(argParser, specifiedFileTypes);
		
		Console.WriteLine(controller.ToString());
		// Accessible arguments:
		// Pattern: TakenArguments.COMMAND_LINE_ARGUMENT
		// TakenArguments.OutputDirectory String path, which is the place the directories, files should be made, current directory path by default
		// TakenArguments.NumThreads Integer number of threads to be used 40 by default
		// TakenArguments.Verbose Boolean if everything should be in CL, false by default
		// TakenArguments.QuietMode Boolean if no CL output wanted, false by default
		// TakenArguments.Timestamp Boolean if the output directories are to be timestamped, false by default
		// TakenArguments.NoOrganise Boolean if organising by filetype is not needed, false by default(or organised by default)
		// DefinedArguments a List of the filetypes needed to search for.
		// TakenArguments.InputFile a String of a path for the supplied file which the reconstruction will work on.
		
		DoTesting();
	}

	static void DoTesting() {
		byte[] testNfaData = {
			0x00, 0x00, 0x01, 0x7E
		};
		var matcher = new RegexMatcher(@"\xff");
		var matches = matcher.Advance(testNfaData);
		Console.Write("[");
		foreach(var m in matches) {
			Console.Write($"{m}, ");
		}
		Console.WriteLine("]");
		// Outputs: [2, ]

		byte[] testCrc32Data = {
			0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x0E, 0x00, 0x00, 0x00, 0x16, 0x04, 0x03, 0x00, 0x00, 
			0x00
		};
		uint expectedCrc32 = 0x02E3B014;
		uint crc = Helper.Crc32(testCrc32Data);
		Debug.Assert(crc == expectedCrc32, $"Calculated CRC does not match the actual CRC. The calculated CRC was: {Convert.ToString(crc, 16).PadLeft(8, '0')}");

		byte[] testPngData = GetPngTestData();
		FileTypePng png = new FileTypePng();
		AnalysisResult result = png.Analyse(testPngData);
		Debug.Assert(result == AnalysisResult.Correct, "The incorrect analysis result was computed");

		var infix = @"((\x0a\x0b*)|\x0c?)+\x0d\x0e\x0f";
		var expectedPostfix = @"\x0a\x0b*'\x0c?|+\x0d'\x0e'\x0f'";
		Console.WriteLine($"Infix: {infix}");
		string postfix = RegexLexer.Token.TokensToString(RegexEngine.ParseToPostfix(infix));
		Console.WriteLine($"Postfix: {postfix}");
		Debug.Assert(postfix.Equals(expectedPostfix), "Test Failed: Infix regex was not converted to correct postfix expression");
		
		var regex = @"\x0a\x0b";

		StateTransitionTable stt = StateTransitionTable.Build(RegexEngine.ParseToPostfix(regex));
		Console.WriteLine($"stt({stt.ToString()})");

		var expected = new FiniteStateMachine<byte>(0x0a);
		FiniteStateMachine<byte> got = RegexEngine.ParseRegex(regex);
		Debug.Assert(got.Equals(expected), "Test Failed: Postfix regex was not converted into a Finite State Machine/NFA correctly (or in this case the NFA comparison is broken while I work on a solution)");
	}

	static byte[] GetPngTestData() { // Function for testing FileTypePng analysis
		return new byte[] {
		    0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 
		    0x00, 0x00, 0x00, 0x0E, 0x00, 0x00, 0x00, 0x16, 0x04, 0x03, 0x00, 0x00, 0x00, 0x02, 0xE3, 0xB0, 
		    0x14, 0x00, 0x00, 0x00, 0x04, 0x67, 0x41, 0x4D, 0x41, 0x00, 0x00, 0xB1, 0x8F, 0x0B, 0xFC, 0x61, 
		    0x05, 0x00, 0x00, 0x00, 0x20, 0x63, 0x48, 0x52, 0x4D, 0x00, 0x00, 0x7A, 0x26, 0x00, 0x00, 0x80, 
		    0x84, 0x00, 0x00, 0xFA, 0x00, 0x00, 0x00, 0x80, 0xE8, 0x00, 0x00, 0x75, 0x30, 0x00, 0x00, 0xEA, 
		    0x60, 0x00, 0x00, 0x3A, 0x98, 0x00, 0x00, 0x17, 0x70, 0x9C, 0xBA, 0x51, 0x3C, 0x00, 0x00, 0x00, 
		    0x2A, 0x50, 0x4C, 0x54, 0x45, 0x00, 0x00, 0x00, 0x18, 0x10, 0x29, 0x22, 0x17, 0x3C, 0x3F, 0x27, 
		    0x69, 0x8B, 0x6D, 0xBF, 0x3B, 0x2A, 0x13, 0x73, 0x45, 0x0C, 0x1B, 0x16, 0x27, 0xB3, 0x50, 0x1B, 
		    0xFE, 0xC2, 0x14, 0xFE, 0xF6, 0x25, 0x50, 0x31, 0x85, 0xD4, 0x83, 0x0B, 0xFF, 0xFF, 0xFF, 0x3A, 
		    0xBF, 0xB3, 0x96, 0x00, 0x00, 0x00, 0x01, 0x74, 0x52, 0x4E, 0x53, 0x00, 0x40, 0xE6, 0xD8, 0x66, 
		    0x00, 0x00, 0x00, 0x01, 0x62, 0x4B, 0x47, 0x44, 0x0D, 0xF6, 0xB4, 0x61, 0xF5, 0x00, 0x00, 0x00, 
		    0x07, 0x74, 0x49, 0x4D, 0x45, 0x07, 0xE5, 0x02, 0x10, 0x00, 0x16, 0x35, 0xEA, 0xAF, 0x55, 0x9A, 
		    0x00, 0x00, 0x00, 0x45, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0x60, 0x60, 0x60, 0x10, 0x54, 
		    0x62, 0x00, 0x03, 0x28, 0x2D, 0x68, 0xEC, 0xA2, 0x84, 0x4C, 0x87, 0xA6, 0xA5, 0x19, 0x97, 0x23, 
		    0xE8, 0xD0, 0x8E, 0x99, 0xAB, 0xD2, 0x18, 0xE0, 0xB4, 0xA0, 0xB1, 0x92, 0xF1, 0x6E, 0x63, 0x25, 
		    0x38, 0x1D, 0x7A, 0x06, 0x28, 0x3E, 0x33, 0x0D, 0x4E, 0x83, 0xC5, 0x77, 0x2B, 0xC1, 0x69, 0x42, 
		    0xF2, 0xE8, 0xE6, 0x83, 0xED, 0x65, 0x80, 0xD3, 0x00, 0x7C, 0x73, 0x2D, 0x6D, 0x1D, 0x56, 0x9E, 
		    0x16, 0x00, 0x00, 0x00, 0x25, 0x74, 0x45, 0x58, 0x74, 0x64, 0x61, 0x74, 0x65, 0x3A, 0x63, 0x72, 
		    0x65, 0x61, 0x74, 0x65, 0x00, 0x32, 0x30, 0x32, 0x31, 0x2D, 0x30, 0x32, 0x2D, 0x31, 0x36, 0x54, 
		    0x30, 0x30, 0x3A, 0x32, 0x32, 0x3A, 0x35, 0x33, 0x2B, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x9E, 0xA6, 
		    0xDB, 0x61, 0x00, 0x00, 0x00, 0x25, 0x74, 0x45, 0x58, 0x74, 0x64, 0x61, 0x74, 0x65, 0x3A, 0x6D, 
		    0x6F, 0x64, 0x69, 0x66, 0x79, 0x00, 0x32, 0x30, 0x32, 0x31, 0x2D, 0x30, 0x32, 0x2D, 0x31, 0x36, 
		    0x54, 0x30, 0x30, 0x3A, 0x32, 0x32, 0x3A, 0x35, 0x33, 0x2B, 0x30, 0x30, 0x3A, 0x30, 0x30, 0xEF, 
		    0xFB, 0x63, 0xDD, 0x00, 0x00, 0x00, 0x20, 0x74, 0x45, 0x58, 0x74, 0x73, 0x6F, 0x66, 0x74, 0x77, 
		    0x61, 0x72, 0x65, 0x00, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x69, 0x6D, 0x61, 0x67, 
		    0x65, 0x6D, 0x61, 0x67, 0x69, 0x63, 0x6B, 0x2E, 0x6F, 0x72, 0x67, 0xBC, 0xCF, 0x1D, 0x9D, 0x00, 
		    0x00, 0x00, 0x18, 0x74, 0x45, 0x58, 0x74, 0x54, 0x68, 0x75, 0x6D, 0x62, 0x3A, 0x3A, 0x44, 0x6F, 
		    0x63, 0x75, 0x6D, 0x65, 0x6E, 0x74, 0x3A, 0x3A, 0x50, 0x61, 0x67, 0x65, 0x73, 0x00, 0x31, 0xA7, 
		    0xFF, 0xBB, 0x2F, 0x00, 0x00, 0x00, 0x17, 0x74, 0x45, 0x58, 0x74, 0x54, 0x68, 0x75, 0x6D, 0x62, 
		    0x3A, 0x3A, 0x49, 0x6D, 0x61, 0x67, 0x65, 0x3A, 0x3A, 0x48, 0x65, 0x69, 0x67, 0x68, 0x74, 0x00, 
		    0x32, 0x32, 0x31, 0xEF, 0xC9, 0xB5, 0x00, 0x00, 0x00, 0x16, 0x74, 0x45, 0x58, 0x74, 0x54, 0x68, 
		    0x75, 0x6D, 0x62, 0x3A, 0x3A, 0x49, 0x6D, 0x61, 0x67, 0x65, 0x3A, 0x3A, 0x57, 0x69, 0x64, 0x74, 
		    0x68, 0x00, 0x31, 0x34, 0x0B, 0x0E, 0xFF, 0xCE, 0x00, 0x00, 0x00, 0x19, 0x74, 0x45, 0x58, 0x74, 
		    0x54, 0x68, 0x75, 0x6D, 0x62, 0x3A, 0x3A, 0x4D, 0x69, 0x6D, 0x65, 0x74, 0x79, 0x70, 0x65, 0x00, 
		    0x69, 0x6D, 0x61, 0x67, 0x65, 0x2F, 0x70, 0x6E, 0x67, 0x3F, 0xB2, 0x56, 0x4E, 0x00, 0x00, 0x00, 
		    0x17, 0x74, 0x45, 0x58, 0x74, 0x54, 0x68, 0x75, 0x6D, 0x62, 0x3A, 0x3A, 0x4D, 0x54, 0x69, 0x6D, 
		    0x65, 0x00, 0x31, 0x36, 0x31, 0x33, 0x34, 0x33, 0x34, 0x39, 0x37, 0x33, 0x75, 0x87, 0x36, 0x03, 
		    0x00, 0x00, 0x00, 0x10, 0x74, 0x45, 0x58, 0x74, 0x54, 0x68, 0x75, 0x6D, 0x62, 0x3A, 0x3A, 0x53, 
		    0x69, 0x7A, 0x65, 0x00, 0x31, 0x39, 0x30, 0x42, 0x1E, 0xE9, 0x38, 0xFC, 0x00, 0x00, 0x00, 0x35, 
		    0x74, 0x45, 0x58, 0x74, 0x54, 0x68, 0x75, 0x6D, 0x62, 0x3A, 0x3A, 0x55, 0x52, 0x49, 0x00, 0x66, 
		    0x69, 0x6C, 0x65, 0x3A, 0x2F, 0x2F, 0x2F, 0x74, 0x6D, 0x70, 0x2F, 0x74, 0x68, 0x75, 0x6D, 0x62, 
		    0x6C, 0x72, 0x2F, 0x69, 0x6D, 0x67, 0x39, 0x31, 0x37, 0x36, 0x37, 0x39, 0x36, 0x37, 0x31, 0x39, 
		    0x37, 0x35, 0x32, 0x30, 0x39, 0x34, 0x33, 0x36, 0x38, 0x9D, 0xE3, 0xC8, 0xB0, 0x00, 0x00, 0x00, 
		    0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
		};
	}
}
