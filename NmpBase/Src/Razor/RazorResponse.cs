#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
using System.Text;
using System.IO;
using System;
using System.Diagnostics;



namespace NmpBase.Razor {

	/////////////////////////////////////////////////////////////////////////////

	public class RazorResponse : IDisposable		
	{

		public TextWriter Writer = new StringWriter();


		/////////////////////////////////////////////////////////////////////////////

		public string GetText
		{
			get {
				var stringWriter = Writer as StringWriter;
				if( null == stringWriter ) {
					throw new Exception( "Writer is not a StringWriter instance" );
				}

				// ******
				return stringWriter.GetStringBuilder().ToString();
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual void SetTextWriter( TextWriter writer )
		{
			// ******
			//
			// Close original writer
			//
			if( Writer != null ) {
				Writer.Close();
			}
			
			// ******			
			Writer = writer;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SetWriterText( string text )
		{
			Writer = new StringWriter( new StringBuilder(text) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string GetWriterText()
		{
			var sb = ((StringWriter) Writer).GetStringBuilder();
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public StringBuilder SwapWriters( StringBuilder sb = null )
		{
			var temp = ((StringWriter) Writer).GetStringBuilder();
			Writer = null == sb ? new StringWriter() : new StringWriter( sb );
			return temp;
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual void Write(object value)
		{
			Writer.Write(value);
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual void WriteLine( object value )
		{
			Write(value);
			Write("\r\n");
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual void WriteFormat( string format, params object[] args )
		{
			Write( string.Format(format, args) );		
		}


		/////////////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			return Writer.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual void Dispose()
		{
			Writer.Close();
		}


	}

}
