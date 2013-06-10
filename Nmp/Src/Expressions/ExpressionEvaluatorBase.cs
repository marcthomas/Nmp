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

using NmpBase;
using Nmp;


namespace NmpExpressions {

	/////////////////////////////////////////////////////////////////////////////

	class NullValueSkipRemainderException : Exception {

		public NullValueSkipRemainderException()
		{
		}

	}


	/////////////////////////////////////////////////////////////////////////////

	abstract class ExpressionEvaluatorBase {

		// ******
		protected IMacroProcessor macroProcessor;
		protected TypeHelperDictionary typeHelpers;
		protected ExtensionTypeDictionary methodExtensions;

		// ******
		public abstract object EvalMemberAccessExpression( object obj, MemberAccessExpression exp );
		public abstract object EvalUnnamedMemberAccessExpression( object obj, UnnamedMemberAccessExpression exp );

		public abstract object EvalUnnamedMethodCallExpression( object obj, UnnamedMethodCallExpression exp );
		public abstract object EvalMethodCallExpression( object obj, MethodCallExpression exp );

		public abstract object EvalUnnamedIndexExpression( object obj, UnnamedIndexExpression exp );
		public abstract object EvalIndexExpression( object obj, IndexExpression exp );

		public abstract object EvalActionExpression( object obj, ActionExpression exp );


		/////////////////////////////////////////////////////////////////////////////

		protected void ReturnNull()
		{
			throw new NullValueSkipRemainderException();
		}


		/////////////////////////////////////////////////////////////////////////////

		protected object ExecuteMacro( string name, MacroExpression exp )
		{
			IMacro macro;
			if( !macroProcessor.FindMacro( name, out macro ) ) {
				ThreadContext.MacroError( "\"{0}\" is not the name of a macro", name );
			}

			// ******
			//return macroProcessor.ProcessMacro( macro, new MacroArguments(macro, null, exp), false );
			return macroProcessor.InvokeMacro( macro, exp, false );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected object Process( object objIn, Expression exp )
		{
			// ******
			if( null == objIn ) {
				//
				// this should ONLY happen when we're called by Arguments(...)
				//
			}


			// ******
			object obj;

			// ******
			switch( exp.NodeType ) {
				case ExpressionType.Compound:
					obj = EvalCompoundExpression( objIn, exp.Cast<CompoundExpression>() );
					break;

				case ExpressionType.MemberAccess:
					obj = EvalMemberAccessExpression( objIn, exp.Cast<MemberAccessExpression>() );
					break;

				case ExpressionType.UnnamedMemberAccess:
					obj = EvalUnnamedMemberAccessExpression( objIn, exp.Cast<UnnamedMemberAccessExpression>() );
					break;

				case ExpressionType.MethodCall:
					obj = EvalMethodCallExpression( objIn, exp.Cast<MethodCallExpression>() );
					break;

				case ExpressionType.UnnamedMethodCall:
					obj = EvalUnnamedMethodCallExpression( objIn, exp.Cast<UnnamedMethodCallExpression>() );
					break;

				case ExpressionType.Index:
					obj = EvalIndexExpression( objIn, exp.Cast<IndexExpression>() );
					break;

				case ExpressionType.UnnamedIndex:
					obj = EvalUnnamedIndexExpression( objIn, exp.Cast<UnnamedIndexExpression>() );
					break;

				case ExpressionType.Action:
				//obj = EvalActionExpression( objIn, exp.AsAction() );
				//break;


				default:
					throw ExceptionHelpers.CreateException( "unknown expression type \"{0}\"", exp.NodeType.ToString() );
			}

			// ******
			return obj;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object Eval( object netObj, Expression exp )
		{
			return Process( netObj, exp );
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual object EvalCompoundExpression( object objIn, CompoundExpression exp )
		{
			// ******
			var obj = objIn;
			foreach( Expression expression in exp.Cast<CompoundExpression>().Items ) {
				object newObj = Process( obj, expression );

				if( null == newObj ) {
					ReturnNull();
				}

				obj = newObj;
			}

			// ******
			return obj;
		}

		/////////////////////////////////////////////////////////////////////////////

		public ExpressionEvaluatorBase( IMacroProcessor mp )
		{
			macroProcessor = mp;
			typeHelpers = mp.Get<TypeHelperDictionary>();
			methodExtensions = mp.Get<ExtensionTypeDictionary>();
		}

	}
}
