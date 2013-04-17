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
using Nmp;
using Nmp.Expressions;

namespace NmpExpressions {


	/////////////////////////////////////////////////////////////////////////////

	public class ETBX {

		/////////////////////////////////////////////////////////////////////////////

		public static object CreateMethodInvoker( object obj, string methodName )
		{
			//
			// this will allow a method to be invoked off a macro IF the macro invocation
			// is followed by parens and possible args: (...), otherwise it's just a MethodInvoker
			// instance
			//
			// mp.AddObjectMacro( "method", ETB.CreateMethodInvoker(obj, "MyMethod") );
			//
			// in script: method( arg1, arg2 ... )
			//
			//return new MethodInvoker( obj, methodName );
			return ETB.CreateMethodInvoker( obj, methodName );
		}


	}

	/////////////////////////////////////////////////////////////////////////////

	class ETB {	// : IExpressionTreeBuilder {

		// ******
		string	rootName;
		bool		isAltInvocationExpression;

		IScanner scanner;
		IRecognizer recognizer;


		/////////////////////////////////////////////////////////////////////////////
		
		//public NmpStringList MacroInstructions { get; private set; }


		/////////////////////////////////////////////////////////////////////////////

		protected void OutOfData()
		{

//TODO: need details


			//ThreadContext.MacroError( "out of data while parsing expression" );

			scanner.Error( null, "out of data while parsing expression" );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void UnexpectedCharacter( char ch, string expecting )
		{

//TODO: needs to detail where the error is occuring


			//ThreadContext.MacroError( "unexpected character while parsing macro expression: '{0}': {1}", ch, expecting );

			scanner.Error( null, "unexpected character while parsing macro expression: '{0}': {1}", ch, expecting );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void ArgumentError( Exception ex, string argText )
		{
			ThreadContext.MacroError( "error in cast expression for argument \"{0}\": {1}", argText, ex.Message );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		///  Gets a token from input, unlike macro names token names in
		///  expressions must follow 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>

		public string GetToken( IParseReader input )
		{
			// ******
			StringBuilder sb = new StringBuilder();

			// ******
			while( true ) {
				char ch = input.Peek();

				if( char.IsLetterOrDigit(ch) || '_' == ch ) {
					sb.Append( input.Next() );
				}
				else {
					return sb.ToString();
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void ParseMacroOptions( IInput input, NmpStringList instructions )
		{
			//MacroInstructions = argScanner( input, RecognizedCharType.CloseBracketChar );
			NmpStringList list = scanner.ArgScanner( input, RecognizedCharType.CloseBracketChar );
			instructions.Add( list );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected Expression MemberAccess( bool isRootNode, string memberName, IInput input )
		{
		
			// need MemberAccessExpression constructor that just takes 'memberName' and defaults to NOT root node

			return isRootNode ? new UnnamedMemberAccessExpression(memberName) : new MemberAccessExpression( isRootNode, memberName );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected Expression MethodCall( bool isRootNode, string methodName, bool validToken, IInput input )
		{
			// ******
			Expression expression = null;

			IArgumentsProcessor ap = new ArgumentsProcessor( scanner, recognizer );

			NmpStringList strArgs = scanner.ArgScanner( input, RecognizedCharType.CloseParenChar );

			// ******
			if( ! validToken ) {
				//
				// IndexResult
				//
				return new UnnamedMethodCallExpression( ap, strArgs );
			}
			else {
				if( isRootNode ) {
					expression = new UnnamedMethodCallExpression( methodName, ap, strArgs );
				}
				else {
					expression = new MethodCallExpression( methodName, ap, strArgs );
				}
			}

			// ******
			return expression;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected Expression IndexMember( bool isRootNode, string memberName, bool validToken, IInput input )
		{
			// ******
			Expression expression = null;

			//IArgumentsProcessor ap = new ArgumentsProcessor( argScanner, recognizer );
			IArgumentsProcessor ap = new ArgumentsProcessor( scanner, recognizer );

			//NmpStringList strArgs = argScanner( input, RecognizedCharType.CloseBracketChar );
			NmpStringList strArgs = scanner.ArgScanner( input, RecognizedCharType.CloseBracketChar );

			// ******
			if( ! validToken ) {
				//
				// IndexResult
				//
				return new UnnamedIndexExpression( ap, strArgs );
			}
			else {
				if( isRootNode ) {
					expression = new UnnamedIndexExpression(memberName, ap, strArgs );
				}
				else {
					expression = new IndexExpression( memberName, ap, strArgs );
				}
			}

			// ******
			return expression;
		}


		/////////////////////////////////////////////////////////////////////////////

		private bool HandleFreshToken( bool isRootNode, string token, bool nextIsWhiteSpace, MacroExpression exList, IInput input )
		{
			//
			// where extList is empty and isAltInvocationExpression is
			// true then we have (#someMacro ...) and we have to treat 
			// it as a method invocation IF THE NEXT CHAR IS WHITE-SPACE,
			// but only if it's white space - dont do it for (#someMacro.thing ), 
			// or (#someMacro() ) which is weird
			//
			if( 0 == exList.Count && isAltInvocationExpression && nextIsWhiteSpace ) {
				exList.Add( MethodCall(isRootNode, token, true, input) );
				return true;	// done
			}
			else {
				exList.Add( MemberAccess(isRootNode, token, input) );
				return false;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public MacroExpression ParseExpression( IInput input )
		{
			// ******
			var exList = new MacroExpression( new NmpStringList() );

			// ******
			//
			// first token is the name of the macro
			//
			string	token = rootName;
			bool		haveFreshToken = true;

			// ******
			while( true ) {
				bool isRootNode = 0 == exList.Count;
				bool allDone = false;

				char ch = input.Peek();

				if( haveFreshToken ) {
					allDone = HandleFreshToken( isRootNode, token, char.IsWhiteSpace(ch), exList, input );
					haveFreshToken = false;
				}
				
				if( SC.DOT == ch ) {
					//
					// '.'
					//
					input.Skip( 1 );
					token = GetToken( input );
				
					if( string.IsNullOrEmpty(token) ) {
						//
						// does not return
						//
						UnexpectedCharacter( input.Peek(), "expecting identifier (member name)" );
					}
				
					// ******
					haveFreshToken = true;
				}

// jpm 11 April 13; change back to NOT check for (# because its more correct that what
// we ended up with - that is, without the check its possible for the user to create a
// macro (across multiple lines using '-' at end of line and comments ;;) where we
// get "something()(#defmacro..)" glued together as the result of a macro atempting to
// call a method - not what was intended
//
				//else if( SC.OPEN_PAREN == ch && SC.HASH != input.Peek(1) ) {
				else if( SC.OPEN_PAREN == ch ) {
					//
					// '(' NOT "(#"
					//
					input.Skip( 1 );
					exList.Add( MethodCall(isRootNode, token, haveFreshToken, input) );
					haveFreshToken = false;
				}

				else if( SC.OPEN_BRACKET == ch ) {
					//
					// '['
					//
					input.Skip( 1 );
					exList.Add( IndexMember(isRootNode, token, haveFreshToken, input) );
					haveFreshToken = false;                                               
				}

				else if( SC.ATCHAR == ch && SC.OPEN_BRACKET == input.Peek(1) ) {
					input.Skip( 2 );
					ParseMacroOptions( input, exList.MacroInstructions );
				}
				else {
					//
					// end of expression
					//

					/*	

						should put whatever we do here just before the end of the
						method - get it out of here for cleanthlyness sake


						if is altInvocationExpression and we did not detect it as a method in HandleFreshToken()
							
							we've hit ')' or some char we don't know what to do with

						we could gather up "... )" and add to MacroOptions

							or error

							or just eat it
					*/

					//if( isAltInvocationExpression && ! allDone ) {
					//	//NmpStringList strArgs = argScanner( input, RecognizedCharType.CloseParenChar );
					//	NmpStringList strArgs = scanner.ArgScanner( input, RecognizedCharType.CloseParenChar );
					//}

					if( isAltInvocationExpression ) {
						if( ! allDone ) {
							NmpStringList strArgs = scanner.ArgScanner( input, RecognizedCharType.CloseParenChar );
						}

						if( SC.ATCHAR == input.Peek() && SC.OPEN_BRACKET == input.Peek(1) ) {
							input.Skip( 2 );
							ParseMacroOptions( input, exList.MacroInstructions );
						}

					}

					break;
				}

				// ******
				//
				// since there is no member/token name for the result of an operation
				// we need to create one in case of cascading operations
				//
				if( !haveFreshToken ) {
					token = "result of " + token;
				}
			}

			// ******
			//return 1 == exList.Count ? exList[0] : exList;
			return exList;
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public Expression ParseExpression( IInput input )
		//{
		//	// ******
		//	CompoundExpression exList = new CompoundExpression();
		//
		//	// ******
		//	//
		//	// first token is the name of the macro
		//	//
		//	string	token = rootName;
		//	bool		haveFreshToken = true;
		//
		//	// ******
		//	while( true ) {
		//		bool isRootNode = 0 == exList.Count;
		//
		//		char ch = input.Peek();
		//
		//		if( SC.OPEN_PAREN == ch ) {
		//			//
		//			// '('
		//			//
		//			input.Skip( 1 );
		//			exList.Add( MethodCall(isRootNode, token, haveFreshToken, input) );
		//			haveFreshToken = false;
		//		}
		//		//else if( SC.OPEN_BRACKET == ch ) {
		//		//	//
		//		//	// '['
		//		//	//
		//		//	input.Skip( 1 );
		//		//	exList.Add( IndexMember(isRootNode, token, haveFreshToken, input) );
		//		//	haveFreshToken = false;                                               
		//		//}
		//		else {
		//			bool allDone = false;
		//
		//			if( haveFreshToken ) {
		//				allDone = HandleFreshToken( isRootNode, token, char.IsWhiteSpace(ch), exList, input );
		//				haveFreshToken = false;
		//			}
		//			
		//			if( SC.DOT == ch ) {
		//				//
		//				// '.'
		//				//
		//				input.Skip( 1 );
		//				token = GetToken( input );
		//			
		//				if( string.IsNullOrEmpty(token) ) {
		//					//
		//					// does not return
		//					//
		//					UnexpectedCharacter( input.Peek(), "expecting identifier (member name)" );
		//				}
		//			
		//				// ******
		//				haveFreshToken = true;
		//			}
		//
		//			else if( SC.OPEN_BRACKET == ch ) {
		//				//
		//				// '['
		//				//
		//				input.Skip( 1 );
		//				exList.Add( IndexMember(isRootNode, token, haveFreshToken, input) );
		//				haveFreshToken = false;                                               
		//			}
		//
		//			else if( SC.ATCHAR == ch && SC.OPEN_BRACKET == input.Peek(1) ) {
		//				input.Skip( 2 );
		//				ParseMacroOptions( input );
		//			}
		//			else {
		//				//
		//				// end of expression
		//				//
		//
		//				/*	
		//
		//					should put whatever we do here just before the end of the
		//					method - get it out of here for cleanthlyness sake
		//
		//
		//					if is altInvocationExpression and we did not detect it as a method in HandleFreshToken()
		//						
		//						we've hit ')' or some char we don't know what to do with
		//
		//					we could gather up "... )" and add to MacroOptions
		//
		//						or error
		//
		//						or just eat it
		//				*/
		//
		//		if( isAltInvocationExpression && ! allDone ) {
		//			NmpStringList strArgs = argScanner( input, RecognizedCharType.CloseParenChar );
		//		}
		//
		//
		//				break;
		//			}
		//		}
		//
		//		// ******
		//		//
		//		// since there is no member/token name for the result of an operation
		//		// we need to create one in case of cascading operations
		//		//
		//		if( !haveFreshToken ) {
		//			token = "result of " + token;
		//		}
		//	}
		//
		//	// ******
		//	return 1 == exList.Count ? exList[0] : exList;
		//}
		//

//		/////////////////////////////////////////////////////////////////////////////
//
//		public ETB( MIR mir, IScanner scanner, IRecognizer recognizer )
//		{
//			// ******
//			isAltInvocationExpression = mir.AltToken;
//
//			// ******
//			this.scanner = scanner;
//			this.recognizer = recognizer;
//
//			// ******
//			MacroInstructions = new NmpStringList();
//		}
//

		/////////////////////////////////////////////////////////////////////////////
		
		public ETB( string rootName, bool isAltInvocationExpression, IScanner scanner, IRecognizer recognizer )
		{
			// ******
			this.rootName = rootName;
			this.isAltInvocationExpression = isAltInvocationExpression;
		
			this.scanner = scanner;
			this.recognizer = recognizer;
		
			// ******
			//MacroInstructions = new NmpStringList();
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public static object CreateMethodInvoker( object obj, string methodName )
		{
			//
			// this will allow a method to be invoked off a macro IF the macro invocation
			// is followed by parens and possible args: (...), otherwise it's just a MethodInvoker
			// instance
			//
			// mp.AddObjectMacro( "method", ETB.CreateMethodInvoker(obj, "MyMethod") );
			//
			// in script: method( arg1, arg2 ... )
			//
			return new MethodInvoker( obj, methodName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Func<object> CreatePropertyInvoker( object obj, string propName )
		{
			//
			// result used as the object in an object macro, when the macro is invoked
			// the value of the property 'propName' on obj is returned
			//
			return () => { return new PropertyInvoker(obj, propName).Invoke( (object[]) null ); };
		}


		/////////////////////////////////////////////////////////////////////////////

		public static MacroExpression CreateMacroCallExpression( string name, object [] args )
		{
			// ******
			if( string.IsNullOrEmpty(name) ) {
				throw new ArgumentNullException( "name" );
			}

			// ******
			var argList = new ArgumentList( null == args ? new object [ 0 ] : args );
			var list = new MacroExpression( new NmpStringList() );

			// ******
			list.Add( new MethodCallExpression( name, argList ) );

			// ******
			return list;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static MacroExpression CreateMacroCallExpression( IMacro macro, object [] args )
		{
			// ******
			if( null == macro ) {
				throw new ArgumentNullException( "macro" );
			}

			// ******
			var argList = new ArgumentList( null == args ? new object [0] : args );
			var list = new MacroExpression( new NmpStringList() );

			// ******
			if( MacroType.Builtin == macro.MacroType || MacroType.Text == macro.MacroType ) {
				list.Add( new MethodCallExpression( macro.Name, argList) );
			}
			else {
				//
				// its an object, if it's not a MethodInvoker instance or a delegate
				// it will error when the invocation fails
				//
				list.Add( new UnnamedMemberAccessExpression(macro.Name) );
				list.Add( new UnnamedMethodCallExpression(argList) );
			}

			// ******
			return list;
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public static MacroExpression CreateMacroCallExpression( string macroName, object [] args )
		//{
		//	//return new MethodCallExpression( macroName, new ArgumentList(args) );
		//
		//	// ******
		//	if( null == args ) {
		//		args = new object[ 0 ];
		//	}
		//
		//	// ******
		//	var list = new MacroExpression( new NmpStringList() );
		//	list.Add( new MethodCallExpression( macroName, new ArgumentList(args) ) );
		//	return list;
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public static MacroExpression CreateNullExpression()
		{
			//return new MethodCallExpression( macroName, new ArgumentList(args) );

			var list = new MacroExpression( new NmpStringList() );
			list.Add( new NullExpression() );
			return list;
		}



	}

}
