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
using System.Diagnostics;
using System.Linq;
using System.Text;


using NmpBase;


namespace Nmp.Output {


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	class Diversion {

		public const string	DEFAULT_DIV_NAME	= "";
		public const string THROW_AWAY_DIV_NAME = "null";

		int						index;
		string				name;
		StringBuilder	value;


		/////////////////////////////////////////////////////////////////////////////

		public bool IsDefaultDiversion		{ get { return name == DEFAULT_DIV_NAME; } }
		public bool IsThrowAwayDivName		{ get { return name == THROW_AWAY_DIV_NAME; } }


		/////////////////////////////////////////////////////////////////////////////

		public string Name
		{
			get {
				return name;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public StringBuilder Value
		{
			get {
				return this.value;
			}

			set {
				this.value = value;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public Diversion( int index, string name )
		{
			this.index = index;
			this.name = name;
			this.value = new StringBuilder();
		}

	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	class Diversions {

		// ******
		int indexNextDiversion = 0;
		Dictionary<string, Diversion>	diversions = new Dictionary<string, Diversion>();


		/////////////////////////////////////////////////////////////////////////////

		public Diversion GetExistingDiversion( string name )
		{
			// ******
			name = name.ToLower();
			if( string.IsNullOrEmpty(name) || "0" == name ) {
				name = Diversion.DEFAULT_DIV_NAME;
			}

			// ******
			Diversion div;
			if( diversions.TryGetValue(name, out div) ) {
				return div;
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public Diversion GetDiversion( string name )
		{
			// ******
			name = name.ToLower();
			Diversion div = GetExistingDiversion( name );
			if( null != div ) {
				return div;
			}

			// ******
			diversions.Add( name, new Diversion(indexNextDiversion++, name) );
			return diversions[ name ];
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Remove( string name )
		{
			name = name.ToLower();
			diversions.Remove( name );
		}


	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	class DiversionStack : NmpStack<Diversion> {
	}

}
