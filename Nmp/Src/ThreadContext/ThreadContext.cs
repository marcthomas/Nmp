#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// ThreadContext.cs
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Web;

using Nmp;



	/////////////////////////////////////////////////////////////////////////////

	public class ThreadContextObjectDictionary : Dictionary<string, object> {
	}


	/////////////////////////////////////////////////////////////////////////////

	public static partial class ThreadContext {

		const string DATA_SLOT_NAME			= "__nmptc__slot__";
		const string THREAD_DATA_NAME		= "__Thread_Data__";

		public static ThreadData		threadData;


		/////////////////////////////////////////////////////////////////////////////

		private static ThreadContextObjectDictionary GetDataDictionary()
		{
			// ******
			ThreadContextObjectDictionary storage = null;

			// ******
			HttpContext ctx = HttpContext.Current;
			if( ctx == null ) {
				storage = Thread.GetData( Thread.GetNamedDataSlot(DATA_SLOT_NAME) ) as ThreadContextObjectDictionary;
			}
			else {
				storage = ctx.Items [ DATA_SLOT_NAME ] as ThreadContextObjectDictionary;
			}
			
			// ******
			return storage;
		}


		/////////////////////////////////////////////////////////////////////////////

		private static object GetData(string name)
		{
			// ******
			ThreadContextObjectDictionary storage = GetDataDictionary();

			// ******
			object value;
			if( null == storage || ! storage.TryGetValue(name, out value) ) {
				return null;
			}
			
			// ******
			return value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object GetThreadStorage()
		{
			
			//
			// get a generic object for save/restore the macro processor environment
			//

			return GetDataDictionary();
		}


		/////////////////////////////////////////////////////////////////////////////

		public static ThreadContextObjectDictionary ClearThreadStorage()
		{
			// ******
			ThreadContextObjectDictionary storage = GetDataDictionary();

			HttpContext ctx = HttpContext.Current;
			if( ctx == null ) {
				Thread.FreeNamedDataSlot(DATA_SLOT_NAME);
			}
			else {
				ctx.Items.Remove( DATA_SLOT_NAME );
			}

			// ******
			return storage;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object SetThreadStorage( object objStorage )
		{
			// ******
			if( null == objStorage ) {
				ClearThreadStorage();
				return null;
			}

			// ******
			ThreadContextObjectDictionary storage = objStorage as ThreadContextObjectDictionary;
			if( null == storage ) {
				throw new InvalidCastException( "objStorage" );
			}

			// ******
			ThreadContextObjectDictionary lastStorage = ClearThreadStorage();

			// ******
			HttpContext ctx = HttpContext.Current;
			if( ctx == null ) {
				Thread.SetData( Thread.GetNamedDataSlot(DATA_SLOT_NAME), storage );
			}
			else {
				ctx.Items [ DATA_SLOT_NAME ] = storage;
			}
			
			// ******
			//
			// get thread data
			//
			threadData = GetData( THREAD_DATA_NAME ) as ThreadData;

			// ******
			return lastStorage;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static ThreadContextObjectDictionary CreateThreadStorage()
		{
			ThreadData td = new ThreadData();
			return td.Storage;
		}


	}


