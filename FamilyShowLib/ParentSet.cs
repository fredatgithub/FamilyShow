using System;

namespace Microsoft.FamilyShowLib
{
  /// <summary>
  /// Representation for a Parent couple.  E.g. Bob and Sue
  /// </summary>
  public class ParentSet : IEquatable<ParentSet>
  {
    private Person firstParent;

    private Person secondParent;

    public Person FirstParent
    {
      get { return firstParent; }
      set { firstParent = value; }
    }

    public Person SecondParent
    {
      get { return secondParent; }
      set { secondParent = value; }
    }

    public ParentSet(Person firstParent, Person secondParent)
    {
      this.firstParent = firstParent;
      this.secondParent = secondParent;
    }

    public string Name
    {
      get
      {
        string name = string.Empty;
        name += $"{firstParent.Name} + {secondParent.Name}";
        return name;
      }
    }

    // Parameterless contstructor required for serialization
    public ParentSet() { }

    #region IEquatable<ParentSet> Members

    /// <summary>
    /// Determine equality between two ParentSet classes.  Note: Bob and Sue == Sue and Bob
    /// </summary>
    public bool Equals(ParentSet other)
    {
      if (other != null)
      {
        if (firstParent.Equals(other.firstParent) && secondParent.Equals(other.secondParent))
        {
          return true;
        }

        if (firstParent.Equals(other.secondParent) && secondParent.Equals(other.firstParent))
        {
          return true;
        }
      }

      return false;
    }

    #endregion
  }
}