/*
 * Derived class that filters data in the diagram view.
*/

using Microsoft.FamilyShowLib;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FamilyShow.Controls.FamilyData
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
  class FamilyDisplayListView : FilterSortListView
  {
    /// <summary>
    /// Called for each item in the list. Return true if the item should be in
    /// the current result set, otherwise return false to exclude the item.
    /// </summary>
    protected override bool FilterCallback(object item)
    {
      Person person = item as Person;
      if (person == null)
      {
        return false;
      }

      return Filter.Matches(person.Name) || Filter.MatchesYear(person.BirthDate) || Filter.MatchesYear(person.DeathDate) || Filter.Matches(person.Age);
    }
  }
}