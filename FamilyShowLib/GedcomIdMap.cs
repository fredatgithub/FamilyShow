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
using System.Globalization;

namespace Microsoft.FamilyShowLib
{
  /// <summary>
  /// Maps FamilyShow Person.Id to a GEDCOM ID. GEDCOM IDs cannot 
  /// exceed 22 characters so GUIDs (Person.Id type) cannot be used
  /// when exporting. 
  /// </summary>
  class GedcomIdMap
  {
    #region fields

    // Quick lookup that maps a GUID to a GEDCOM ID.
    private Dictionary<string, string> map = new Dictionary<string, string>();

    // The next ID to assign.
    private int nextId;

    #endregion

    /// <summary>
    /// Return the mapped ID for the specified GUID.
    /// </summary>
    public string Get(string guid)
    {
      // Return right away if already mapped.
      if (map.ContainsKey(guid))
      {
        return map[guid];
      }

      // Assign a new GEDCOM ID and add to map.
      string id = string.Format(CultureInfo.InvariantCulture, "I{0}", nextId++);
      map[guid] = id;
      return id;
    }
  }
}