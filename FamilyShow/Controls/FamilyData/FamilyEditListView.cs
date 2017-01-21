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
        return false;

      // Check for match.
      if (this.Filter.Matches(person.FirstName) ||
          this.Filter.Matches(person.LastName) ||
          this.Filter.Matches(person.BirthPlace) ||
          this.Filter.Matches(person.DeathPlace) ||
          this.Filter.Matches(person.BirthDate) ||
          this.Filter.Matches(person.DeathDate) ||
          this.Filter.Matches(person.Age))
        return true;

      // Check for the special case of birthdays, if
      // matches the month and day, but don't check year.
      if (this.Filter.MatchesMonth(person.BirthDate) &&
          this.Filter.MatchesDay(person.BirthDate))
        return true;

      return false;
    }
  }
}