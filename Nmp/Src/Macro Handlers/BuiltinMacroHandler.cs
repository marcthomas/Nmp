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

namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	class BuiltinMacroHandler : IMacroHandler {

		protected IMacroProcessor mp;
		protected IGrandCentral gc;


		///////////////////////////////////////////////////////////////////////////

		public string Name 
		{
			get;
			set;
		}
		
		
		///////////////////////////////////////////////////////////////////////////
		
		protected bool handlesBlocks = false;
		
		public bool HandlesBlocks
		{
			get {
				return handlesBlocks;
			}

			protected set {
				handlesBlocks = value;
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public virtual string ParseBlock( Expression exp, IInput input )
		{
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual IMacro Create( string name, IMacro macro, object instance, bool clone )
		{
			// ******
			//
			// illegal
			//
			throw new Exception( "Builtin macro instances can not be copied" );
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public static object EvaluateRemainingExpressions( IMacroProcessor mp, object rootObj, MacroExpression exp, int skipCount )
		{
			// ******
			var exp2 = exp.Clone();
			exp2.Items.RemoveRange( 0, skipCount );

			// ******
			var evaluator = new ExpressionEvaluator( mp );
			return evaluator.Evaluate( rootObj, exp2 );
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual object Evaluate( IMacro macro, IMacroArguments macroArgs )
		{
			// ******
			MacroCall method = macro.MacroHandlerData as MacroCall;
			if( null == method ) {
				throw ExceptionHelpers.CreateException( "MacroHandlerData for macro \"{0}\" does not contain a valid MacroCall delegate instance", macro.Name );
			}

			// ******
			//
			// does not return unless validation succeeds
			//
			int skipCount;
			ArgumentList argList = GetArguments( macro.Name, macroArgs.Expression, out skipCount );

			// ******
			//
			// call the macro method
			//
			object result = method( macro, argList, macroArgs );

			// ******
			//
			// finish the expression with the result of the macro method call
			//
			return EvaluateRemainingExpressions( mp, result, (MacroExpression) macroArgs.Expression, skipCount );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static ArgumentList GetArguments( string macroName, Expression expIn )
		{
			int skipCount;
			return GetArguments( macroName, expIn, out skipCount );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static ArgumentList GetArguments( string macroName, Expression expIn, out int skipCount )
		{
			// ******
			if( ExpressionType.Compound != expIn.NodeType ) {
				throw new Exception( string.Format( "macro handler expeced a compound expression invoking macro \"{0}\"", macroName ) );
			}

			//
			// arguments for the first item in the compound expression list
			//

			// ******
			int usedExpressionCount = 0;

			// ******
			var expList = expIn.Cast<MacroExpression>().Items;
			Expression exp = expList [ 0 ];

			// ******
			ArgumentList argList = null;

			switch( exp.NodeType ) {
				case ExpressionType.Index:
				case ExpressionType.MethodCall:

				case ExpressionType.UnnamedIndex:
				case ExpressionType.UnnamedMethodCall:
					//
					// arguments to the indexer method, or the arguments to the
					// method being called
					//
					argList = GetArgumentList( exp );

					usedExpressionCount = 1;
					break;

				case ExpressionType.MemberAccess:
				case ExpressionType.UnnamedMemberAccess:
					//
					// just the Unnamed invoking expression
					//

					/*
						ok, what were looking at is

							since a macro is being accessed (otherwise we would not be called) we
							know this is an UnnamedMemberAccess, not MemberAccess

							so:

							a.	UnnamedMemberAccess is the first expression

							b.	if there is an additional expression we check that expression
									to see if its an index or method call expression: [] or ()
									following the member access (e.g. macro[] or macro())

									if it is (index or method call) then we return the arguments
									that were included in that call - maybe some, maybe none

									its up to our caller to to know what to do

									in practice, we will only be called by block macros which are
									??? always interpreted as method calls
					*/


					if( expList.Count > 1 ) {
						exp = expList [ 1 ];
						if( exp.IsIndexOrMethodCallExpression() ) {
							argList = GetArgumentList( exp );
							//
							// we use the UnnmaedMemberAccess expression AND the index or
							// method call expression that follows (as noted above)
							//
							usedExpressionCount = 2;
						}
					}
					break;

				//
				// macros with the NonExpressive flag set will have a Null expression; the
				// expression should be ignored
				//
				case ExpressionType.Null:
					usedExpressionCount = 1;
					break;

				default:
					throw new Exception( string.Format( "macro handler expeced an UnnamedMethodCall, UnnamedIndex or UnnamedMemberAccess Expression invoking macro \"{0}\"", macroName ) );
			}

			// ******
			if( null == argList ) {
				
				//throw ExceptionHelpers.CreateException( "macro handler expeced an argument list for UnnamedMethodCall, UnnmaedIndex or UnnamedMemberAccess Expression, invoking macro \"{0}\"", macroName );

				argList = new ArgumentList();
			}

			// ******
			skipCount = usedExpressionCount;
			return argList;
		}


		/////////////////////////////////////////////////////////////////////////////

		private static ArgumentList GetArgumentList( Expression exp )
		{
			var argGetter = exp as IExpressionArguments;
			return null == argGetter ? null : argGetter.Arguments;
		}


		/////////////////////////////////////////////////////////////////////////////

		public BuiltinMacroHandler( IMacroProcessor mp )
		{
			this.mp = mp;
			this.gc = mp.GrandCentral;
		}

	}


}
