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
using System.Linq;
using System.Text;


#pragma warning disable 414


using NmpBase;
using Nmp;


namespace NmpExpressions {		//ExpressionDump {



	/////////////////////////////////////////////////////////////////////////////

	abstract class ExpDumpBase {

		// ******
		public abstract string DumpCompoundExpression( CompoundExpression exp );

		public abstract string DumpMemberAccessExpression( MemberAccessExpression exp );
		public abstract string DumpMacroMemberAccessExpression( UnnamedMemberAccessExpression exp );

		public abstract string DumpMethodCallExpression( MethodCallExpression exp );
		public abstract string DumpUnnamedMethodCallExpression( UnnamedMethodCallExpression exp );

		public abstract string DumpIndexExpression( IndexExpression exp );
		public abstract string DumpUnnamedIndexExpression( UnnamedIndexExpression exp );

		public abstract string DumpActionExpression( ActionExpression exp );


		/////////////////////////////////////////////////////////////////////////////

		public string Process( Expression exp )
		{
			// ******
			string result = string.Empty;

			// ******
			switch( exp.NodeType ) {
				case ExpressionType.Compound:
					StringBuilder sb = new StringBuilder();

					//foreach( Expression ex in exp.AsCompound().Items ) {
					foreach( Expression ex in exp.Cast<CompoundExpression>().Items ) {
						sb.Append( Process(ex) );
					}

					result = sb.ToString();
					break;

				case ExpressionType.MemberAccess:
					result = DumpMemberAccessExpression( exp.Cast<MemberAccessExpression>() );
					break;

				case ExpressionType.UnnamedMemberAccess:
					result = DumpMacroMemberAccessExpression( exp.Cast<UnnamedMemberAccessExpression>() );
					break;

				case ExpressionType.MethodCall:
					result = DumpMethodCallExpression( exp.Cast<MethodCallExpression>() );
					break;

				//case ExpressionType.MacroMethodCall:
				//	result = DumpUnnamedMethodCallExpression( exp.Cast<UnnamedMethodCallExpression>() );
				//	break;

				case ExpressionType.Index:
					result = DumpIndexExpression( exp.Cast<IndexExpression>() );
					break;

				//case ExpressionType.MacroIndex:
				//	result = DumpUnnamedIndexExpression( exp.Cast<UnnamedIndexExpression>() );
				//	break;

				case ExpressionType.Action:
					//result = DumpActionExpression( exp.AsAction() );
					result = DumpActionExpression( exp.Cast<ActionExpression>() );
					break;

				default:
					throw ExceptionHelpers.CreateException( "unknown expression type \"{0}\"", exp.NodeType.ToString() );
			}

			// ******
			return result;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string Dump( Expression exp )
		{
			return Process( exp );
		}


		/////////////////////////////////////////////////////////////////////////////

		public ExpDumpBase()
		{
		}

	}


	/////////////////////////////////////////////////////////////////////////////

	class DisplayExpression : ExpDumpBase {


		/////////////////////////////////////////////////////////////////////////////

		private string AddDot( bool isRootNode )
		{
			return isRootNode ? "" : ".";
		}


		/////////////////////////////////////////////////////////////////////////////

		private string DumpArgumentConstant( ArgumentExpression arg )
		{
			// ******
			string value = string.Empty;
			string typeName = string.Empty;
		
			// ******
			if( null == arg.ConstantValue ) {
				value = "object";
				typeName = "null";
			}
			else {
				value = arg.ConstantValue.ToString();
				typeName = arg.ConstantValueType.Name;
			}
		
			// ******
			return string.Format( " ({0}) {1}", typeName, value );
		}


		/////////////////////////////////////////////////////////////////////////////

		private string DumpArgumentExpression( ArgumentExpression arg )
		{
			return Process( arg.Expression );
		}


		/////////////////////////////////////////////////////////////////////////////

		private string DumpArgumentConvert( ArgumentExpression arg )
		{
			// ******
			string typeName = arg.ExpressionCastType.Name;
			string value = Process( arg.Expression );
		
			// ******
			return string.Format( " ({0}) {1}", typeName, value );
		}


		/////////////////////////////////////////////////////////////////////////////

		private string Arguments( ArgumentList args )
		{
			// ******
			StringBuilder sb = new StringBuilder();

			// ******
			for( int iArg = 0; iArg < args.Count; iArg++ ) {
				ArgumentExpression arg = args[ iArg ];

				// ******
				if( iArg > 0 ) {
					sb.Append( ',' );
				}

				string str = "????";

				switch( arg.ArgumentType ) {
					case ArgumentType.Constant:
						str = DumpArgumentConstant( arg );
						break;

					case ArgumentType.Expression:
						str = DumpArgumentExpression( arg );
						break;

					case ArgumentType.Convert:
						str = DumpArgumentConvert( arg );
						break;
				}

				sb.Append( str );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public override string DumpCompoundExpression( CompoundExpression exp )
		{
			// ******
			string typeName = "ExpressionList";

			// ******
			DisplayExpression de = new DisplayExpression();
			string value = de.Dump( exp );

			// ******
			return string.Format( " ({0}) {1}", typeName, value );
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public override string DumpMacroExpression( MacroExpression exp )
		//{
		//	return exp.MacroName;
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public override string DumpMemberAccessExpression( MemberAccessExpression exp )
		{
			return string.Format( ".{0}", exp.MemberName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override string DumpMacroMemberAccessExpression( UnnamedMemberAccessExpression exp )
		{
			return string.Format( "{0}", exp.MemberName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override string DumpMethodCallExpression( MethodCallExpression exp )
		{
			return string.Format( ".{0}({1})", exp.MethodName, Arguments(exp.Arguments) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override string DumpUnnamedMethodCallExpression( UnnamedMethodCallExpression exp )
		{
			return string.Format( "macro|()result|[]result({0})", Arguments(exp.Arguments) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override string DumpIndexExpression( IndexExpression exp )
		{
			return string.Format( ".{0}[{1}]", exp.MemberName, Arguments(exp.Arguments) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override string DumpUnnamedIndexExpression( UnnamedIndexExpression exp )
		{
			return string.Format( "macro|[]result|()result[{0}]", Arguments(exp.Arguments) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override string DumpActionExpression( ActionExpression exp )
		{
			return string.Format( "ActionExpression\n" );
		}


		/////////////////////////////////////////////////////////////////////////////

		public DisplayExpression()
		{
		}

	}


}


/////////////////////////////////////////////////////////////////////////////

namespace NmpExpressions {


	/////////////////////////////////////////////////////////////////////////////

	class ExpressionDumper : IExpressionEvaluator {

		IMacroProcessor macroProcessor;


		/////////////////////////////////////////////////////////////////////////////

		public object Evaluate( IMacro macro, Expression exp )
		{
			DisplayExpression dumper = new DisplayExpression();
			return dumper.Dump( exp );
		}


		/////////////////////////////////////////////////////////////////////////////

		public ExpressionDumper( IMacroProcessor mp )
		{
			macroProcessor = mp;
		}


	}



}
