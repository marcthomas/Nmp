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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;

namespace NmpBase {

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// 	List of NmpstringLists using List&lt;T&gt;  
	/// </summary>
	///
	/// <remarks>
	/// 	Jpm, 3/26/2011.
	/// </remarks>
	////////////////////////////////////////////////////////////////////////////

	[Serializable()]
	public class ListOfNmpStringList : List<NmpStringList> {


		/////////////////////////////////////////////////////////////////////////////

		public void Save( IFormatter formatter, string fileName )
		{
			// ******
			try {
				using( var fs = new FileStream( fileName, FileMode.Create ) ) {
					formatter.Serialize( fs, this );
				}
			}
			catch( Exception ex ) {
				throw ex;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public ListOfNmpStringList Load( IFormatter formatter, string fileName )
		{
			// ******
			try {
				using( var fs = new FileStream( fileName, FileMode.Open ) ) {
					ListOfNmpStringList list = formatter.Deserialize( fs ) as ListOfNmpStringList;

					if( null != list ) {
						foreach( NmpStringList strList in list ) {
							Add( strList );
						}
					}
				}

				// ******
				return this;
			}
			catch( Exception ex ) {
				throw ex;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SaveSoap( string fileName )
		{
			Save( new SoapFormatter(), fileName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public ListOfNmpStringList LoadSoap( string fileName )
		{
			return Load( new SoapFormatter(), fileName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SaveBinary( string fileName )
		{
			Save( new BinaryFormatter(), fileName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public ListOfNmpStringList LoadBinary( string fileName )
		{
			return Load( new BinaryFormatter(), fileName );
		}


	}
}
