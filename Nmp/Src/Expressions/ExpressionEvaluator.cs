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
using System.Reflection;
using System.Text;

//#pragma warning disable 414


using NmpBase;
using Nmp;


namespace Nmp.Expressions {


	/////////////////////////////////////////////////////////////////////////////

	class ExpressionEvaluator : ExpressionEvaluatorBase, IExpressionEvaluator {


		/////////////////////////////////////////////////////////////////////////////

		protected void CheckNullResult( object obj, object parent, string memberName )
		{
			if( null == obj ) {
				ThreadContext.Trace( "a null value was returned by {0}.{1}", ObjectInfo.GetObjectType(parent).FullName, memberName );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		private object [] EvaluateArguments( ArgumentList args )
		{
			return ArgumentsEvaluator.Evaluate( macroProcessor, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object EvalMemberAccessExpression( object objIn, MemberAccessExpression exp )
		{
			// ******
			//
			// expression:  macro.member
			//
			// look up member name and return the object it references
			//
			object objResult = null;
			
			var invoker = objIn as Invoker;
			if( null != invoker ) {
				objResult = invoker.Invoke();
			}
			else {
				objResult = Invokers.EvalMemberHelper( objIn, exp.MemberName, typeHelpers );
			}

			// ******
			CheckNullResult( objResult, objIn, exp.MemberName );
			return objResult;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object EvalUnnamedMemberAccessExpression( object objIn, UnnamedMemberAccessExpression exp )
		{
			// ******
			//
			// expression:  macro
			//
			// when the macro is member access then objIn is the object EXCEPT
			// for the special case of the object being a method that takes
			// no arguments and returns an object:
			//
			//		Func<object>
			//
			// which exists so we can store a property "reference" in a macro
			// and have do as expected -> return the value of the property -
			// see ETB.CreatePropertyInvoker(...)
			//
			Func<object> simpleCall = objIn as Func<object>;
			if( null != simpleCall ) {
				object objResult = simpleCall();
				CheckNullResult( objResult, objIn, exp.MemberName );
				return objResult;
			}

			return objIn;
		}


		/////////////////////////////////////////////////////////////////////////////

		//public override object EvalMethodCallExpression( object objIn, MethodCallExpression exp )
		//{
		//	// ******
		//	//
		//	// expression:  macro.method(..)
		//	//
		//	// exp.MemberName should be a method on the object passed in to us, it is the
		//	// method that we need to retreive and then invoke
		//	//
		//	object objResult = null;
		//	object [] args = EvaluateArguments( exp.Arguments );
		//	
		//	// ******
		//	Invoker invoker = Invokers.GetMethodInvoker( objIn, exp.MethodName );
		//	if( null != invoker ) {
		//		objResult = invoker.Invoke( args );
		//	}
		//	else {
		//		//
		//		// could be a "raw" delegate
		//		//
		//		var oInfo = new ObjectInfo( objIn, exp.MethodName );
		//		if( oInfo.IsDelegate ) {
		//			objResult = oInfo.GetDelegateValue( args );
		//		}
		//		else {
		//			ThreadContext.MacroError( "there is no method or delegate named \"{0}\" on the object type \"{1}\"", exp.MethodName, ObjectInfo.GetTypeName(objIn) );
		//		}
		//	}
		//
		//	// ******
		//	CheckNullResult( objResult, objIn, exp.MethodName );
		//	return objResult;
		//}


		public override object EvalMethodCallExpression( object objIn, MethodCallExpression exp )
		{
			// ******
			//
			// expression:  macro.method(..)
			//
			// exp.MemberName should be a method on the object passed in to us, it is the
			// method that we need to retreive and then invoke
			//
			object objResult = null;
			object [] args = EvaluateArguments( exp.Arguments );
			
			// ******
			//Invoker invoker = Invokers.GetMethodInvoker( objIn, exp.MethodName );
			Invoker invoker = Invokers.GetMethodInvoker( objIn, exp.MethodName, typeHelpers );
			if( null != invoker ) {
				objResult = invoker.Invoke( args );
			}
			else {
				//
				// could be a "raw" delegate
				//
				var oInfo = new ObjectInfo( objIn, exp.MethodName );
				if( oInfo.IsDelegate ) {
					objResult = oInfo.GetDelegateValue( args );
				}
				else {
					ThreadContext.MacroError( "there is no method or delegate named \"{0}\" on the object type \"{1}\"", exp.MethodName, ObjectInfo.GetTypeName(objIn) );
				}
			}

			// ******
			CheckNullResult( objResult, objIn, exp.MethodName );
			return objResult;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object EvalUnnamedMethodCallExpression( object objIn, UnnamedMethodCallExpression exp )
		{
			// ******
			//
			// expression:  macro.something( x )( y ), or macro.something[ z ]( zz )
			//
			var args = EvaluateArguments( exp.Arguments );
			object objResult = Invokers.EvalMethodCallHelper( objIn, args );
			CheckNullResult( objResult, objIn, "method result expression" );
			return objResult;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object EvalIndexExpression( object objIn, IndexExpression exp )
		{
			// ******
			//
			// expression:  macro.fieldOrProperty[ i ]
			//
			// exp.MemberName should be the name of an indexer (property with arguments)
			// that we need to access
			//
			var invoker = Invokers.GetIndexerInvoker( objIn, exp.MemberName, typeHelpers );
			if( null == invoker ) {
				ThreadContext.MacroError( "there is no indexer named \"{0}\" on the object type \"{1}\"", exp.MemberName, ObjectInfo.GetTypeName(objIn) );
			}
			object objResult = invoker.Invoke( EvaluateArguments(exp.Arguments) );

			// ******
			CheckNullResult( objResult, objIn, exp.MemberName );
			return objResult;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object EvalUnnamedIndexExpression( object objIn, UnnamedIndexExpression exp )
		{
			// ******
			//
			// expression:  macro.something[ x ][ y ], or macro.amethod( x )[ zz ]
			//
			object objResult = Invokers.EvalIndexerHelper( objIn, EvaluateArguments( exp.Arguments), typeHelpers );
			CheckNullResult( objResult, objIn, "method result expression" );
			return objResult;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object EvalActionExpression( object obj, ActionExpression exp )
		{
			throw new NotImplementedException();
		}


		/////////////////////////////////////////////////////////////////////////////

		public object Evaluate( object rootObj, Expression exp )
		{
			// ******
			try {
				return Eval( rootObj, exp );
			}
			catch ( NullValueSkipRemainderException ) {
				ThreadContext.Trace( "ObjectExpressionEvaluator.Evaluate: one of the intermediate expressions returned null" );
				return null;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object Evaluate( IMacro macro, Expression exp )
		{
			return Evaluate( macro.MacroObject, exp );
		}


		/////////////////////////////////////////////////////////////////////////////

		public ExpressionEvaluator( IMacroProcessor mp )
			:	base( mp )
		{
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object _SkipFirstAndEvaluate( IMacroProcessor mp, object rootObj, MacroExpression cexp )
		{
			// ******
			//if( ExpressionType.Compound != exp.NodeType ) {
			//	//
			//	// only the one expression
			//	//
			//	return rootObj;
			//}

			// ******
			//CompoundExpression cexp = exp.Cast<CompoundExpression>();
			ExpressionList expList = cexp.Items;
			
			if( expList.Count < 2 ) {
				//
				// only the one (or zero) expressions in the list
				//
				return rootObj;
			}

			// ******
			Expression first = expList[ 0 ];
			expList.RemoveAt( 0 );

			// ******
			var evaluator = new ExpressionEvaluator( mp );
			object result = evaluator.Evaluate( rootObj, cexp );

			// ******
			expList.Insert( 0, first );

			// ******
			return result;
		}


	}



}
