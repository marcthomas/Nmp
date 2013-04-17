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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using NmpBase;
using Nmp.Input;
using NmpExpressions;
using Nmp.Expressions;
using Nmp.Output;

#pragma warning disable 414, 219


namespace Nmp {



	/////////////////////////////////////////////////////////////////////////////

	[Serializable()]
	class NmpScanner : IScanner {

		/////////////////////////////////////////////////////////////////////////////

		public class ScannerOptions {
			public RecognizedCharType	BreakChar1;
			public RecognizedCharType	BreakChar2;
			public bool								FoundQuotedText;
			public bool								KeepQuotes;

			public ScannerOptions( RecognizedCharType b1, RecognizedCharType	b2, bool keepQuotes = false )
			{
				BreakChar1 = b1;
				BreakChar2 = b2;
				FoundQuotedText = false;
				KeepQuotes = keepQuotes;
			}

			public ScannerOptions( ScannerOptions so )
				:	this( so.BreakChar1, so.BreakChar2, so.KeepQuotes )
			{
				FoundQuotedText = false;
			}

		}

		// ******
		//Hub hub;
		GrandCentral gc;
		IMacroProcessorBase	mpBase;
		
		// ******
		//
		// default implementation ignores this, in face it has to
		// ignore this because it's initialized with null
		//
		IErrorWarningTrace errorWarningTrace;


		/////////////////////////////////////////////////////////////////////////////

		public T Get<T>() where T : class
		{
			return gc.Get<T>();
		}


		///////////////////////////////////////////////////////////////////////////////

		public IGrandCentral GrandCentral
		{
			get
			{
				return gc;
			}
		}


		///////////////////////////////////////////////////////////////////////////////

		public IRecognizer Recognizer
		{
			get {
				return gc.Get<IRecognizer>();
			}
		}


		///////////////////////////////////////////////////////////////////////////////

		public bool ErrorReturns
		{
			get {
				//
				// default error method does not return
				//
				return false;
			}
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Error( Notifier notifier, string fmt, params object [] args )
		{
			ThreadContext.MacroError( notifier, fmt, args );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Warning( Notifier notifier, string fmt, params object [] args )
		{
			ThreadContext.MacroWarning( notifier, fmt, args );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void WriteMessage( string fmt, params object [] args )
		{
			ThreadContext.WriteMessage( fmt, args );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Trace( string fmt, params object [] args )
		{
			ThreadContext.Trace( fmt, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string HandleMacro( IInput input, MIR mir )
		{
			// ******
			if( gc.BreakNext ) {
				gc.BreakNext = false;

				if( Debugger.IsAttached ) {
					Debugger.Break();
				}
			}

			// ******
			IMacro macro = mir.Macro;
			mir.State = MacroProcessingState.Parsing;
	
			// ******
			MacroExpression exp = null;

			if( 0 == (MacroFlags.NonExpressive & macro.Flags) ) {
		 		ETB expressionTreeBuilder = new ETB( mir.Macro.Name, mir.AltToken, this, Recognizer );
		 		exp = expressionTreeBuilder.ParseExpression( input );
			}
			else {
				exp = ETB.CreateNullExpression();
			}

			// ******
			string blockText = string.Empty;
			if( macro.IsBlockMacro ) {
				blockText = mir.Macro.MacroHandler.ParseBlock( exp, input );
			}

			// ******
			mir.State = MacroProcessingState.Processing;

			mir.SetSourceEndIndex( input.Index );
			mir.MacroArgs = new MacroArguments(	macro, 
																					input,
																					exp,
																					mir.SpecialArgs, 
																					blockText
																				);

///	macroProcessor.DumpExpressionOnly = true;

			return mpBase.ProcessMacro( mir );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void PostProcessArgument( NmpStringList args, string str )
		{
			// ******
			StringBuilder sb = new StringBuilder();
			NonEscapingParseReader reader = gc.GetNonEscapingParseReader( str );
			//StringIndexer reader = new StringIndexer( str );
			CharSequence openQuote = gc.SeqOpenQuote;

	/*
	 * by using ParseReader its translating characters like in "d:\fred\nmp\etc" '\n'
	 * 
	 * need to do this on a string, add GetQuotedText for string
	 * 
	 */

			while( ! reader.AtEnd ) {
				char ch = reader.Peek();

				if( openQuote.FirstChar == ch && openQuote.Starts(reader) ) {
					//reader.Skip( 1 );

					openQuote.Skip( reader );

					string result = gc.GetQuotedText( reader, false );
					sb.Append( result );
				}
				else if( SC.COMMA == ch ) {
					reader.Skip( 1 );
					reader.Skip( c => char.IsWhiteSpace(c) );
					args.Add( sb.ToString() );
					sb.Length = 0;

				}
				else {
					sb.Append( reader.Next() );
				}
			}

			// ******
			args.Add( sb.ToString() );
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpStringList ArgScanner( IInput input, RecognizedCharType terminalChar )
		{
			// ******
			NmpStringList args = new NmpStringList();
			var output = new NmpOutput();

			// ******
			ScannerOptions options = new ScannerOptions(RecognizedCharType.ArgSepChar, terminalChar, true);

			// ******
			while( true ) {
				if( input.AtEnd ) {

// TODO: need file/line/col number for use with MPError

					ThreadContext.MacroError( "end of data parsing argument list, {0} character not found", terminalChar.ToString() );
				}
				
				// ******
				output.Zero();
				options.FoundQuotedText = false;

				/*

					()

					(   )

					( arg )

					( `' )

					( `  ' )

					( arg, arg )

					( `', arg )

					( `  ', `  arg   ' )


				*/

				// ******
				//
				// have to scan without stripping quotes so that we can preserve
				// white space within quotes when we trim the string - afterword
				// we strip the quotes
				//
				// note that the scanner may miss some commas that are generated by
				// macro expansion because the text (if pushback is off) will be
				// put in the output buffer and not seen by the scanner, PostProcessArgument() 
				// will locate these extra commas and split the result into multiple arguments
				//
				// LIMITATION: a macro can NOT supply the closing parenthesis for the argument
				// list unless pushback is turned on
				//
				// Additionaly, white space that preceeds the macro generated comma is NOT trimmed
				// away
				//
				RecognizedCharType breakChar = Scanner( input, output, options );

				// ******
				//
				// trims white-space and removes any outer quotes that might have been protecting
				// inner white-space that the caller wanted to maintain
				//
				// PostProcess will locate new commas that might have been generated by macro
				// expansion and split the string into multiple arguments
				//

				//
				// note that trimming the white space will also remove leading and trailing white space
				// from any macro that was invoked by the scanner (above)
				//


				string argStr = output.Contents.ToString().Trim();	//TrimArgument( output.Contents );
				PostProcessArgument( args, argStr );

				// ******
				if( terminalChar == breakChar ) {
					if( 1 == args.Count && string.IsNullOrEmpty(args[0]) && ! options.FoundQuotedText ) {
						//
						// since we've hit the closing char and there is only one argument
						// and if that arg was empty there was no argument, we had () with
						// maybe some white-space between the open/close chars
						//
						// unles we had ( `' )
						//
						args.RemoveAt( 0 );
					}
					return args;
				}
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public string Scanner( string textToScan, string sourceContext )
		{
			// ******
			var input = gc.GetParseReader( textToScan, string.Empty, sourceContext );
			var output = new NmpOutput();

			// ******
			Scanner( input, output );

			// ******
			return output.StringContents;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Scanner( IInput input, IOutput output )
		{
			Scanner( input, output, new ScannerOptions(RecognizedCharType.UnknownCharType, RecognizedCharType.UnknownCharType) );
		}


		/////////////////////////////////////////////////////////////////////////////

		int scanDepth = 0;

		char [] protectParens = new char [] { SC.OPEN_PAREN, SC.CLOSE_PAREN };
		char [] protectBrackets = new char [] { SC.OPEN_BRACKET, SC.CLOSE_BRACKET };

		public RecognizedCharType Scanner( IInput input, IOutput output, ScannerOptions options )
		{
			// ******
			///Console.WriteLine( "{0}", scanDepth );

			if( ++scanDepth >= gc.MaxMacroScanDepth ) {
				//
				// recursion depth exceeded
				//
				Error( new MacroExecutionNotifier(new RunawayMacroException("The scanner recursion depth has exceeded the maximum ({0})", gc.MaxMacroScanDepth)), string.Empty );
			}

			// ******
			try {
				////
				//// once in a while
				////
				//if( ++elapsedTimeCheckCounter > 1000 && ! Debugger.IsAttached ) {
				//	//
				//	// check if we've exceeded our maximum execution time
				//	//
				//	elapsedTimeCheckCounter = 0;
				//
				//	TimeSpan ts = DateTime.Now.Subtract( initTime );
				//	if( ts.TotalMilliseconds > macros.MaxExecTime ) {
				//		throw new ExitException( 1, "maximum execution time ({0}ms) exceeded.", macros.MaxExecTime );
				//	}
				//}
			

				//bool flagEmptyQuotedStr = false;

				RecognizedCharType breakChar1 = options.BreakChar1;
				RecognizedCharType breakChar2 = options.BreakChar2;

				// ******
				RecognizedCharType breakProtect = RecognizedCharType.UnknownCharType;
				char [] breakProtectArray = null;

				if( breakChar2 == RecognizedCharType.CloseParenChar ) {
					breakProtect = RecognizedCharType.OpenParenChar;
					breakProtectArray = protectParens;
				}
				else if( breakChar2 == RecognizedCharType.CloseBracketChar ) {
					breakProtect = RecognizedCharType.OpenBracketChar;
					breakProtectArray = protectBrackets;
				}

				// ******
				while( !input.AtEnd ) {
					TokenMap tm;

					// ******
					RecognizedCharType charType = Recognizer.Next( input, out tm );

					if( RecognizedCharType.TokenStart == charType ) {
						int pos = input.Index;
						int line = input.Line;
						int column = input.Column;
						IMacro macro = null;

						// ******
						bool found = mpBase.FindMacro( tm.Token, tm.IsAltTokenFormat, out macro );

						if( found ) {
							bool notAltTokenFormat = ! tm.IsAltTokenFormat;

							if( notAltTokenFormat && 0 != ((int) macro.Flags & (int) MacroFlags.AltTokenFmtOnly) ) {
								//
								// gota be (#macro ...)
								//
								output.Write( Recognizer.GetText( input, tm ) );
							}

							else {
								//
								// now that we know its a macro we can skip over the macro
								// name and (possibly) the alt invocation chars
								//
								Recognizer.Skip( input, tm );

								// ******
								//
								// macro invocation record
								//
								var mir = new MIR( macro, tm.IsAltTokenFormat, tm.RegExCaptures, input, "Macro: " + macro.Name, pos, line, column );

								// ******
								//
								// handle the macro
								//
								
								//
								// can NOT pushPop when were in editor mode
								//

								using( Get<InvocationContext>().Init( mir ) ) {
									//
									// recognizer may change behind our back durring a macro
									// call - just showing that 'tm' could become invalid
									//
									tm = null;

									// ******
									string macroResult = HandleMacro( input, mir );

									if( macroResult.Length > 0 ) {
										if( mir.MacroArgs.Options.Pushback ) {
											input.PushBack( macroResult );
										}
										else {
											output.Write( macroResult );
										}
									}
								}
							}

						}
						else {
							//
							// get and output the text that we now know is NOT a macro
							//
							output.Write( Recognizer.GetText( input, tm ) );
						}
					}

					else if( RecognizedCharType.QuoteStartChar == charType && Recognizer.OpenQuote.Starts( input ) ) {
						//
						// GetQuotedText() strips the outer quotes but perserves inner quotes; but
						// first we need to eat the open quote
						//
						Recognizer.OpenQuote.Skip( input );

						string quotedResult = gc.GetQuotedText( input, options.KeepQuotes, false );
						output.Write( quotedResult );

						options.FoundQuotedText = true;
					}

					else if( breakProtect == charType ) {

						//input.Skip( 1 );
						//output.Write( ThreadContext.GetBalancedText( input, breakProtectArray [ 0 ], breakProtectArray [ 1 ] ) );

						var so = new ScannerOptions( options );
						var tempOutput = new NmpOutput();

						input.Skip( 1 );
						RecognizedCharType rct = Scanner( input, tempOutput, so );
						output.WriteChar( breakProtectArray [0] );
						output.Append( tempOutput );
						output.WriteChar( breakProtectArray [1] );

					}

					else if( breakChar1 == charType || breakChar2 == charType ) {
						input.Skip( 1 );
						return charType;
					}

					else {
						char ch = input.Next();
						if( SC.NO_CHAR != ch ) {
							//
							// note: must avoid writing SC.NO_CHAR !
							//
							output.WriteChar( ch );
						}
					}
				}

				// ******
				return RecognizedCharType.UnknownCharType;
			}
			
			finally {
				--scanDepth;
			}
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public void NmpScannerInitialize( IRecognizer recognizer, IMacroProcessorBase mpBase, IErrorWarningTrace errorWarningTrace )
		//{
		//	this.Recognizer = recognizer;
		//	//
		//	// allow ourselves to be initialize with an IMacroProcessor but at
		//	// this time it MUST be the default one
		//	//
		//	this.mpBase = mpBase;
		//	this.errorWarningTrace = errorWarningTrace;
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public NmpScanner( IRecognizer recognizer, IMacroProcessorBase mpBase, IErrorWarningTrace errorWarningTrace )
		//{
		//	NmpScannerInitialize( recognizer, mpBase, errorWarningTrace );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		//public void NmpScannerInitialize( Hub hub, IMacroProcessorBase mpBase, IErrorWarningTrace errorWarningTrace )

		public void NmpScannerInitialize( IMacroProcessorBase mpBase, IErrorWarningTrace errorWarningTrace )
		{
			// ******
			//this.hub = hub;
			//this.gc = hub.Get<GrandCentral>();
			this.gc = mpBase.GrandCentral as GrandCentral;

			//
			// need GrandCentral to get the parsers which are not exposed by the interface
			//
			//this.gc = mpBase.GrandCentral;

			// ******
			//
			// allow ourselves to be initialize with an IMacroProcessor but at
			// this time it MUST be the default one
			//
			this.mpBase = mpBase;
			this.errorWarningTrace = errorWarningTrace;
		}


		/////////////////////////////////////////////////////////////////////////////

		//public NmpScanner( Hub hub, IMacroProcessorBase mpBase, IErrorWarningTrace errorWarningTrace )
		//{
		//	NmpScannerInitialize( hub, mpBase, errorWarningTrace );
		//}

		public NmpScanner( IMacroProcessorBase mpBase, IErrorWarningTrace errorWarningTrace )
		{
			NmpScannerInitialize( mpBase, errorWarningTrace );
		}

		/////////////////////////////////////////////////////////////////////////////

		public NmpScanner()
		{
		}

	}

}

