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

#pragma warning disable 169

namespace Nmp.Builtin.Macros {


	/////////////////////////////////////////////////////////////////////////////

	class ForeachHandler {

		IMacroProcessor mp;


		/////////////////////////////////////////////////////////////////////////////

		protected static void AddExtraArgs( int startIndex, object [] args, object [] extraArgs )
		{
			for( int i = startIndex; i < args.Length; i++ ) {
				args[ i ] = extraArgs[ i - startIndex ];
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static IEnumerable GetIterableObject( object obj, out int itemsCount )
		{
			//
			// get 'obj' as an IEnumerable, and get the number of items
			//
			// if 'obj' is not enumerable then the 'obj' will be placed in an array
			// with 'obj' (count will be 1)
			//

			// ******
			IEnumerable items = obj as IEnumerable;
			if( null == items ) {
				//
				// if the object does not support enumeration then we'll
				// put it into something that does
				//
				items = new object [] { obj };
			}

			// ******
			ICollection col = items as ICollection;
			itemsCount = null == col ? -1 : col.Count;

			return items;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string ForeachObjectMacro( IMacro target, object objToEnumerate, object [] extraArgs )
		{
			// ******
			int itemsCount;
			IEnumerable items = GetIterableObject( objToEnumerate, out itemsCount );

			// ******
			StringBuilder sb = new StringBuilder();

			// ******
			object [] argsToMacro = new object[ 5 + extraArgs.Length ];
			AddExtraArgs( 5, argsToMacro, extraArgs );

			// ******
			MacroExpression expression = ETB.CreateMacroCallExpression( target, argsToMacro );

			// ******
			int itemsLastIndex = itemsCount > 0 ? itemsCount - 1 : -1;
			int index = 0;

			foreach( object item in items ) {
				//
				//	list: `value', `index', `lastIndex', `count', `type'
				//
				argsToMacro[ 0 ] = item;
				argsToMacro[ 1 ] = index++;
				argsToMacro[ 2 ] = itemsLastIndex;
				argsToMacro[ 3 ] = itemsCount;
				argsToMacro[ 4 ] = item.GetType();

				// ******
				sb.Append( mp.InvokeMacro( target, expression, true) );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public string ForeachTextMacro( IMacro target, object objToEnumerate, object [] extraArgs )
		{
			// ******
			int itemsCount;
			IEnumerable items = GetIterableObject( objToEnumerate, out itemsCount );

			// ******
			StringBuilder sb = new StringBuilder();

			// ******
			//
			// temporary macro
			//
			IMacro tempMacro = mp.CreateObjectMacro( mp.GenerateMacroName("foreach"), null );
			mp.AddMacro( tempMacro );
			
			// ******
			//for( int i = 0; i < extraArgs.Length; i++ ) {
			//	extraArgs[ i ] = extraArgs[ i ].ToString();
			//}

			object [] argsToMacro = new object[ 5 + extraArgs.Length ];
			AddExtraArgs( 5, argsToMacro, extraArgs );

			// ******
			MacroExpression expression = ETB.CreateMacroCallExpression( target, argsToMacro );

			// ******
			int itemsLastIndex = itemsCount > 0 ? itemsCount - 1 : -1;
			int index = 0;


			foreach( object item in items ) {
				//
				//	list: `value', `index', `lastIndex', `count', `type'
				//

				// ******
				//
				// assign item as the macro object
				//
				string itemStr = item as string;
				if( null != itemStr ) {
					argsToMacro [ 0 ] = itemStr;
					argsToMacro[ 4 ] = "System.String";	//typeof( string );
				}
				else {
					tempMacro.MacroObject = item;
					argsToMacro[ 0 ] = tempMacro.Name;
					argsToMacro[ 4 ] = item.GetType().ToString().Replace( '`', '^' );
				}

				// ******
				argsToMacro[ 1 ] = index++;
				argsToMacro[ 2 ] = itemsLastIndex;
				argsToMacro[ 3 ] = itemsCount;

				// ******
				sb.Append( mp.InvokeMacro(target, expression, true) );
			}

			// ******
			//
			// get rid of the temp macro
			//
			mp.DeleteMacro( tempMacro );

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public string Foreach_TextChunk( object objToEnumerate, string macroText, object [] extraArgs )
		{
			// ******
			//
			//	list: `value', `index', `lastIndex', `count', `type'
			//
			var argNames = new NmpStringList( "value", "index", "lastIndex", "count", "type" );

			//
			// extra0 ... extraN
			//
			for( int i = 0; i < extraArgs.Length; i++ ) {
				argNames.Add( string.Format( "extra{0}", i ) );
			}

			// ******
			IMacro target = mp.AddTextMacro( mp.GenerateMacroName("$.foreach"), macroText, argNames );
			string result = ForeachTextMacro( target, objToEnumerate, extraArgs );
			mp.DeleteMacro( target );

			// ******
			return result;
		}


		/////////////////////////////////////////////////////////////////////////////

		public ForeachHandler( IMacroProcessor mp )
		{
			this.mp = mp;
		}

	}
}
