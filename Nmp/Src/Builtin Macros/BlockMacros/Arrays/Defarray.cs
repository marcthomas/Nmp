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
using Nmp.Expressions;


#pragma warning disable 169

namespace Nmp.Builtin.Macros {

/*

http://www.json.org/example.html

http://www.bing.com/search?q=JSON+examples&form=QBLH&qs=n&sk=&sc=4-13

http://tools.ietf.org/html/rfc4627
*/


	/////////////////////////////////////////////////////////////////////////////

	class Defarray : BlockMacroHandlerBase {

		// ******
		//
		// (#defarray `macroName') ... (#endarray)
		//
		IEnumerable<Type> _argTypes = new Type[] { typeof(string) };
		IEnumerable<object> _defArgs = new object[] { string.Empty };

		// ******
		protected override IEnumerable<Type> 		ExpectedArgTypes		{ get { return _argTypes; } }
		protected override IEnumerable<object>	DefaultArgs { get { return _defArgs; } }

		// ******
		const string	DEFARRAY					= "defarray";

		const string	DEFARRAY_START		= "#defarray";
		const string	ENDARRAY					= "#endarray)";

		const string	DEFARRAY_INJECT		= "(#defarray ";
		const string	ENDARRAY_INJECT		= "(#endarray)";


		StringBuilder trace = new StringBuilder();


/*

	stack objects on enter

	when exit then pop and that becomes a value in the poped object
		
		- either array value
		- or value part following key


*/



		///////////////////////////////////////////////////////////////////////////

		private void ParseMacro( IInput input, StringBuilder sb )
		{
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
					sb.Append( gc.GetQuotedText(input, true) );
				}

				// ******				
				char ch = input.Next();

				if( SC.NO_CHAR == ch ) {
					ThreadContext.MacroError( "defarray: end of input before (#endarray) found" );
				}
				
				if( SC.OPEN_PAREN == ch && input.StartsWith(ENDARRAY) ) {
					input.Skip( ENDARRAY.Length );
					return;
				}

				else {
					sb.Append( ch );
				}
			}
		}

		
		/////////////////////////////////////////////////////////////////////////////

		public override string ParseBlock( Expression exp, IInput input )
		{
			// ******
			//
			// remove any newline following the closing paren (#macro ...)
			//
			if( SC.NEWLINE == input.Peek() ) {
				input.Skip( 1 );
			}

			// ******
			StringBuilder macroText = new StringBuilder();

			macroText.Append( '{' );
			ParseMacro( input, macroText );
			macroText.Append( '}' );

			// ******
			//
			// remove any newline following the closing paren for the (#endmacro)
			//
			if( SC.NEWLINE == input.Peek() ) {
				input.Skip( 1 );
			}

			// ******
			return macroText.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// (#defarray `macroName' )
		//
		/////////////////////////////////////////////////////////////////////////////

		public override object Evaluate( IMacro macro, IMacroArguments macroArgs )
		{
			// ******
			var args = GetMacroArgsAsTuples( macroArgs.Expression ) as NmpTuple<string>;
			string macroName = args.Item1;

			// ******
			if( string.IsNullOrEmpty(macroName) ) {
				ThreadContext.MacroError( "attempt to define or pushdef a macro without a name" );
			}

			// ******
			//
			// parse text into NmpArray instance
			//
			try {
				var newArray = NmpArray.BuildArray( macroArgs.BlockText );

				// ******
				//
				// create the new macro
				//
				IMacro newMacro = mp.AddObjectMacro( macroName, newArray );
			}
			catch ( NmpJSONException ex ) {
				ThreadContext.MacroError( ex.Message );
			}

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public Defarray( IMacroProcessor mp )
			: base(DEFARRAY, mp)
		{
			handlesBlocks = true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static IMacro Create( IMacroProcessor mp )
		{
			// ******
			var handler = new Defarray(mp);

			//IMacro macro = mp.CreateBuiltinMacro( o.Name, o );
			IMacro macro = mp.CreateBlockMacro( handler.Name, handler );

			macro.Flags |= MacroFlags.AltTokenFmtOnly | MacroFlags.RequiresArgs;

			// ******
			return macro;
		}

	}
}
