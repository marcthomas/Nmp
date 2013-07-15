#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using NmpBase;
using NmpExpressions;
//using Nmp.Powershell;

/*

expando object:

	http://blogs.msdn.com/b/csharpfaq/archive/2009/10/01/dynamic-in-c-4-0-introducing-the-expandoobject.aspx


using ExpandoObject to call methods in AST

	http://dlr.codeplex.com/Thread/View.aspx?ThreadId=80123


iron python, DLR expression trees and a bunch of other stuff

	http://msdn.microsoft.com/en-us/magazine/cc163344.aspx#S5


Generate AST for the DLR

	http://www.codeproject.com/KB/codegen/astdlrtest.aspx

	***	Which shows us that you need to reflect over an object to get the
			information required for the DLR to make a method call

			So, if we have to reflect anyway we might as well do the whole
			thing ourselves and eliminate the overhead of the DLR until/unless
			we want to add scripting for a dynamic language


*/

////////////////////////////////////////////////////////////////////////////
// namespace: NmpBase
//
// summary:	.
////////////////////////////////////////////////////////////////////////////

namespace Nmp {

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// 	Argument scanner.
	/// </summary>
	///
	/// <remarks>
	/// 	Jpm, 3/26/2011.
	/// </remarks>
	///
	/// <param name="input">
	/// 	The input.
	/// </param>
	/// <param name="terminalChar">
	/// 	The terminal character.
	/// </param>
	///
	/// <returns>
	/// 	.
	/// </returns>
	////////////////////////////////////////////////////////////////////////////

	delegate NmpStringList ArgumentScanner( IInput input, RecognizedCharType terminalChar );
	delegate void ErrorCallback( string msgFmt, params object [] args );
	

	///////////////////////////////////////////////////////////////////////////
	//
	// Interfaces
	//
	/////////////////////////////////////////////////////////////////////////////

	public interface IHub {

		T Get<T>() where T : class;
	
	}


	/////////////////////////////////////////////////////////////////////////////

	public enum TraceLevels {
		None,
		Error,
		Warning,
		Information,
		Verbose,
		ExtraVerbose,
	}


	/////////////////////////////////////////////////////////////////////////////

	public interface IGrandCentral : IHub {

		CharSequence SeqOpenQuote { get; set; }
		CharSequence SeqCloseQuote { get; set; }

		ExecutionSecurity Security { get; }
		bool RunningUnderAspNet { get; }
		
		IRecognizer Recognizer { get; }

		//TypeHelperDictionary GetTypeHelpers();

		bool EncodeQuotes { get; set; }
		bool FixResults { get; set; }
		bool MacroTraceOn { get; set; }
		TraceLevels MacroTraceLevel { get; set; }
		bool PushbackResult { get; set; }
		bool ExpandAndScan { get; set; }
		int MaxMacroScanDepth { get; set; }
		int MaxRunTime { get; set; }
		int MaxExecTime { get; set; }
		bool BreakNext { get; set; }
		bool AltTokenFmtOnly { get; set; }
		string OpenQuote { get; }
		string CloseQuote { get; }
		void SetQuotes( string open, string close );
		string QuoteWrapString( string str );
		NamedTextBlocks GetTextBlocks();
		EscapedCharList EscapedChars { get; }
		NmpStringList GetSearchPaths();
		NmpStack<string> GetDirectoryStack();
		
		//ExtensionTypeDictionary GetMethodExtensions();

		string FindFile( string name );
		string ReadFile( string fileNameIn, out string filePathOut, Regex regex = null );
		string ReadFile( string fileNameIn );

		string FixText( string [] strs );
		string FixText( string str );

		//NonEscapingParseReader GetNonEscapingParseReader( string text );
		//ParseReader GetParseReader();
		//ParseReader GetParseReader( string text );
		//ParseReader GetParseReader( IBaseReader reader, string context );
		//ParseReader GetParseReader( string text, string sourceFile, string context );
		//MasterParseReader GetMasterParseReader( IBaseReader reader );

		string GetQuotedText( IParseReader input, bool keepQuotes, bool processEscapes );
		string GetQuotedText( IParseReader input, bool keepQuotes );
		string GetQuotedText( StringIndexer input, bool keepQuotes );

	}


	/////////////////////////////////////////////////////////////////////////////

	public interface INotifications {

		void MacroError( string fmt, params object [] args );
		void MacroError( Notifier notifier, string fmt, params object [] args );
		void MacroWarning( string fmt, params object [] args );
		void MacroWarning( Notifier notifier, string fmt, params object [] args );
		void WriteMessage( string fmt, params object [] args );
		void Trace( string fmt, params object [] args );
	}


	/////////////////////////////////////////////////////////////////////////////

	//
	// create an ArgumentList (list of expressions) from list of strings, parent
	// is the parent of each expression added
	//

	internal interface IArgumentsProcessor {

		ArgumentList ProcessArgumentList( Expression parent, NmpStringList strArgs );

	}


	/////////////////////////////////////////////////////////////////////////////

	public delegate object MacroCall( IMacro macro, ArgumentList argList, IMacroArguments macroArgs );

	public interface IMacroHandler {

		// ******
		string Name { get; }

		// ******
		bool HandlesBlocks { get; }
		string ParseBlock( Expression exp, IInput input );

		// ******
		IMacro Create( string name, IMacro macro, object instance, bool clone );
		object Evaluate( IMacro macro, IMacroArguments macroArgs );
	}


	/////////////////////////////////////////////////////////////////////////////

	public interface IReader {

		string	Buffer					{ get; }

		int			Index						{ get; }		
		int			RemainderCount	{ get; }
		string	Remainder				{ get; }

		string	SourceName			{ get; }	// soure file path - may be empty depending upon Context
		int			Line						{ get; }	// gets from current IBaseReader
		int			Column					{ get; }	// gets from current IBaseReader

		string	GetText( int start, int end );

	}


	/////////////////////////////////////////////////////////////////////////////

	public interface IInput : IParseReader, IDisposable {

		// ******
		IReader	Current					{ get; }

		// ******
		bool		PushbackCalled	{ get; }
		string	Context					{ get; }	// context of the call; "file", "macro `name'", etc

		//
		// will likely get from Current (IReader)
		//
		string SourceName { get; }
		int Line { get; }
		int Column { get; }

		// ******
		void	PushBack( string text );

		// ******
		//
		// must NOT be from an external source unless it's been passed
		// through FileReader.FixText()
		//
		void	IncludeText( string text, string includeName );

		// ******
		//void	IncludeFile( string name );
	}


	/////////////////////////////////////////////////////////////////////////////

	internal interface IOutput {

		int						Count						{ get; }
		StringBuilder	Contents				{ get; }
		string				StringContents	{ get; }
		bool					WriteOn					{ get; }

		IOutput	WriteChar( char ch );
		IOutput	Write( string str );
		IOutput	Append( IOutput rhs );
		void		Zero();
	}


	/////////////////////////////////////////////////////////////////////////////

	internal interface ExpressionTreeBuilder {

		NmpStringList	MacroInstructions { get; }
		Expression		ParseExpression( IInput input );

	}


	/////////////////////////////////////////////////////////////////////////////

	internal interface IErrorWarningTrace {

		bool	ErrorReturns		{ get; }

		void	Error( Notifier notifier, string fmt, params object [] args );
		void	Warning( Notifier notifier, string fmt, params object [] args );

		void	WriteMessage( string fmt, params object [] args );
		void	Trace( string fmt, params object [] args );
	}


	/////////////////////////////////////////////////////////////////////////////

	internal interface IScanner : IErrorWarningTrace, IHub {

		IGrandCentral GrandCentral { get; }

		//NmpStringList ParseArguments( string context, IInput input, RecognizedCharType terminalChar );
		NmpStringList ArgScanner( string context, IInput input, RecognizedCharType terminalChar );
		string				Scanner( string textToScan, string sourceContext );
		void					Scanner( IInput input, IOutput output );

	}


	/////////////////////////////////////////////////////////////////////////////

	public enum RecognizedCharType {
		UnknownCharType,
		JustAChar,

		TokenStart,					// generic for TokenStartChar or AltTokenStartChar - used by Next(input, out TokenMap)
		TokenStartChar,
		AltTokenStartChar,
		//PwrShellStartChar,	// $ starts power shell value name

		QuoteStartChar,
		//CloseQuoteStartChar,

		OpenParenChar,
		CloseParenChar,

		OpenBracketChar,
		CloseBracketChar,

		ArgSepChar,
	};


	/////////////////////////////////////////////////////////////////////////////

	public enum RecognizedMacroType {
		None,
		Standard,
		AltFormat,
		RegEx,
		Other,
	}


	/////////////////////////////////////////////////////////////////////////////

	public interface IRecognizer {

		CharSequence OpenQuote					{ get; }
		CharSequence CloseQuote					{ get; }
		CharSequence AltTokenStart			{ get; }

		bool StartsWithAltTokenChars( string str );
		bool IsMacroIdentifierStartChar( char ch );
		bool IsValidMacroIdentifier( string name, bool allowDots = false );

		void		Skip( IInput input, TokenMap tm );
		string	GetText( IInput input, TokenMap tm );

		//RecognizedCharType	Next( IInput input );
		RecognizedCharType	Next( IInput input, out TokenMap tm );

		string	GetMacroName( IInput input, out TokenMap tm );
		string	GetMacroName( IInput input );
	}


	/////////////////////////////////////////////////////////////////////////////

	internal interface IExpressionEvaluator {

		object	Evaluate( IMacro macro, Expression exp );

	}


	///////////////////////////////////////////////////////////////////////////////
	//
	//internal interface IMacroLoader {
	//
	//	//
	//	// this registers the object instance as an object macro
	//	//
	//	string ObjectMacroName		{ get; }
	//
	//	//
	//	// this registers macros on the object instance
	//	//
	//	int LoadMacros( IMacroProcessor mp, bool displayFound );
	//}
	//

	/////////////////////////////////////////////////////////////////////////////

	public interface IMacroContainer {	//: IHub {

		//
		// this is the names the instance should be registered as macros under,
		// note its a array which allows the object macro to be registered under
		// more than one name
		//
		string [] ObjectMacroNames { get; }

		void Initialize( IMacroProcessor mp );

	}


	///////////////////////////////////////////////////////////////////////////////
	//
	//internal interface IMacroCreate {	//: IMacroContainer {
	//
	//}
	//

	/////////////////////////////////////////////////////////////////////////////

	public enum MacroType {
		Builtin,
		Object,
		Text,
	}


	/////////////////////////////////////////////////////////////////////////////

	/// MacroOptions and MacroArguments are in MacroArguments.cs

	[Flags]
	public enum MacroFlags {

		AltTokenFmtOnly	= 1,
		RequiresArgs		= 2,
	
		Pushback				= 4,
		FwdExpand				= 8,	

		NoPushback			= 16,
		NoFwdExpand			= 32,

		NonExpressive		= 64,	// does not handle expressions, only with macro that supports blocks
	}


	/////////////////////////////////////////////////////////////////////////////

	[Flags]
	public enum MacroOptionsFlags {
		None							= 0,

		Pushback					= 2,
		ClearPushback			= 4,

		FwdExpand					= 8,
		ClearFwdExpand		= 16,

		Trim							= 32,
		ClearTrim					= 64,

		NLStrip						= 128,
		ClearNLStrip			= 256,

		Normalize					= 512,
		ClearNormalize		= 1024,

		ILNormalize				= 2048,
		ClearILNormalize	= 4096,

		Quote							= 8192,
		ClearQuote				= 16384,

		TextBlock					= 32768,
		ClearTextBlock		= 65536,

		Data							= 0x20000,
		Format						= 0x40000,

		NoSubst						= 0x80000,

		//							= 0x100000,
	}



	/////////////////////////////////////////////////////////////////////////////

	public interface IMacro {
		IMacro				Pushed							{ get; set; }
		
		string				Name								{ get; }
		MacroType			MacroType						{ get; }
		MacroFlags		Flags								{ get; set; }
		
		IMacroHandler	MacroHandler				{ get; }
		object				MacroHandlerData		{ get; set; }

		object				MacroObject					{ get; set; }
		string				MacroText						{ get; set; }

		bool					IsBlockMacro				{ get; }

		IList<string>	ArgumentNames				{ get; }
		IMacroProcessor	MacroProcessor		{ get; }

		string				SourceFile					{ get; set; }
		int						MacroStartLine			{ get; set; }
	}

	
	/////////////////////////////////////////////////////////////////////////////

	//public enum MacroProcessingState {
	//	None,
	//	Parsing,
	//	Executing,
	//}


	/////////////////////////////////////////////////////////////////////////////

	public interface IMacroInvocationRecord {

		IMacro Macro { get; }
		string Context { get; }	// context of the call; "file", "macro `name'", etc
		bool AltToken { get; }

		IMacroArguments MacroArgs { get; }
		NmpStringList SpecialArgs { get; }

		bool PushbackCalled { get; }
		string SourceName { get; }	// soure file path - may be empty depending upon Context

		int SourceStartIndex { get; }
		int SourceEndIndex { get; }
		int SourceExpandedEndIndex { get; }

		string InitialTextSpan { get; }
		string TextSpan { get; }

		int Line { get; }
		int Column { get; }

		// ******
		bool CalledFromMacro { get; }
		bool CalledFromFile { get; }

		//MacroProcessingState State { get; set; }
	}

	/////////////////////////////////////////////////////////////////////////////

	public interface IMacroOptions {
	
		bool CallerInstructions					{ get; set; }

		bool Pushback										{ get; set; }
		bool FwdExpand									{ get; set; }

		bool Quote											{ get; set; }
		bool Trim												{ get; set; }
		bool NLStrip										{ get; set; }
		bool CompressAllWhiteSpace			{ get; set; }
		bool ILCompressWhiteSpace				{ get; set; }
		bool TextBlockWrap							{ get; set; }
		bool Divert											{ get; set; }
		bool Eval												{ get; set; }

		bool Data												{ get; set; }
		bool Format											{ get; set; }

		bool NoSubst										{ get; set; }

		bool Razor											{ get; set; }
		bool RazorObject								{ get; set; }

		bool Empty { get; set; }

		bool TabsToSpaces { get; set; }

		bool HtmlEncode { get; set; }

		bool Echo { get; set; }

		bool FixResult { get; set; }
		bool EncodeQuotes { get; set; }

		//
		// other options
		//
		bool NoExpression								{ get; set; }

		NmpStringList	AdditionalOptions	{ get; set; }

	}


	/////////////////////////////////////////////////////////////////////////////

	public interface IMacroArguments {

		IInput					Input				{ get; }
		Expression			Expression	{ get; }
		IMacroOptions		Options			{ get; }
		string					BlockText		{ get; }

		NmpStringList	SpecialArgs	{ get; }

	}

	/////////////////////////////////////////////////////////////////////////////

	public interface IMacroProcessorBase {

		//
		// so don't have to Get<GrandCentral>() (IHub) which has been taking a
		// LOT of overhead - optimize
		//
		IGrandCentral GrandCentral { get; }

		// ******
		NmpStringList	GetMacroNames( bool userOnly );

		// ******
		string FixText( string [] text );
		string FixText( string text );

		// ******
		//
		// altTokenStart indicates is altToken and the leading chars
		// have been removed
		//
		// FindMacro(name, out macro) may fail in ExpressionEvaluatorBase and
		// ArgumentsEvaluator if the call is to a block macro, or any macro that
		// requires the alt format
		//
		//		would need to parse in FindMacr(name, out macro) by calling somthing
		//		on IRecognizer that returns index > 0 if if alt is true
		//
		bool		FindMacro( string name, bool altTokenStart, out IMacro macro );
		bool		FindMacro( string name, out IMacro macro );
		bool		IsMacroName( string name );
		
		// ******
		object	InvokeMacro( IMacroInvocationRecord mir, bool postProcess );

	}


	/////////////////////////////////////////////////////////////////////////////

	public interface IMacroProcessor : IMacroProcessorBase, IHub {

		// ******
		object	OutputInstance				{ get; set; }

		// ******
		IPowerShellInterface	Powershell		{ get; }


		// ******
		//
		// MUST be set to the MacroArguments instance of each macro as it's
		//
		IMacroArguments CurrentMacroArgs	{ get; }
		
		// ******
		object	InvokeMacro( IMacro macro, MacroExpression exp, bool postProcess );
		object	InvokeMacro( IMacro macro, string methodName, object [] args, bool postProcess );
		object	InvokeMacro( IMacro macro, object [] args, bool postProcess = false );
		object	InvokeMacro( string macroName, object [] args, bool postProcess = false );

		// ******
		List<IMacro>	RemoveMacros( NmpStringList macrosList );
		void		UndefMacro( string macroName );
		void		UndefMacros( NmpStringList macroList );
		void		AddMacros( List<IMacro> macro );

		// ******
		List<IMacro>	GetMacros( bool userOnly );

		// ******
		bool		DeleteMacro( string name );
		bool		DeleteMacro( IMacro macro );

		// ******
		string	GenerateMacroName( string appendText );
		string	GenerateArgListName( string appendText );
		string	GenerateLocalName( string appendText );
		string	GenerateListName( string appendText );
		string	GenerateArrayName( string appendText );
		bool IsGeneratedName( string name );


		// ******
		//
		// creates and returns macro instances
		//
		// ******
		//
		// net object macro
		//
		IMacro	CreateObjectMacro( string macroName, object netObj );

		IMacro	CreateBlockMacro( string macroName, IMacroHandler mh );

		//
		// macro called on IMacroHandler.Evaluate()
		//
		IMacro	CreateBuiltinMacro( string macroName, IMacroHandler mh );

		
		//
		// macro called on IMacroHandler.Evaluate, add handlerData to IMacro instance
		//
		IMacro	CreateBuiltinMacro( string macroName, IMacroHandler mh, object handlerData );
		
		//
		// macro is macroProc - uses BuiltinMacroHandler
		//
		IMacro	CreateBuiltinMacro( string macroName, MacroCall macroProc );
		
		//
		// text macro
		//
		IMacro	CreateTextMacro( string macroName, string macroText, IList<string> argNames );

		// ******
		//
		// adds an instance of a macro - see above for what each is
		//
		IMacro		AddMacro( IMacro macro );
		IMacro		AddObjectMacro( string macroName, object netObj );

		IMacro		AddBlockMacro( string macroName, IMacroHandler mh );

		IMacro		AddBuiltinMacro( string macroName, IMacroHandler mh );
		IMacro		AddBuiltinMacro( string macroName, IMacroHandler mh, object handlerData );
		IMacro		AddBuiltinMacro( string macroName, MacroCall macroProc );
		IMacro		AddTextMacro( string macroName, string macroText, IList<string> argNames );

		void			RegisterMacros( string path, bool displayFound );

		IPowerShellInterface	RegisterPowershell( IPowerShellInterface psIntf );
	}


	/////////////////////////////////////////////////////////////////////////////

	public interface IMacroTracer {

		// ******
		void	ProcessMacroBegin( int id, IMacro macro, IMacroArguments macroArgs, bool postProcess );
		void	ProcessMacroDone( int id, IMacro macro, bool postProcessed, bool diverted, object macroResult, object finalResult );
		void	FindMacroCall( string name, IMacro macro );

		// ******
		void	BeginSession( string sessionName, string rootDirectory );
		void	EndSession();

	}


	/////////////////////////////////////////////////////////////////////////////

	public interface INmpHost {

		// ******
		IRecognizer						Recognizer		{ get; }
		IMacroTracer					Tracer				{ get; }
		IMacroProcessorBase		MacroHandler	{ get; }
		string								HostName			{ get; }

		// ******
		bool	ErrorReturns		{ get; }

		void	Error( Notifier notifier, string fmt, params object [] args );
		void	Warning( Notifier notifier, string fmt, params object [] args );

		void	WriteMessage( ExecutionInfo ei, string fmt, params object [] args );
		void	Trace( ExecutionInfo ei, string fmt, params object [] args );

		// ******
		void Die( string fmt, params object [] args );
	}


	/////////////////////////////////////////////////////////////////////////////

	public interface IEvaluationContext {

		// ******
		bool		FromFile	{ get; }
		string	FileName			{ get; }
		string	FileNamePart	{ get; }
		string	FilePathPart	{ get; }

		// ******
		string	Text			{ get; }

		void SetFileInfo( string fullFilePath );

	}


	/////////////////////////////////////////////////////////////////////////////

	public interface IPSObjectList : IList<object> {

		object	LastItem						{ get; }
		object	LastItemUnwrapped		{ get; }

		object	UnwrapItem( object o );
		object	UnwrapItem( int index );

	}


	/////////////////////////////////////////////////////////////////////////////

	public interface IPowerShellInterface {

		// ******
		void	CreateFunctionMacro( string macroName, string funName );
		void	CreateScriptMacro( string scriptName, string scriptText );

		// ******
		IPSObjectList	InvokeFunction( string funName, params object [] args );
		IPSObjectList	InvokeScript( string scriptName, params object [] args );
		IPSObjectList	InvokeScriptText( string text, params object [] args );

		// ******
		bool		GetVariable( string varName, out object obj );
		void		SetVariable( string name, object value );
		string	ExpandString( string str )	;
		
		// ******
		//
		// this allows special checking for 'name' as a property of 'obj'
		//
		//bool		UnknownPropertyHandler( object obj, string name, out object value );

		// ******
		object	UnwrapPSObject( object obj );
	}


	///////////////////////////////////////////////////////////////////////////////
	//
	//public interface PowerShellObject {
	//
	//	
	//	// ******
	//	void	CreateFunctionMacro( string macroName, string funName );
	//	void	CreateScriptMacro( string scriptName, string scriptText );
	//
	//	// ******
	//	PSObjectCollection	InvokeFunction( string funName, params object [] args );
	//	PSObjectCollection	InvokeScript( string scriptName, params object [] args );
	//	PSObjectCollection	InvokeScriptText( string text, params object [] args );
	//
	//	// ******
	//	object	GetVariable( string varName );
	//	void		SetVariable( string varName, object value );
	//
	//	// ******
	//	string	ExpandString( string str );
	//	object	UnwrapPSObject( object obj );
	//}
	//

 
}
