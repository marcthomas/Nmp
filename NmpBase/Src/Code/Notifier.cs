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

	public enum NotifierType {

		Unknown,

		ExpressionBuilder,
		ExpressionExecution,

		MacroExecution,
	}


	/////////////////////////////////////////////////////////////////////////////

	public class ExecutionInfo {

		public int		DistressLevel	= 0;
		public string	FileName = string.Empty;
		public int		Line = 0;
		public int		Column = 0;
		public string	FullMessage = string.Empty;

		
		/////////////////////////////////////////////////////////////////////////////

		public ExecutionInfo()
		{
		}


		/////////////////////////////////////////////////////////////////////////////

		public ExecutionInfo( int level, string name, int line, int column, string msg )
		{
			DistressLevel = level;
			FileName = name;
			Line = line;
			Column = column;
			FullMessage = msg;
		}

	}


	/////////////////////////////////////////////////////////////////////////////

	public class UnknownNotifier : Notifier {

		/////////////////////////////////////////////////////////////////////////////

		public UnknownNotifier()
			:	base( NotifierType.Unknown )
		{
		}
	}


	/////////////////////////////////////////////////////////////////////////////

	public class MacroExecutionNotifier : Notifier {


		/////////////////////////////////////////////////////////////////////////////

		public MacroExecutionNotifier()
			:	base( NotifierType.MacroExecution )
		{
		}


		/////////////////////////////////////////////////////////////////////////////

		public MacroExecutionNotifier( Exception exception )
			:	base( NotifierType.MacroExecution, exception )
		{
		}
	}


	/////////////////////////////////////////////////////////////////////////////

	abstract public class Notifier {

		public NotifierType NotifierType		{ get; private set; }
		public Exception ExceptionToThrow		{ get; set; }

		public ExecutionInfo ExecutionInfo = null;	//new ExecutionInfo();


		/////////////////////////////////////////////////////////////////////////////

		public bool IsExpressionBuilderNotifier { get { return NotifierType.ExpressionBuilder == NotifierType; } }
		public bool IsExpressionExecutionNotifier { get { return NotifierType.ExpressionExecution == NotifierType; } }
		public bool IsMacroExecutionNotifier { get { return NotifierType.MacroExecution == NotifierType; } }


		/////////////////////////////////////////////////////////////////////////////

		public T Cast<T>() where T : class
		{
			return this as T;
		}


		/////////////////////////////////////////////////////////////////////////////

		public Notifier( NotifierType nt )
		{
			NotifierType = nt;
			ExceptionToThrow = null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public Notifier( NotifierType nt, Exception exception )
		{
			NotifierType = nt;
			ExceptionToThrow = exception;
		}

	}




}
