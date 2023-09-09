/*
 * Exports data from the People collection to a GEDCOM file.
 * 
 * More information on the GEDCOM format is at http://en.wikipedia.org/wiki/Gedcom.
 * 
 * GedcomExport class
 * Exports data from a Person collection to a GEDCOM file.
 *
 * GedcomIdMap
 * Maps a Person's ID (GUID) to a GEDCOM ID (int).
 * 
 * FamilyMap
 * Creates a list of GEDCOM family groups from the People collection.
 * 
 * Family
 * One family group in the FamilyMap list.
 * 
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FamilyShowLib
{
  /// <summary>
  /// Orgainzes the People collection into a list of families. 
  /// </summary>
  class FamilyMap : Dictionary<string, Family>
  {
    /// <summary>
    /// Organize the People collection into a list of families. A family consists of
    /// an wife, husband, children, and married / divorced information.
    /// </summary>
    public void Create(PeopleCollection people)
    {
      Clear();

      // First, iterate though the list and create parent groups.
      // A parent group is one or two parents that have one or
      // more children.
      foreach (Person person in people)
      {
        Collection<Person> parents = person.Parents;
        if (parents.Count > 0)
        {
          // Only using the first two parents.
          Person parentLeft = parents[0];
          Person parentRight = (parents.Count > 1) ? parents[1] : null;

          // See if this parent group has been added to the list yet.
          string key = GetKey(parentLeft, parentRight);
          if (!ContainsKey(key))
          {
            // This parent group does not exist, add it to the list.
            Family details = new Family(parentLeft, parentRight);
            details.Relationship = parentLeft.GetSpouseRelationship(parentRight);
            this[key] = details;
          }

          // Add the child to the parent group.
          this[key].Children.Add(person);
        }
      }

      // Next, iterate though the list and create marriage groups.
      // A marriage group is current or former marriages that
      // don't have any children.
      foreach (Person person in people)
      {
        Collection<Person> spouses = person.Spouses;
        foreach (Person spouse in spouses)
        {
          // See if this marriage group is in the list.
          string key = GetKey(person, spouse);
          if (!ContainsKey(key))
          {
            // This marriage group is not in the list, add it to the list.
            Family details = new Family(person, spouse);
            details.Relationship = person.GetSpouseRelationship(spouse);
            this[key] = details;
          }
        }
      }
    }

    /// <summary>
    /// Return a string for the parent group.
    /// </summary>
    private static string GetKey(Person partnerLeft, Person partnerRight)
    {
      // This is used as the key to the list. This is tricky since parent
      // groups should not be duplicated. For example, the list should
      // not contain the parent groups:
      //
      //  Bob Bee
      //  Bee Bob
      //  
      // The list should only contain the group:
      //
      //  Bob Bee
      //
      // This is accomplished by concatenating the parent
      // ID's together when creating the key.

      string key = partnerLeft.Id;
      if (partnerRight != null)
      {
        if (partnerLeft.Id.CompareTo(partnerRight.Id) < 0)
          key = partnerLeft.Id + partnerRight.Id;
        else
          key = partnerRight.Id + partnerLeft.Id;
      }

      return key;
    }
  }
}