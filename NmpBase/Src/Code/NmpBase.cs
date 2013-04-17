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



namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////

	//public delegate void ErrorCall( string frm, params object [] args );
	public delegate void WarningCall( string frm, params object [] args );



	/////////////////////////////////////////////////////////////////////////////

	//
	// used by a ParseReader
	//

	public interface IBaseReader {

		// ******
		string	Buffer					{ get; }

		// ******
		string	SourceName			{ get; }
		string	DefaultPath			{ get; }

		/*
			__our__ pushback handles stopping line counting when a pushback is active
		*/

		int			PushedbackCount	{ get; }
		int			Count						{ get; }
		int			Pos							{ get; }		
		int			RemainderCount	{ get; }
		string	Remainder				{ get; }

		int			Line						{ get; }
		int			Column					{ get; }

		// ******
		char	LastChar	{ get; }

		char 	this[ int reqIndex ]	{ get; }

		int		BackupOne();
		char	Peek( int index );
		char	Peek();
		string	PeekNext( int count );

		char		Next();
		char		Next( out bool wasPushedBack );
		string	Next( int count );

		string	GetText( int start, int end );

		// ******
		//void Pushback( string [] text );
		void	Pushback( string text );

		void	Reset();

	}


	/////////////////////////////////////////////////////////////////////////////

	public interface IParseReader {

		//
		// this interface gets passed around to code that does not have access
		// to ParseReader
		//

		bool		CheckEscapesAndSpecialChars	{ get; set; }

		int			Index						{ get; }		
		int			RemainderCount	{ get; }
		bool		AtEnd						{ get; }

		bool 		Matches( int startIndex, string cmpStr );
		bool		StartsWith( string cmpStr );
		bool		StartsWith( string cmpStr, bool ignoreCase );

		void		Skip( int n );
		char		Peek( int index );
		char		Peek();
		char		Next();
		string	Next( int count );

		string	GetText( int start, int end );

	}


	/////////////////////////////////////////////////////////////////////////////

	//
	// used by ItemGetter in NetObjectHelpers.cs to register helper classes
	//

	public class TypeHelperDictionary : Dictionary<Type, Func<object, object>> {


		/////////////////////////////////////////////////////////////////////////////

		public new void Add( Type type, Func<object, object> func )
		{
			if( ContainsKey( type ) ) {
				Remove( type );
			}
			this[ type ] = func;

		}

		/////////////////////////////////////////////////////////////////////////////

		public Func<object, object> FindHelper( Type type )
		{
			// ******
			Func<object, object> makeObjHelper;
			if( TryGetValue(type, out makeObjHelper) ) {
				return makeObjHelper;
			}
			
			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object GetHelper( object anObj )
		{
			// ******
			var makeHelper = FindHelper( anObj.GetType() );
			if( null != makeHelper ) {
				return makeHelper( anObj );
			}
			
			// ******
			return null;
		}

	}



	/////////////////////////////////////////////////////////////////////////////

	public class EnumerationWrapper<T> : IEnumerable<T> {

		ICollection<T> items;
		Func<T, T> processItem;

		/////////////////////////////////////////////////////////////////////////////

		IEnumerator IEnumerable.GetEnumerator() {
				return this.GetEnumerator();
		}


		/////////////////////////////////////////////////////////////////////////////

		public IEnumerator<T> GetEnumerator() {
			foreach( var item in items ) {
				yield return processItem(item);
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public EnumerationWrapper( ICollection<T> items, Func<T, T> processItem = null )
		{
			this.items = items;
			this.processItem = null != processItem ? processItem : item => item;
		}

	}


	///////////////////////////////////////////////////////////////////////////////
	//
	//public sealed class Singleton {
	//	private static readonly Singleton instance = new Singleton();
	//
	//	private Singleton(){}
	//
	//	public static Singleton Instance
	//	{
	//		get {
	//			return instance; 
	//		}
	//	}
	//}
	//

	///////////////////////////////////////////////////////////////////////////////
	//
	//class ThreadedSingleton {
	//
	//	[ThreadStatic]
	//	static ThreadedSingleton instance;
	//
	//	private ThreadedSingleton()
	//	{
	//		//Console.WriteLine("Creating instance in thread "+Thread.CurrentThread.Name);
	//	}
	//
	//	public static ThreadedSingleton Instance
	//	{
	//		get {
	//			if( instance==null ) {
	//				instance = new ThreadedSingleton();
	//			}
	//		
	//			return instance;
	//		}
	//	}
	//}
	//

	///////////////////////////////////////////////////////////////////////////////
	//	
	//public class C1<T> : IEnumerable<T> {
	//
	//	private T[] items;
	//
	//	public C1( T[] items ) {
	//			this.items = items;
	//	}
	//
	//	public IEnumerator<T> GetEnumerator() {
	//			foreach( T item in items ) {
	//					yield return item;
	//			}
	//	}
	//
	//	IEnumerator IEnumerable.GetEnumerator() {
	//			return this.GetEnumerator();
	//	}
	//
	//}
	//

}
