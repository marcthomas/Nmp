#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// NmpTuple6.cs
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[Serializable]
public class NmpTuple<T1, T2, T3, T4, T5, T6> : IStructuralEquatable, IStructuralComparable, IComparable, INmpTuple
{
  // ******
  private readonly T1 m_Item1;
  private readonly T2 m_Item2;
  private readonly T3 m_Item3;
  private readonly T4 m_Item4;
  private readonly T5 m_Item5;
  private readonly T6 m_Item6;



  /////////////////////////////////////////////////////////////////////////////
	
  //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
  public NmpTuple( T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6 )
  {
    this.m_Item1 = item1;
    this.m_Item2 = item2;
    this.m_Item3 = item3;
    this.m_Item4 = item4;
    this.m_Item5 = item5;
    this.m_Item6 = item6;
  }


  /////////////////////////////////////////////////////////////////////////////

  public override bool Equals(object obj)
  {
    return ((IStructuralEquatable) this).Equals(obj, EqualityComparer<object>.Default);
  }


  /////////////////////////////////////////////////////////////////////////////

  public override int GetHashCode()
  {
    return ((IStructuralEquatable) this).GetHashCode(EqualityComparer<object>.Default);
  }


  /////////////////////////////////////////////////////////////////////////////

  int IStructuralComparable.CompareTo(object other, IComparer comparer)
  {
    if (other == null) {
      return 1;
    }
    NmpTuple<T1, T2, T3, T4, T5, T6> tuple = other as NmpTuple<T1, T2, T3, T4, T5, T6>;
    if (tuple == null) {
      //throw new ArgumentException(Environment.GetResourceString("ArgumentException_TupleIncorrectType", new object[] { base.GetType().ToString() }), "other");
      throw new ArgumentException( "not an NmpTuple" );
    }

    int num = 0;
    num = comparer.Compare(this.m_Item1, tuple.m_Item1);
    if (num != 0) {
      return num;
    }
    num = comparer.Compare(this.m_Item2, tuple.m_Item2);
    if (num != 0) {
      return num;
    }
    num = comparer.Compare(this.m_Item3, tuple.m_Item3);
    if (num != 0) {
      return num;
    }
    num = comparer.Compare(this.m_Item4, tuple.m_Item4);
    if (num != 0) {
      return num;
    }
    num = comparer.Compare(this.m_Item5, tuple.m_Item5);
    if (num != 0) {
      return num;
    }

    return comparer.Compare(this.m_Item6, tuple.m_Item6);
  }


  /////////////////////////////////////////////////////////////////////////////

  bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
  {
    if (other == null) {
      return false;
    }
    NmpTuple<T1, T2, T3, T4, T5, T6> tuple = other as NmpTuple<T1, T2, T3, T4, T5, T6>;
    if (tuple == null) {
      return false;
    }
    return (comparer.Equals(this.m_Item1, tuple.m_Item1) && comparer.Equals(this.m_Item2, tuple.m_Item2) && comparer.Equals(this.m_Item3, tuple.m_Item3) && comparer.Equals(this.m_Item4, tuple.m_Item4) && comparer.Equals(this.m_Item5, tuple.m_Item5) && comparer.Equals(this.m_Item6, tuple.m_Item6));
  }

	
  /////////////////////////////////////////////////////////////////////////////

  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
  {
    int hc = NmpTuple.CombineHashCodes( comparer.GetHashCode(this.m_Item1), comparer.GetHashCode(this.m_Item2) );
    hc = NmpTuple.CombineHashCodes( hc, NmpTuple.CombineHashCodes( comparer.GetHashCode(this.m_Item3), comparer.GetHashCode(this.m_Item4) ));
    hc = NmpTuple.CombineHashCodes( hc, NmpTuple.CombineHashCodes( comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6) ));
    return hc;
  }

	
  /////////////////////////////////////////////////////////////////////////////

  int IComparable.CompareTo(object obj)
  {
    return ((IStructuralComparable) this).CompareTo(obj, Comparer<object>.Default);
  }


  /////////////////////////////////////////////////////////////////////////////

  int INmpTuple.GetHashCode(IEqualityComparer comparer)
  {
    return ((IStructuralEquatable) this).GetHashCode(comparer);
  }


  /////////////////////////////////////////////////////////////////////////////

  string INmpTuple.ToString(StringBuilder sb)
  {
    sb.Append("(");
    sb.Append(this.m_Item1);  sb.Append(", ");
    sb.Append(this.m_Item2);  sb.Append(", ");
    sb.Append(this.m_Item3);  sb.Append(", ");
    sb.Append(this.m_Item4);  sb.Append(", ");
    sb.Append(this.m_Item5);  sb.Append(", ");
    sb.Append(this.m_Item6); 
    sb.Append(")");
    return sb.ToString();
  }


  /////////////////////////////////////////////////////////////////////////////

  public override string ToString()
  {
    return ((INmpTuple) this).ToString(new StringBuilder());
  }

  // Properties
  /////////////////////////////////////////////////////////////////////////////

  public T1 Item1
  {
    //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    get
    {
      return this.m_Item1;
    }
  }
  /////////////////////////////////////////////////////////////////////////////

  public T2 Item2
  {
    //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    get
    {
      return this.m_Item2;
    }
  }
  /////////////////////////////////////////////////////////////////////////////

  public T3 Item3
  {
    //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    get
    {
      return this.m_Item3;
    }
  }
  /////////////////////////////////////////////////////////////////////////////

  public T4 Item4
  {
    //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    get
    {
      return this.m_Item4;
    }
  }
  /////////////////////////////////////////////////////////////////////////////

  public T5 Item5
  {
    //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    get
    {
      return this.m_Item5;
    }
  }
  /////////////////////////////////////////////////////////////////////////////

  public T6 Item6
  {
    //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    get
    {
      return this.m_Item6;
    }
  }


  int INmpTuple.Size
  {
    get
    {
      return 6;
    }
  }
}
