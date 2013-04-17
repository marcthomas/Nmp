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

	[Serializable()] 
	public class NmpDynamicBase : INmpDynamic {

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Has member.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <param name="memberName">
		/// 	Name of the member.
		/// </param>
		///
		/// <returns>
		/// 	.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		public virtual NmpDynamicType HasMember( string memberName )	{ return NmpDynamicType.None; }

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Query if 'propName' has property.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <param name="propName">
		/// 	Name of the property.
		/// </param>
		///
		/// <returns>
		/// 	true if property, false if not.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		public virtual bool HasProperty( string propName ) { return false; }

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Gets a property type.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <param name="propName">
		/// 	Name of the property.
		/// </param>
		///
		/// <returns>
		/// 	The property type.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		public virtual Type GetPropertyType( string propName ) { return null; }

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Gets a property value.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <param name="propName">
		/// 	Name of the property.
		/// </param>
		///
		/// <returns>
		/// 	The property value.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		public virtual object GetPropertyValue( string propName ) { return null; }

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Query if 'propName' has indexer.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <param name="propName">
		/// 	Name of the property.
		/// </param>
		///
		/// <returns>
		/// 	true if indexer, false if not.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		public virtual bool HasIndexer( string propName ) { return false; }

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Gets an indexer value.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <param name="propName">
		/// 	Name of the property.
		/// </param>
		/// <param name="args">
		/// 	The arguments.
		/// </param>
		///
		/// <returns>
		/// 	The indexer value.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		public virtual object GetIndexerValue( string propName, object [] args ) { return null; }

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Query if 'propName' has method.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <param name="propName">
		/// 	Name of the property.
		/// </param>
		///
		/// <returns>
		/// 	true if method, false if not.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		public virtual bool HasMethod( string propName ) { return false; }

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Gets a method value.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <param name="propName">
		/// 	Name of the property.
		/// </param>
		/// <param name="args">
		/// 	The arguments.
		/// </param>
		///
		/// <returns>
		/// 	The method value.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		public virtual object GetMethodValue( string propName, object [] args ) { return null; }


	}
}
