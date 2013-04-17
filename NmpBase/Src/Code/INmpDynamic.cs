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

namespace NmpBase {

	/////////////////////////////////////////////////////////////////////////////

	public enum NmpDynamicType {
		None,
		Property,
		Indexer,
		Method,
		Delegate,
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// INmpDynamic interface is used on objects that implement an extensible ...
	/// </summary>
	/// 

	public interface INmpDynamic {

		NmpDynamicType HasMember( string memberName );

		//
		// property
		//
		bool HasProperty( string propName );
		Type GetPropertyType( string propName );
		object GetPropertyValue( string propName );

		//
		// indexer
		//
		bool HasIndexer( string propName );
		object GetIndexerValue( string propName, object [] args );

		//
		// method
		//
		bool HasMethod( string propName );
		object GetMethodValue( string propName, object [] args );

		//
		// property or field
		//
		//bool HasDelegate( string propName );
		//Type GetDelegateType( string propName );
		//Delegate GetDelegate( string propName );

	}
}
