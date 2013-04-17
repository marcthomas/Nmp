#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// StringDictionary.cs
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Security.Permissions;

#pragma warning disable 169


namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////


	class JSONArrayBuilder : JSONItemHandler {

		// ******
		WarningCall warning;

		// ******
		string	lastIdentifier;

		NmpArray	rootArray;
		NmpArray	currentArray;

		NmpObjectList	lastList;

		NmpStack<NmpArray>			arrayStack = new NmpStack<NmpArray>();
		NmpStack<NmpObjectList>	listStack = new NmpStack<NmpObjectList>();


		///////////////////////////////////////////////////////////////////////////

		public NmpArray Array
		{
			get {
				return rootArray;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		protected NmpArray CurrentArray
		{
			get {
				return arrayStack.Peek();
			}
		}


		///////////////////////////////////////////////////////////////////////////

		protected NmpObjectList CurrentList
		{
			get {
				//return 0 == listStack.Count ? null : listStack.Peek();


				if( 0 == listStack.Count && 0 == arrayStack.Count ) {
					//
					// '[' opening the json text
					//
					listStack.Push( new NmpObjectList() );
				}
				
				return 0 == listStack.Count ? null : listStack.Peek();


			}
		}


		///////////////////////////////////////////////////////////////////////////

		public void Warning( string msgFmt, params object [] args )
		{
			if( null != warning ) {
				warning( "JSON parser warning: " + msgFmt, args );
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public void EnterObject( JSONParser jp )
		{
			// ******
			//Trace.Write( "Enter object\n" );

			// ******
			if( null == rootArray ) {
				rootArray = new NmpArray();
				arrayStack.Push( rootArray );
			}
			else {
				//
				// arrays can have unnamed objects
				//


				var array = new NmpArray();
				if( string.IsNullOrEmpty(lastIdentifier) ) {
					CurrentList.Add( array );
				}
				else {
					CurrentArray.Add( lastIdentifier, array );
					lastIdentifier = null;
				}

				// ******
				arrayStack.Push( array );
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public void ExitObject( JSONParser jp )
		{
			// ******
			//Trace.Write( "Exit object\n" );

			// ******
			arrayStack.Pop();
		}


		///////////////////////////////////////////////////////////////////////////

		public void EnterArray( JSONParser jp )
		{
			// ******
			//Trace.Write( "Enter array\n" );

			// ******
			var list = new NmpObjectList();
			if( string.IsNullOrEmpty(lastIdentifier) ) {
				CurrentList.Add( list );
			}
			else {
				CurrentArray.Add( lastIdentifier, list );
				lastIdentifier = null;
			}

			// ******
			listStack.Push( list );
		}


		///////////////////////////////////////////////////////////////////////////

		public void ExitArray( JSONParser jp )
		{
			// ******
			//Trace.Write( "Exit array\n" );

			// ******
			listStack.Pop();
		}


		///////////////////////////////////////////////////////////////////////////

		public void Identifier( JSONParser jp, string identifier )
		{
			// ******
			//Trace.Write( "Identifier: {0}\n", identifier );

			// ******
			lastIdentifier = identifier;
		}


		///////////////////////////////////////////////////////////////////////////

		public void Value( JSONParser jp, object value )
		{
			// ******
			//Trace.Write( "Value: {0}\n", value.ToString() );

			// ******
			if( string.IsNullOrEmpty(lastIdentifier) ) {
				CurrentList.Add( value );
			}
			else {
				CurrentArray.Add( lastIdentifier, value );
				lastIdentifier = null;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public JSONArrayBuilder( WarningCall warning )
		{
			// ******
			this.warning = warning;

			// ******
			rootArray = null;
		}

	}



}
