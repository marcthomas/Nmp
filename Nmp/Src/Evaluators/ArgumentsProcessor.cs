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
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NmpBase;
using NmpBase.Reflection;
using Nmp;
using NmpExpressions;

namespace NmpEvaluators {

	//
	// converts a list of text arguments into individual ArgumentExpresion's and
	// return them as an ArgumentList which later need to be interpreted by ArgumentsEvaluator
	//


	/////////////////////////////////////////////////////////////////////////////

	class ArgumentsProcessor : IArgumentsProcessor {

		IScanner scanner;
		IRecognizer recognizer;


		/////////////////////////////////////////////////////////////////////////////

		protected ArgumentExpression EvaluateAtExpression( Expression parent, string exp )
		{
			// ******
			if( string.IsNullOrEmpty(exp) ) {

	// TODO: issue when called by ETB

				scanner.Error( null, "empty '@' expression" );
			}

			IInput input = scanner.Get<GrandCentral>().CreateParseReader( exp, string.Empty, "at expression" );
			string macroName = recognizer.GetMacroName( input );

			if( string.IsNullOrEmpty(macroName) ) {

	// TODO: issue when called by ETB

				scanner.Error( null, "expecting the start of macro name in '@' expression, found: {0}", exp );
			}

			// ******
			var etb = new ETB( macroName, false, scanner, recognizer );
			MacroExpression e = etb.ParseExpression( input );

			return new ArgumentExpression( parent, macroName, e );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected ArgumentExpression EvaluateCastExpression( Expression parent, string str )
		{
			// ******
			//
			// calling SplitString() with 'splitCastMode' set to true, see the comments below where
			// we check the number of items returned
			//
			StringIndexer si = new StringIndexer( str );
			NmpStringList list = SplitString.Split( si, SplitString.END_OF_STRING, SplitString.END_OF_STRING, true );

			// ******
			if( 2 != list.Count ) {
				//
				// "()" 
				//

	// TODO: issue when called by ETB

				scanner.Error( null, "error atempting to cast expression: \"{0}\"", str );
			}

			// ******
			string rhs = list[ 1 ];
			if( ! string.IsNullOrEmpty(rhs) && SC.ATCHAR == rhs[0] ) {

				ArgumentExpression argExp = EvaluateAtExpression( parent, rhs.Substring(1) );

				string typeName = list[ 0 ];
				//Type type = Helpers.FindType( null, typeName.Substring(1, typeName.Length - 2), true );
				Type type = TypeLoader.GetType( typeName.Substring( 1, typeName.Length - 2 ) );
				if( null == type ) {

	// TODO: issue when called by ETB

					scanner.Error( null, "unknown type \"{0}\" while generating cast expression: \"{1}\", note: use full type name including namespaces", typeName, str );
				}

				argExp.SetCastType( type );
				return argExp;
			}
			else {
				try {
					object obj = Arguments.CastArg( list, str );
					return new ArgumentExpression( parent, obj );
				}
				catch {////( ArgumentCastException ex ) {

	// TODO: issue when called by ETB

					//scanner.Error( null, ex.Message );
				}

				return null;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		private bool IgnoreCastLikeThis( string arg )
		{
			if( arg.StartsWith("(?") ) {
				//
				// start of a regular expression options
				//
				return true;
			}
			else if( arg.StartsWith( "(#" ) ) {
				//
				// start of block macro
				//
				return true;
			}

			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		// converts a list of text arguments into individual ArgumentExpresion's and
		// return them as an ArgumentList which later need to be interpreted by ArgumentsEvaluator

		public ArgumentList ProcessArgumentList( Expression parent, NmpStringList strArgs )
		{
			// ******
			ArgumentList objArgs = new ArgumentList();

			// ******
			for( int iArg = 0; iArg < strArgs.Count; iArg++ ) {
				string strArg = strArgs[ iArg ];

				// ******
				if( string.IsNullOrEmpty(strArg) ) {
					//
					// empty string arg
					//
					objArgs.Add( new ArgumentExpression(parent, string.Empty) );
				}
				else {
					char firstChar = strArg[ 0 ];

					//
					// if we don't understand the cast maybe we should just let
					// it go ...
					//
					
					if( SC.OPEN_PAREN == firstChar && ! IgnoreCastLikeThis(strArg) ) {
						//
						// (cast) something
						//
						ArgumentExpression ae = EvaluateCastExpression( parent, strArg );
						if( null != ae ) {
							objArgs.Add( ae );
						}
						else {
							objArgs.Add( new ArgumentExpression( parent, strArg ) );
						}
					}
					else if( SC.ATCHAR == firstChar && strArg.Length > 1 && '@' != strArg[1] ) {
						//
						// @macro ...
						//
						// string must be more than just "@", "@@" escapes it
						//
						objArgs.Add( EvaluateAtExpression( parent, strArg.Substring(1) ) );
					}
					else {
						//
						// a string
						//
						objArgs.Add( new ArgumentExpression(parent, strArg) );
					}
				}
			}

			// ******
			return objArgs;
		}


		/////////////////////////////////////////////////////////////////////////////

		public ArgumentsProcessor( IScanner scanner, IRecognizer recognizer )
		{
			this.scanner = scanner;
			this.recognizer = recognizer;
		}


	}







}
