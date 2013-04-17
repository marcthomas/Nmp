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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Reflection;


//#pragma warning disable 169

using NmpBase;
using Nmp.Expressions;


namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	[DebuggerDisplay( "[] = {finalArgs}" )]
	class MacroArgs : IEnumerable {

		// ******
		readonly IMacroProcessor	macroProcessor;
		readonly string	macroName;

		// ******
		readonly List<Type> _expectedArgTypes = null;
		readonly List<object> _argDefaults = null;

		// ******
		object [] finalArgs = null;


		/////////////////////////////////////////////////////////////////////////////

		public IEnumerator GetEnumerator() {
			for( int i = 0; i < finalArgs.Length; i++ ) {
				yield return this[ i ];
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		IEnumerator IEnumerable.GetEnumerator() {
				return this.GetEnumerator();
		}


		/////////////////////////////////////////////////////////////////////////////

		int iIndexIterator = 0;

		public object First()
		{
			iIndexIterator = 0;
			return Next();
		}


		/////////////////////////////////////////////////////////////////////////////

		public object Next()
		{
			if( null == finalArgs || iIndexIterator >= finalArgs.Length ) {
				throw new ArgumentOutOfRangeException( "iIndexIterator" );
			}

			return finalArgs[ iIndexIterator++ ];
		}


		/////////////////////////////////////////////////////////////////////////////

		public int Count
		{
			get {
				return null == finalArgs ? 0 : finalArgs.Length;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object this [ int index ]
		{
			[DebuggerStepThrough]
			get
			{
				if( null == finalArgs ) {
					throw new Exception( "finalArgs has not been initialized" );
				}

				// ******
				if( index < 0 || index >= finalArgs.Length ) {
					throw new ArgumentOutOfRangeException( "finalArgs index" );
				}

				// ******
				return finalArgs[ index ];
			}
		}


		///////////////////////////////////////////////////////////////////////////////

		public string AsString ( int index )
		{
			// ******
			object obj = this [ index ];
			return obj.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		protected object [] ConvertArgs( IList<object> argsList )
		{
			// ******
			//
			// we ignore too many args but error out if there
			// are too few
			//
			//if( argsList.Length < _argTypes.Length ) {
			//	ThreadContext.MacroError( "not enough arguments to macro {0}, expected {1} got {2}, required argument types are: {3}", macroName, _argTypes.Length, argsList.Length, Arguments.ArgumentsTypeNames(_argTypes) );
			//}

			// ******
			var args = Invoker.MatchBuiltinMacroArgs( macroName, null, _expectedArgTypes, argsList, true );
			if( null == args ) {
				//
				// we did not check the number of args above because the rules allow the final arg to be "empty" if
				// it's an objec or string array - if we'd checked arg count we would never have called MatchArgs()
				// if we fail then we can say not enough arguments
				//
				if( argsList.Count < _expectedArgTypes.Count ) {
					ThreadContext.MacroError( "not enough arguments to macro {0}, expected {1} got {2}, required argument types are: {3}", macroName, _expectedArgTypes.Count, argsList.Count, Arguments.ArgumentsTypeNames( _expectedArgTypes ) );
				}

				//
				// otherwise
				//
				ThreadContext.MacroError( "unable to convert arguments for macro {0}", macroName );
			}

			return args;
		}


		/////////////////////////////////////////////////////////////////////////////

		//protected IList<object> AddOptionalArgs( object [] args )
		//{
		//	// ******
		//	if( null == _argDefaults || 0 == _argDefaults.Count ) {
		//		return args;
		//	}

		//	//
		//	// FixArguments
		//	//
		//	// if fewer args than _argDefaults then add _argDefault to args
		//	// starting in _argDefaults where args leaves off; i.e. if 2
		//	// args are supplied and there are 3 defaults then add the 3'd
		//	// default arg to args
		//	//
		//	// if there are more args that defaults then add the types of the
		//	// additional arguments to _expectedArgTypes
		//	//

		//	// ?? mismatch in expected types and defaults

		//	// ******
		//	//
		//	// save for later
		//	//
		//	int numRequiredArgs = _expectedArgTypes.Count;
		//	int optionalArgsProvided = args.Length - numRequiredArgs;

		//	// ******
		//	//
		//	// add default argument types to _argTypes
		//	//
		//	foreach( object o in _argDefaults ) {
		//		_expectedArgTypes.Add( o.GetType() );
		//	}

		//	// ******
		//	if( args.Length >= _expectedArgTypes.Count ) {
		//		//
		//		// no defaults required, the caller supplied them all
		//		//
		//		return args;
		//	}

		//	// ******
		//	//
		//	// add default value for any arguments not provided
		//	//
		//	// ******

		//	//
		//	// copy args to a list that we can add to
		//	//
		//	var argList = new NmpObjectList( args );

		//	if( optionalArgsProvided >= 0 ) {
		//		for( int i = optionalArgsProvided; i < _argDefaults.Count; i++ ) {
		//			argList.Add( _argDefaults [ i ] );
		//		}
		//	}

		//	// ******
		//	return argList;
		//}


		protected IList<object> AddOptionalArgs( object [] args )
		{
			// ******
			if( null == _argDefaults || 0 == _argDefaults.Count ) {
				return args;
			}

			//
			// FixArguments
			//

			// ******
			//int numRequiredArgs = _expectedArgTypes.Count;

			/*
			 * 
			 * ?? _argDefaults.Count == _expectedArgTypes.Count or no default args ??
			 * 
			 * */

			var argList = new NmpObjectList( args );
			int numRequiredArgs = _argDefaults.Count;

			if( numRequiredArgs > args.Length ) {
				//
				// have default arguments to add to args
				//
				for( int i = args.Length; i < numRequiredArgs; i++ ) {
					//
					// we could check to see if the default value type matches the
					// expected type in _expectedArgTypes BUT we dont want to do
					// that because we try to coerce values when matching args
					// to method signatures
					//
					argList.Add( _argDefaults [ i ] );
				}
			}
			else if( args.Length > numRequiredArgs ) {
				//
				// additional arguments over and above those required
				// have been provided by the caller
				//
				// generaly speaking this should only happen for "extra"
				// args that combined into a string or object array passed
				// to the method being called as the last argument
				//
				// we need to do nothing here because ConvertArgs() will allow
				// the extra args to be truncated, or gather the extra args into
				// a string or object array if the last expected argument is
				// string[] or object[]
			}

			// ******
			return argList;
		}


		/////////////////////////////////////////////////////////////////////////////

		public MacroArgs Initialize( ArgumentList argList )
		{
			// ******
			//
			// first evaluate the arguments (process expressions)
			//
			object [] args = ArgumentsEvaluator.Evaluate( macroProcessor, argList );

			// ******
			if( null == _expectedArgTypes ) {
				//
				// no conversions, no optional args - just the arguments in the expression
				//
				finalArgs = args;
			}
			else {
				//
				// now add any optional arguments that were not supplied in
				// argList
				//
				var objList = AddOptionalArgs( args );

				// ******
				//
				// convert args
				//
				finalArgs = ConvertArgs( objList );
			}

			// ******
			return this;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object AsTuples( ArgumentList argList )
		{
			// ******
			//
			// this either succeeds or never returns
			//
			Initialize( argList );

			// ******
			int count = finalArgs.Length;
			if( count < 1 || count > 7 ) {
				ThreadContext.MacroError( "MarcoArgs.AsTuples only supports 1 to 7 values, the macro \"{0}\" requested {1} values", macroName, count );
			}

			////var t = new NmpTuple<int, int, int, int, int, int, int>();
			//var t = new NmpTuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7);

			// ******
			//Type type = Type.GetType( string.Format("System.Tuple`{0}", count) );
			//Type type = Type.GetType( string.Format( "NmpBase.NmpTuple`{0}", count ) );

			Type type = Type.GetType( string.Format( "NmpTuple`{0},NmpBase", count ) );
			Type constructedType = type.MakeGenericType( _expectedArgTypes.ToArray() );
			object tuple = Activator.CreateInstance( constructedType, finalArgs );

			// ******
			return tuple;
		}


		/////////////////////////////////////////////////////////////////////////////

		List<T> NewList<T>( IEnumerable<T> items )
		{
			return null == items ? new List<T>() : new List<T>( items );
		}


		/////////////////////////////////////////////////////////////////////////////

		public MacroArgs( IMacroProcessor mp, ArgumentList argList, string macroName, IEnumerable<Type> argTypes, IEnumerable<object> defaultArgs )
		{
			// ******
			this.macroProcessor = mp;

			// ******
			this.macroName = macroName;
			this._expectedArgTypes = NewList( argTypes );
			this._argDefaults = NewList( defaultArgs );

			if( _argDefaults.Count > 0 && _argDefaults.Count != _expectedArgTypes.Count ) {
				throw new ArgumentOutOfRangeException( "defaultArgs", "defaultArgs null, empty or match the count of argTypes" );
			}

			// ******
			Initialize( argList );
		}


		/////////////////////////////////////////////////////////////////////////////

		public MacroArgs( IMacroProcessor mp, string macroName, IEnumerable<Type> argTypes, IEnumerable<object> defaultArgs )
		{
			// ******
			this.macroProcessor = mp;

			// ******
			this.macroName = macroName;
			this._expectedArgTypes = NewList( argTypes );
			this._argDefaults = NewList( defaultArgs );

			if( _argDefaults.Count > 0 && _argDefaults.Count != _expectedArgTypes.Count ) {
				throw new ArgumentOutOfRangeException( "defaultArgs", "defaultArgs null, empty or match the count of argTypes" );
			}

		}

	}


}
