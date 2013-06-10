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
using System.Linq;
using System.Text;

using NmpBase;
using Nmp;


namespace NmpExpressions {

/*

	need to combine indexer and method calls into a single Expression
	with a flags:

		is property method

		is indexer property

			to inform macros of how they've been invoked



	since we now parse all identifiers as member access
	expressions and operations always have their own expression
	??

		member access

		operation


*/


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	interface IExpressionArguments {
		
		ArgumentList	Arguments		{ get; }

	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	public enum ExpressionType {
		
		Null,

		Expression,
		Argument,

		Compound,

		MemberAccess,		// "."
		MethodCall,			// "(...)"
		Index,					// "[...]"

		UnnamedMemberAccess,
		UnnamedMethodCall,
		UnnamedIndex,
		
		Action,					// [lhsExpression] opp rhsExpression
	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	[DebuggerDisplay("NodeType: {NodeType}")]
	public abstract class Expression {		//: IExpression {


		public const string NULL_EXPRESSION_NAME				= "null";
		public const string ARGUMENT_EXPRESSION_NAME		= "argument";
		public const string COMPOUND_EXPRESSION_NAME		= "compound";
		public const string MEMBER_EXPRESSION_NAME			= "member";
		public const string METHODCALL_EXPRESSION_NAME	= "method";
		public const string INDEX_EXPRESSION_NAME				= "index";
		public const string ACTION_EXPRESSION_NAME			= "action";


		/////////////////////////////////////////////////////////////////////////////
		
		public abstract ExpressionType NodeType { get; }
		public abstract string NodeTypeName { get; }


		/////////////////////////////////////////////////////////////////////////////

		//
		// note these do NOT disambiguate between the 'Macro' and 'normal'
		// expressions - i.e. IsMemberAccessExpression returns true for
		// both a MemberAccessExpression and a MacroMemberAccessExpression
		//

		public bool IsSingleExpression()						{ return ExpressionType.Compound != NodeType; }

		public bool IsArgumentExpression()					{ return ExpressionType.Argument == NodeType; }
		public bool IsCompoundExpression()					{ return ExpressionType.Compound == NodeType; }
		public bool IsMemberAccessExpression()			{ return ExpressionType.MemberAccess == NodeType || ExpressionType.UnnamedMemberAccess == NodeType; }
		public bool IsMethodCallExpression()				{ return ExpressionType.MethodCall == NodeType || ExpressionType.UnnamedMethodCall == NodeType; }
		public bool IsIndexExpression()							{ return ExpressionType.Index == NodeType || ExpressionType.UnnamedIndex == NodeType; }
		public bool IsActionExpression()						{ return ExpressionType.Action == NodeType; }

		public bool IsIndexOrMethodCallExpression()	{ return IsMethodCallExpression() || IsIndexExpression(); }

		/////////////////////////////////////////////////////////////////////////////
		
		public T Cast<T>() where T : class
		{
			return this as T;
		}

		
		/////////////////////////////////////////////////////////////////////////////

		public Expression()
		{
		}

	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	public class ExpressionList : List<Expression> {


		/////////////////////////////////////////////////////////////////////////////

		public ExpressionList Clone()
		{
			return (ExpressionList) MemberwiseClone();
		}


		/////////////////////////////////////////////////////////////////////////////

		public ExpressionList()
		{
			
		}


		/////////////////////////////////////////////////////////////////////////////

		public ExpressionList( IEnumerable<Expression> collection )
			: base( collection )
		{
			
		}
	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	public class ArgumentList : List<ArgumentExpression> {

		object [] preProcessedArgs = null;


		/////////////////////////////////////////////////////////////////////////////

		public object [] PreProcessedArgs	{ get { return preProcessedArgs; } }


		/////////////////////////////////////////////////////////////////////////////

		public ArgumentList( object [] preProcessedArgs )
		{
			this.preProcessedArgs = preProcessedArgs;
		}


		/////////////////////////////////////////////////////////////////////////////

		public ArgumentList()
		{
		}


		/////////////////////////////////////////////////////////////////////////////

		public ArgumentList( IEnumerable<ArgumentExpression> collection )
			: base( collection )
		{
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		//public static ArgumentList GetArgumentList( IMacroProcessor mp, NmpStringList strList, Expression parentExp = null )
		//{
		//	// ******
		//	if( null == parentExp ) {
		//		parentExp = new NullExpression();
		//	}
		//
		//	// ******
		//	return mp.Get<ArgumentsProcessor>().ProcessArgumentList( parentExp, strList );
		//}
		//

	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	[DebuggerDisplay( "Null Expression" )]
	class NullExpression : Expression {

		public override ExpressionType NodeType { get { return ExpressionType.Null; } }
		public override string NodeTypeName { get { return NULL_EXPRESSION_NAME; } }

		/////////////////////////////////////////////////////////////////////////////

		public NullExpression()
		{
		}

	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	public enum ArgumentType {
		
		Constant,					// an obj value calculated at parse time
		Expression,				// a macro expression to be evaluated at eval time, requires macro name
		Convert,					// like expression (above) but needs to be cast after evaluation

	}

	[DebuggerDisplay( "ArgumentType: {_argumentType}" )]
	public class ArgumentExpression : Expression {

		public override ExpressionType NodeType { get { return ExpressionType.Argument; } }
		public override string NodeTypeName { get { return ARGUMENT_EXPRESSION_NAME; } }

		Expression		_parent;
		ArgumentType	_argumentType;
		string				_macroName;
		Expression		_expression;
		object				_constValue;
		Type					_castToType;


		/////////////////////////////////////////////////////////////////////////////

		public Expression			Parent							{ get { return _parent; } }
		public ArgumentType		ArgumentType				{ get { return _argumentType; } }
		public string					Name								{ get { return _macroName; } }
		public object					ConstantValue				{ get { return _constValue; } }
		public Type						ConstantValueType		{ get { return ObjectInfo.GetObjectType(_constValue); } }
		public Expression			Expression					{ get { return _expression; } }
		public ExpressionType	ExpressionType			{ get { return _expression.NodeType; } }
		public Type						ExpressionCastType	{ get { return _castToType; } }


		/////////////////////////////////////////////////////////////////////////////

		public void SetCastType( Type castToType )
		{
			// ******
			if( ArgumentType.Constant == _argumentType ) {
				throw new Exception( "attemping to cast a constant ArgumentExpression" );
			}

			// ******
			_castToType = castToType;
			_argumentType = ArgumentType.Convert;
		}


		/////////////////////////////////////////////////////////////////////////////

		public ArgumentExpression( Expression parent, object constValue )
		{
			_parent = parent;
			_argumentType = ArgumentType.Constant;
			_constValue = constValue;
	 	}


		/////////////////////////////////////////////////////////////////////////////

		public ArgumentExpression( Expression parent, string name, Expression expression )
		{
			_parent = parent;
			_argumentType = ArgumentType.Expression;
			_macroName = name;
			_expression = expression;
		}


		/////////////////////////////////////////////////////////////////////////////

		public ArgumentExpression( Expression parent, string name, Expression expression, Type castType )
		{
			_parent = parent;
			_argumentType = ArgumentType.Convert;
			_macroName = name;
			_expression = expression;
			_castToType = castType;
		}

	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	public class MacroExpression : CompoundExpression {

		public NmpStringList MacroInstructions { get; private set; }

		/////////////////////////////////////////////////////////////////////////////

		public MacroExpression()
		{
			MacroInstructions = new NmpStringList();
		}

		/////////////////////////////////////////////////////////////////////////////

		public MacroExpression( NmpStringList instructions )
		{
			MacroInstructions = instructions;
		}
	}


  /////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	[DebuggerDisplay( "Items.Count: {_items.Count}" )]
	public class CompoundExpression : Expression {

		public override ExpressionType NodeType { get { return ExpressionType.Compound; } }
		public override string NodeTypeName { get { return COMPOUND_EXPRESSION_NAME; } }


		/////////////////////////////////////////////////////////////////////////////

		public CompoundExpression Clone()
		{
			var ce = (CompoundExpression) MemberwiseClone();
			ce._items = new ExpressionList(_items);
			return ce;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected ExpressionList _items;

		public ExpressionList Items
		{
			get
			{
				return _items;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public int Count
		{
			get
			{
				return _items.Count;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public Expression this [ int index ]
		{
			get
			{
				return _items [ index ];
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Add( Expression exp )
		{
			_items.Add( exp );
		}


		/////////////////////////////////////////////////////////////////////////////

		public CompoundExpression()
		{
			_items = new ExpressionList();
		}

	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	class MemberAccessExpression : Expression {

		public override ExpressionType	NodeType	{ get { return ExpressionType.MemberAccess; } }
		public override string NodeTypeName { get { return MEMBER_EXPRESSION_NAME; } }

		//
		// isMacroNode indicates that when this node is evaluated the
		// object passed to the evaluator is the object that is being
		// accessed (and should be returned) - otherwise MemberName should 
		// be looked up on the object (passed in) and returned
		//
		public bool IsMacroNode		{ get; set; }

		/////////////////////////////////////////////////////////////////////////////

		protected string _memberName = string.Empty;

		public string MemberName
		{
			get {
				return _memberName;
			}
		}

		/////////////////////////////////////////////////////////////////////////////

		public MemberAccessExpression( bool isMacroNode, string memberName )
		{
			// ******
			IsMacroNode = isMacroNode;

			// ******
			if( string.IsNullOrEmpty(memberName) ) {
				throw new ArgumentNullException( "memberName" );
			}

			// ******
			_memberName = memberName;
		}

	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	class UnnamedMemberAccessExpression : MemberAccessExpression {

		public override ExpressionType	NodeType	{ get { return ExpressionType.UnnamedMemberAccess; } }

		/////////////////////////////////////////////////////////////////////////////

		public UnnamedMemberAccessExpression( string memberName )
			: base( true, memberName )
		{
		}

	}


	/////////////////////////////////////////////////////////////////////////////

	class MethodCallExpression : Expression, IExpressionArguments {

		public override ExpressionType	NodeType	{ get { return ExpressionType.MethodCall; } }
		public override string NodeTypeName { get { return METHODCALL_EXPRESSION_NAME; } }

		//
		// is macro node indicates that when this node is evaluated the
		// object passed to the evaluator is a MethodPackage or delegate
		// that should be called - otherwise MethodName is looked up on
		// the input object and called
		//
		public string				MethodName	{ get; private set; }
		public ArgumentList	Arguments		{ get; private set; }


		///////////////////////////////////////////////////////////////////////////////
		//
		//public MethodCallExpression( IArgumentsProcessor ap, NmpStringList strArgs )
		//{
		//	MethodName = string.Empty;
		//	Arguments = ap.ProcessArgumentList( this, strArgs );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public MethodCallExpression( string methodName, IArgumentsProcessor ap, NmpStringList strArgs )
		{
			MethodName = methodName;
			Arguments = ap.ProcessArgumentList( this, strArgs );
		}


		/////////////////////////////////////////////////////////////////////////////

		public MethodCallExpression( string methodName, ArgumentList args )
		{
			MethodName = methodName;
			Arguments = args;
		}
	}


	/////////////////////////////////////////////////////////////////////////////
	//
	// index the result of a previous index expression by invoking its
	// default indexer
	//
	/////////////////////////////////////////////////////////////////////////////

	class UnnamedMethodCallExpression : Expression, IExpressionArguments {
		public override string NodeTypeName { get { return METHODCALL_EXPRESSION_NAME; } }

		// ******
		public override ExpressionType	NodeType		{ get { return ExpressionType.UnnamedMethodCall; } }

		// ******
		public string				MacroName			{ get; private set; }
		public bool					IsMacroObject	{ get { return ! string.IsNullOrEmpty(MacroName); } }
		public ArgumentList	Arguments			{ get; private set; }


		/////////////////////////////////////////////////////////////////////////////
		
		public UnnamedMethodCallExpression( ArgumentList argList )
		{
			MacroName = string.Empty;
			Arguments = argList;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public UnnamedMethodCallExpression( IArgumentsProcessor ap, NmpStringList strArgs )
		{
			MacroName = string.Empty;
			Arguments = ap.ProcessArgumentList( this, strArgs );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public UnnamedMethodCallExpression( string macroName, IArgumentsProcessor ap, NmpStringList strArgs )
			:	this( ap, strArgs )
		{
			MacroName = macroName;
		}

	}


//	/////////////////////////////////////////////////////////////////////////////
//	//
//	//
//	//
//	/////////////////////////////////////////////////////////////////////////////
//
//	class MacroMethodCallExpression : MethodCallExpression {
//
//		public override ExpressionType	NodeType	{ get { return ExpressionType.MacroMethodCall; } }
//
//		/////////////////////////////////////////////////////////////////////////////
//
//		public MacroMethodCallExpression( string methodName, IArgumentsProcessor ap, NmpStringList strArgs )
//			: base( methodName, ap, strArgs )
//		{
//		}
//
//	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	class IndexExpression : Expression, IExpressionArguments {

		public override ExpressionType	NodeType	{ get { return ExpressionType.Index; } }
		public override string NodeTypeName { get { return INDEX_EXPRESSION_NAME; } }

		public string				MemberName	{ get; private set; }
		public ArgumentList	Arguments		{ get; private set; }


		///////////////////////////////////////////////////////////////////////////////
		//
		//public IndexExpression( IArgumentsProcessor ap, NmpStringList strArgs )
		//{
		//	MemberName = string.Empty;
		//	Arguments = ap.ProcessArgumentList( this, strArgs );
		//}
		//
		
		/////////////////////////////////////////////////////////////////////////////

		public IndexExpression( string methodName, IArgumentsProcessor ap, NmpStringList strArgs )
		{
			// ******
			MemberName = methodName;
			Arguments = ap.ProcessArgumentList( this, strArgs );
		}
	}


	/////////////////////////////////////////////////////////////////////////////
	//
	// index the result of a previous index expression by invoking its
	// default indexer
	//
	/////////////////////////////////////////////////////////////////////////////

	class UnnamedIndexExpression : Expression, IExpressionArguments {

		// ******
		public override ExpressionType	NodeType		{ get { return ExpressionType.UnnamedIndex; } }
		public override string NodeTypeName { get { return INDEX_EXPRESSION_NAME; } }

		// ******
		public string				MacroName			{ get; private set; }
		public bool	 				IsMacroObject	{ get { return ! string.IsNullOrEmpty(MacroName); } }
		public ArgumentList	Arguments			{ get; private set; }


		/////////////////////////////////////////////////////////////////////////////
		
		public UnnamedIndexExpression( IArgumentsProcessor ap, NmpStringList strArgs )
		{
			// ******
			MacroName = string.Empty;
			Arguments = ap.ProcessArgumentList( this, strArgs );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public UnnamedIndexExpression( string macroName, IArgumentsProcessor ap, NmpStringList strArgs )
			:	this( ap, strArgs )
		{
			MacroName = macroName;
		}

	}


//	/////////////////////////////////////////////////////////////////////////////
//	//
//	//
//	//
//	/////////////////////////////////////////////////////////////////////////////
//
//	class MacroIndexExpression : IndexExpression {
//
//		public override ExpressionType	NodeType	{ get { return ExpressionType.MacroIndex; } }
//
//		/////////////////////////////////////////////////////////////////////////////
//
//		public MacroIndexExpression( string methodName, IArgumentsProcessor ap, NmpStringList strArgs )
//			: base( methodName, ap, strArgs )
//		{
//		}
//
//	}
//

	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	class ActionExpression : Expression {

		public override ExpressionType	NodeType	{ get { return ExpressionType.Action; } }
		public override string NodeTypeName { get { return ACTION_EXPRESSION_NAME; } }


		/////////////////////////////////////////////////////////////////////////////

		protected Expression _lhsExpression = null;

		public Expression Lhs
		{
			get {
				return _lhsExpression;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected Expression _rhsExpression = null;

		public Expression Rhs
		{
			get {
				return _rhsExpression;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public ActionExpression( bool isRootNode )
		{
		}

	}


}
