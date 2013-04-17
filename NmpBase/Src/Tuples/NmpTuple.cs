#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// NmpTuple.cs
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public interface IStructuralEquatable {
	// Methods
	bool Equals( object other, IEqualityComparer comparer );
	int GetHashCode( IEqualityComparer comparer );
}

public interface IStructuralComparable
{
    // Methods
    int CompareTo(object other, IComparer comparer);
}

 


public interface INmpTuple
{
  int      Size { get; }
  int      GetHashCode(IEqualityComparer comparer);
  string   ToString(StringBuilder sb);
}

public class NmpTuple {

  internal static int CombineHashCodes(int h1, int h2)
  {
     return (((h1 << 5) + h1) ^ h2);
  }

  /////////////////////////////////////////////////////////////////////////////

  public static NmpTuple<T1> Create<T1>(T1 item1)
  {
    return new NmpTuple<T1>(item1);
  }

  /////////////////////////////////////////////////////////////////////////////

  public static NmpTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
  {
    return new NmpTuple<T1, T2>(item1, item2);
  }

  /////////////////////////////////////////////////////////////////////////////

  public static NmpTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
  {
    return new NmpTuple<T1, T2, T3>(item1, item2, item3);
  }

  /////////////////////////////////////////////////////////////////////////////

  public static NmpTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
  {
    return new NmpTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
  }

  /////////////////////////////////////////////////////////////////////////////

  public static NmpTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
  {
    return new NmpTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
  }

  /////////////////////////////////////////////////////////////////////////////

  public static NmpTuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
  {
    return new NmpTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
  }

  /////////////////////////////////////////////////////////////////////////////

  public static NmpTuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
  {
    return new NmpTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
  }

  /////////////////////////////////////////////////////////////////////////////

  public static NmpTuple<T1, T2, T3, T4, T5, T6, T7, T8> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
  {
    return new NmpTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, item6, item7, item8);
  }

  /////////////////////////////////////////////////////////////////////////////

  public static NmpTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9)
  {
    return new NmpTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, item6, item7, item8, item9);
  }

  /////////////////////////////////////////////////////////////////////////////

  public static NmpTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10)
  {
    return new NmpTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10);
  }

  /////////////////////////////////////////////////////////////////////////////

  public static NmpTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11)
  {
    return new NmpTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11);
  }


} 

//
// removing old files
//
// deleting d:\work\2010 forward\nmp2\documentation\examples\shipping\tuples\tuples\code\NmpTuple.cs  
// deleting d:\work\2010 forward\nmp2\documentation\examples\shipping\tuples\tuples\code\NmpTuple1.cs  
// deleting d:\work\2010 forward\nmp2\documentation\examples\shipping\tuples\tuples\code\NmpTuple10.cs  
// deleting d:\work\2010 forward\nmp2\documentation\examples\shipping\tuples\tuples\code\NmpTuple11.cs  
// deleting d:\work\2010 forward\nmp2\documentation\examples\shipping\tuples\tuples\code\NmpTuple2.cs  
// deleting d:\work\2010 forward\nmp2\documentation\examples\shipping\tuples\tuples\code\NmpTuple3.cs  
// deleting d:\work\2010 forward\nmp2\documentation\examples\shipping\tuples\tuples\code\NmpTuple4.cs  
// deleting d:\work\2010 forward\nmp2\documentation\examples\shipping\tuples\tuples\code\NmpTuple5.cs  
// deleting d:\work\2010 forward\nmp2\documentation\examples\shipping\tuples\tuples\code\NmpTuple6.cs  
// deleting d:\work\2010 forward\nmp2\documentation\examples\shipping\tuples\tuples\code\NmpTuple7.cs  
// deleting d:\work\2010 forward\nmp2\documentation\examples\shipping\tuples\tuples\code\NmpTuple8.cs  
// deleting d:\work\2010 forward\nmp2\documentation\examples\shipping\tuples\tuples\code\NmpTuple9.cs  
//
// generating new files
//
// generating NmpTuple<T1>  
// generating NmpTuple<T1, T2>  
// generating NmpTuple<T1, T2, T3>  
// generating NmpTuple<T1, T2, T3, T4>  
// generating NmpTuple<T1, T2, T3, T4, T5>  
// generating NmpTuple<T1, T2, T3, T4, T5, T6>  
// generating NmpTuple<T1, T2, T3, T4, T5, T6, T7>  
// generating NmpTuple<T1, T2, T3, T4, T5, T6, T7, T8>  
// generating NmpTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>  
// generating NmpTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>  
// generating NmpTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>  

//
// update project file: "..\NmpTuples.csproj"
//

