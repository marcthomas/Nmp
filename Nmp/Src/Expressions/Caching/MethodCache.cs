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
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Text;


//#pragma warning disable 414


using NmpBase;
using Nmp;
using NmpExpressions;

namespace ReflectionHelpers {

	/////////////////////////////////////////////////////////////////////////////

	public class MethodCacheItem {
		public MethodInfo MethodInfo { get; set; }
		public InvokeHandler Handler { get; set; }
	}


	/////////////////////////////////////////////////////////////////////////////

	public class MethodCache {


		Dictionary<int, MethodCacheItem> cache = new Dictionary<int, MethodCacheItem> { };

		/////////////////////////////////////////////////////////////////////////////

		int Hash( Type type, string methodName, object [] unmatchedArgs )
		{
			// ******
			var sb = new StringBuilder { };
			sb.Append( type.FullName );
			sb.Append( methodName );

			if( null == unmatchedArgs ) {
				sb.Append( "null" );
			}
			else {
				foreach( var obj in unmatchedArgs ) {
					sb.Append( obj.GetType().FullName );
				}
			}

			// ******
			return sb.ToString().GetHashCode();
		}


		/////////////////////////////////////////////////////////////////////////////

		public MethodCacheItem AddToCache( Type type, object [] unmatchedArgs, string methodNameToUse, MethodInfo methodBase, InvokeHandler handler )
		{
			try {
				var hash = Hash( type, methodNameToUse, unmatchedArgs );
				var cacheItem = new MethodCacheItem { MethodInfo = methodBase, Handler = handler };
				cache.Add( hash, cacheItem );
				return cacheItem;
			}
			catch( Exception ex ) {
				throw ex;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public MethodCacheItem AddToCache( ObjectInfo objInfo, object [] unmatchedArgs, string methodNameToUse, MethodInfo methodInfo, InvokeHandler handler )
		{
			return AddToCache( objInfo.ObjectType, unmatchedArgs, methodNameToUse, methodInfo, handler );
		}


		/////////////////////////////////////////////////////////////////////////////

		public MethodCacheItem GetCachedHandler( Type type, string methodName, object [] unmatchedArgs )
		{
			// ******
			MethodCacheItem item;
			if( cache.TryGetValue( Hash( type, methodName, unmatchedArgs ), out item ) ) {
				return item;
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public MethodCacheItem GetCachedHandler( ObjectInfo objInfo, string methodName, object [] unmatchedArgs )
		{
			return GetCachedHandler( objInfo.ObjectType, methodName, unmatchedArgs );
		}


		/////////////////////////////////////////////////////////////////////////////

		public MethodCacheItem GetHandler( ObjectInfo objInfo, string methodNameToUse, MethodInfo methodInfo, object [] unmatchedArgs )
		{
			var cacheItem = GetCachedHandler( objInfo.ObjectType, methodNameToUse, unmatchedArgs );
			if( null == cacheItem ) {
				cacheItem = Generate( objInfo, methodNameToUse, methodInfo, unmatchedArgs );
			}
			return cacheItem;
		}


		/////////////////////////////////////////////////////////////////////////////

		public MethodCacheItem GetHandler( object instance, Type type, string methodNameToUse, MethodInfo methodInfo, object [] unmatchedArgs )
		{
			var cacheItem = GetCachedHandler( type, methodNameToUse, unmatchedArgs );
			if( null == cacheItem ) {
				cacheItem = Generate( instance, type, methodNameToUse, methodInfo, unmatchedArgs );
			}
			return cacheItem;
		}


		/////////////////////////////////////////////////////////////////////////////

		//public InvokeHandler Generate( MethodInfo methodInfo, object instance )
		//{
		//	var handler = InvokerGenerator.Generate( methodInfo, instance );
		//	return handler;
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public MethodCacheItem GenerateWithInstanceObject( object instanceObject, string methodNameToUse, MethodInfo methodInfo, object [] unmatchedArgs )
		//{
		//	// ******
		//	if( null == instanceObject ) {
		//		throw new ArgumentNullException( "instanceObject" );
		//	}

		//	// ******
		//	var handler = InvokerGenerator.Generate( methodInfo, instanceObject );
		//	return AddToCache( instanceObject.GetType(), unmatchedArgs, methodNameToUse, methodInfo, handler );
		//}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Called by static or instance methods and properties
		/// </summary>
		/// <param name="objInfo"></param>
		/// <param name="methodNameToUse"></param>
		/// <param name="methodInfo"></param>
		/// <param name="unmatchedArgs"></param>
		/// <returns></returns>

		public MethodCacheItem Generate( ObjectInfo objInfo, string methodNameToUse, MethodInfo methodInfo, object [] unmatchedArgs )
		{
			var handler = InvokerGenerator.Generate( methodInfo, objInfo.Object );
			return AddToCache( objInfo, unmatchedArgs, methodNameToUse, methodInfo, handler );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Called for static methods
		/// </summary>
		/// <param name="type"></param>
		/// <param name="unmatchedArgs"></param>
		/// <param name="methodNameToUse"></param>
		/// <param name="methodInfo"></param>
		/// <returns></returns>

		public MethodCacheItem Generate( object instance, Type type, string methodNameToUse, MethodInfo methodInfo, object [] unmatchedArgs )
		{
			var handler = InvokerGenerator.Generate( methodInfo, instance );
			return AddToCache( type, unmatchedArgs, methodNameToUse, methodInfo, handler );
		}


	}








}