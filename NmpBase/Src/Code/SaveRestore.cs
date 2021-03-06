﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NmpBase {

	/// <summary>
	/// Use in "using( new SaveRestor<XXX>() ) { }"
	/// </summary>
	/// <typeparam name="T"></typeparam>

	public class SaveRestore<T> : IDisposable {

		T value;
		Action<T> onDispose;

		public void Dispose()
		{
			if( null != onDispose ) {
				onDispose( value );
			}
		}

		public SaveRestore( T oldValue, T newValue, Action<T> setAction )
		{
			value = oldValue;
			setAction( newValue );
			onDispose = setAction;
		}

	}
}
