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
	/// 	Collection of NmpArray objects using List&lt;T&gt;
	/// </summary>
	///
	/// <remarks>
	/// 	Jpm, 3/26/2011.
	/// </remarks>
	////////////////////////////////////////////////////////////////////////////

	[Serializable()]
	public class ListOfNmpArray : List<NmpArray> {


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

		public ListOfNmpArray Load( IFormatter formatter, string fileName )
		{
			// ******
			try {
				using( var fs = new FileStream( fileName, FileMode.Open ) ) {
					ListOfNmpArray list = formatter.Deserialize( fs ) as ListOfNmpArray;

					if( null != list ) {
						foreach( NmpArray array in list ) {
							Add( array );
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

		public ListOfNmpArray LoadSoap( string fileName )
		{
			return Load( new SoapFormatter(), fileName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SaveBinary( string fileName )
		{
			Save( new BinaryFormatter(), fileName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public ListOfNmpArray LoadBinary( string fileName )
		{
			return Load( new BinaryFormatter(), fileName );
		}


	}
}
