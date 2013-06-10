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
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Reflection;


using NmpBase;
using NmpEvaluators;
using NmpExpressions;

#pragma warning disable 414

namespace Nmp.Builtin.Macros {


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// 
	/// </summary>

	class IfMacroHandler : BlockMacroHandlerBase {

		// ******
		//
		// (#if `trueOrFalseExpression') ... (#endarray)
		//
		IEnumerable<Type> _argTypes = new Type[] { typeof(bool) };
		IEnumerable<object> _defArgs = new object[] { false };

		// ******
		protected override IEnumerable<Type> 		ExpectedArgTypes		{ get { return _argTypes; } }
		protected override IEnumerable<object>	DefaultArgs { get { return _defArgs; } }

		// ******
		const string	IF						= "if";

		const string	IF_START			= "#if";
		const string	ELSEIF_START	= "#elseif";
		const string	ELSE					= "#else)";
		const string	ENDIF					= "#endif)";

		const string	IF_INJECT			= "(#if ";
		const string	ELSEIF_INJECT	= "(#elseif ";
		const string	ELSE_INJECT		= "(#else)";
		const string	ENDIF_INJECT	= "(#endif)";

		const int DEPTH_ZERO	= 0;

		// (#if bool_expression )
		//
		// ... (#elseif ...) / (#else)
		//
		// (#endif)

//		///////////////////////////////////////////////////////////////////////////
//
//		
//string	IfElseEndifPatterns =
//
//@"(?x)\A
//(
//\(\#if[ \t]|
//\(\#elseif[ \t]|
//\(\#else\)|
//\(\#endif\)|
//
//)
//";
//
		
		///////////////////////////////////////////////////////////////////////////

		private StringBuilder IfProcessor( IInput input, bool processing, int depth )
		{
		StringBuilder	result = new StringBuilder();
		bool	inElse = false;
		bool done = false;
		
		/*
		
			processing says whether we are saving text or not
			
				text is only saved if processing is true
			
			depth is how may if statements deep we are into the process
			
				depth zero is the first level after examining the truth of the outermost if, at this
				level processing can be toggled by the else statement and NOT have an else token reinjected
				into the code
				
				at levels greather than zero if/else/endif tokens are reinjected into the code to be
				processed after the text has been pushed back onto the input and is scanned again
		
		
		*/

			// ******
			char quoteStartChar = gc.SeqOpenQuote.FirstChar;
			
			// ******
			while( true ) {
				//
				// first quote check
				//
				if( quoteStartChar == input.Peek() && gc.SeqOpenQuote.Starts(input) ) {
					//
					// GetQuotedText() strips the outer quotes but perserves inner quotes; but
					// first we need to eat the open quote
					//
					gc.SeqOpenQuote.Skip( input );
					string text = gc.GetQuotedText( input, true );

					if( processing ) {
						result.Append( text );
					}
				}

				// ******				
				char ch = input.Next();

				// ******				
				if( SC.NO_CHAR == ch ) {
					ThreadContext.MacroError( "IfHandler: end of input before (#endif) found" );
					return result;
				}
				
				// ******				
				//
				// (#if ...
				//
				if( SC.OPEN_PAREN == ch && input.StartsWith(IF_START) ) {
					//
					// get recursive
					//
					input.Skip( IF_START.Length );

					if( ! char.IsWhiteSpace(input.Peek()) ) {
						ThreadContext.MacroError( "expected white space following \"(#if\"" );
					}
					
					if( processing ) {
						result.Append( IF_INJECT );
					}

					result.Append( IfProcessor(input, processing, 1 + depth) );
					continue;
				}
				
				//
				// (#elseif ...
				//
				else if( SC.OPEN_PAREN == ch && input.StartsWith(ELSEIF_START) ) {
					int argsStart = input.Index;
					input.Skip( ELSEIF_START.Length );
					
					if( ! char.IsWhiteSpace(input.Peek()) ) {
						ThreadContext.MacroError( "expected white space following \"(#elseif\"" );
					}

					if( inElse ) {
						ThreadContext.MacroError( "out of place (#elseif) in (#if ...) statement" );
					}
				
					// ******
					//
					// we're responsible for gather the arguments to `elseif'
					//
					NmpStringList strList = mp.Get<IScanner>().ArgScanner( input, RecognizedCharType.CloseParenChar );
					int argsEnd = input.Index - 1;
				
					if( SC.NEWLINE == input.Peek() ) {
						input.Skip( 1 );
					}
				
					// ******
					if( processing ) {
						if( depth > DEPTH_ZERO ) {
							//
							// reinject elseif token and text: "[token] expression )"
							//
							string text = input.GetText(argsStart, argsEnd);
							result.AppendFormat( "({0}", text );
						}
						else {
							//
							// else DEPTH_ZERO, need to stop processing
							//
							processing = false;
							done = true;
						}
					}
					else if( DEPTH_ZERO == depth && ! done ) {
						////
						//// since we're plowing through the text ourselves we need to 
						//// call out to have the `elseif' arguments evaluated
						////
						//var ah = new ArgumentHandler( mp, "if processor" );
						//ah.Initialize( new Type [] { typeof(bool) }, strList );
						//processing = (bool) ah[ 0 ];
						// 
						// already evaluated when ArgScanner was run
						// 
						processing = 0 == strList.Count ? false : Helpers.IsMacroTrue( strList[0] );
					}
				
					continue;
				}
				
				//
				// (#else)
				//
				else if( SC.OPEN_PAREN == ch && input.StartsWith(ELSE) ) {
					input.Skip( ELSE.Length );

					if( inElse ) {
						ThreadContext.MacroError( "out of place (#else) in (#if ..) statement" );
					}
					
					if( SC.NEWLINE == input.Peek() ) {
						input.Skip( 1 );
					}

					// ******
					inElse = true;
					if( processing && depth > DEPTH_ZERO ) {
						//
						// reinject endif token
						//
						result.Append( ELSE_INJECT );
					}

					// ******
					//
					// toggle what we're doing
					//
					if( ! done && DEPTH_ZERO == depth ) {
						processing = ! processing;
					}
					continue;
				}
			
				//
				// (#endif)
				//
				else if( SC.OPEN_PAREN == ch && input.StartsWith(ENDIF) ) {
					//
					// in theory all done
					//
					input.Skip( ENDIF.Length );

					if( SC.NEWLINE == input.Peek() ) {
						input.Skip( 1 );
					}

					if( processing && depth > DEPTH_ZERO ) {
						//
						// reinject endif token
						//
						result.Append( ENDIF_INJECT );
					}
					
					// ******
					break;
				}
				
				// ******
				if( processing ) {
					result.Append( ch );
				}
			}
			
			// ******
			return result;
		}

		
		/////////////////////////////////////////////////////////////////////////////

		public override string ParseBlock( Expression exp, IInput input )
		{
			// ******
			var args = GetMacroArgsAsTuples( exp ) as NmpTuple<bool>;
			bool initialIfIsTrue = args.Item1;

			//  ******
			string	result = string.Empty;
			StringBuilder text = new StringBuilder();
			bool	processing = initialIfIsTrue;
		
			// ******
			if( SC.NEWLINE == input.Peek() ) {
				input.Skip( 1 );
			}

			// ******
			return IfProcessor( input, processing, DEPTH_ZERO ).ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Evaluate( IMacro macro, IMacroArguments macroArgs )
		{
			// ******
			//
			// we did all the work in ParseBloc(), the result was placed in
			// BlockText for us to return
			//
			return macroArgs.BlockText;
		}


		/////////////////////////////////////////////////////////////////////////////

		public IfMacroHandler( IMacroProcessor mp )
			: base(IF, mp)
		{
			handlesBlocks = true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static IMacro Create( IMacroProcessor mp )
		{
			// ******
			var handler = new IfMacroHandler( mp );
			IMacro macro = mp.CreateBlockMacro( handler.Name, handler );

			//
			// note: MacroFlags.Pushback is how we reinject the if-true text back
			// into the macro processor
			//
			macro.Flags |= MacroFlags.AltTokenFmtOnly | MacroFlags.Pushback | MacroFlags.RequiresArgs;

			// ******
			return macro;
		}


	}
}
