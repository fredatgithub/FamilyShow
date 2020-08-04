/*
 * Derived class that filters data in the family data view.
*/

using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow.Controls.FamilyData
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
  class FamilyEditListView : FilterSortListView
  {
    /// <summary>
    /// Called for each item in the list. Return true if the item should be in
    /// the current result set, otherwise return false to exclude the item.
    /// </summary>    
    override protected bool FilterCallback(object item)
    {
      Person person = item as Person;
      if (person == null)
      {
        return false;
      }

      // Check for match.
      if (Filter.Matches(person.FirstName) ||
          Filter.Matches(person.LastName) ||
          Filter.Matches(person.BirthPlace) ||
          Filter.Matches(person.DeathPlace) ||
          Filter.Matches(person.BirthDate) ||
          Filter.Matches(person.DeathDate) ||
          Filter.Matches(person.Age))
      {
        return true;
      }

      // Check for the special case of birthdays, if
      // matches the month and day, but don't check year.
      if (Filter.MatchesMonth(person.BirthDate) &&
          Filter.MatchesDay(person.BirthDate))
      {
        return true;
      }

      return false;
    }
  }
}