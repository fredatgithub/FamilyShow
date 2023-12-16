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

using System;
using System.Globalization;
using System.IO;

namespace FamilyShowLib
{
  /// <summary>
  /// Exports data from a People collection to a GEDCOM file.
  /// </summary>
  public class GedcomExport
  {
    #region fields

    // Writes the text (GEDCOM) file.
    private StreamWriter writer;

    // Maps GUID IDs (which are too long for GEDCOM) to smaller IDs.
    private readonly GedcomIdMap idMap = new();

    // The people collection that is being exported.
    private PeopleCollection people;

    // Family group counter.
    private int familyId = 1;

    #endregion

    /// <summary>
    /// Export the data from the People collection to the specified GEDCOM file.
    /// </summary>
    public void Export(PeopleCollection peopleCollection, string gedcomFilePath)
    {
      people = peopleCollection;

      using (writer = new StreamWriter(gedcomFilePath))
      {
        WriteLine(0, "HEAD", "");
        ExportPeople();
        ExportFamilies();
        WriteLine(0, "TRLR", "");
      }
    }

    /// <summary>
    /// Export each person to the GEDCOM file.
    /// </summary>
    private void ExportPeople()
    {
      foreach (Person person in people)
      {
        // Start of a new individual record.
        WriteLine(0, string.Format(CultureInfo.InvariantCulture, "@{0}@", idMap.Get(person.Id)), "INDI");

        // Export details.

        // Name.
        ExportName(person);

        // Nickname.
        if (!string.IsNullOrEmpty(person.NickName))
        {
          WriteLine(2, "NICK", person.NickName);
        }

        // Prefix.
        if (!string.IsNullOrEmpty(person.Suffix))
        {
          WriteLine(2, "NPFX", person.Suffix);
        }

        // Married name.
        if (!string.IsNullOrEmpty(person.MarriedName))
        {
          WriteLine(2, "_MARNM", person.MarriedName);
        }

        // Gender.
        ExportGender(person);

        // Birth and death info.
        ExportEvent("BIRT", person.BirthDate, person.BirthPlace);
        ExportEvent("DEAT", person.DeathDate, person.DeathPlace);

        // Photos.
        ExportPhotos(person);
      }
    }

    /// <summary>
    /// Create the family section (the FAM tags) in the GEDCOM file.
    /// </summary>
    private void ExportFamilies()
    {
      // Exporting families is more difficult since need to export each
      // family group. A family group consists of one or more parents,
      // marriage / divorce information and children. The FamilyMap class
      // creates a list of family groups from the People collection.
      FamilyMap map = [];
      map.Create(people);

      // Created the family groups, now export each family.
      foreach (Family family in map.Values)
      {
        ExportFamily(family);
      }
    }

    /// <summary>
    /// Export one family group to the GEDCOM file.
    /// </summary>
    private void ExportFamily(Family family)
    {
      // Return right away if this is only a single person without any children.
      if (family.ParentRight == null && family.Children.Count == 0)
      {
        return;
      }

      // Start of new family record.
      WriteLine(0, string.Format(CultureInfo.InvariantCulture, "@F{0}@", familyId++), "FAM");

      // Marriage info.
      ExportMarriage(family.ParentLeft, family.ParentRight, family.Relationship);

      // Children.
      foreach (Person child in family.Children)
      {
        WriteLine(1, "CHIL", string.Format(CultureInfo.InvariantCulture, "@{0}@", idMap.Get(child.Id)));
      }
    }

    /// <summary>
    /// Export marriage / divorce information.
    /// </summary>
    private void ExportMarriage(Person partnerLeft, Person partnerRight, SpouseRelationship relationship)
    {
      // PartnerLeft.
      if (partnerLeft != null && partnerLeft.Gender == Gender.Male)
      {
        WriteLine(1, "HUSB", string.Format(CultureInfo.InvariantCulture, "@{0}@", idMap.Get(partnerLeft.Id)));
      }

      if (partnerLeft != null && partnerLeft.Gender == Gender.Female)
      {
        WriteLine(1, "WIFE", string.Format(CultureInfo.InvariantCulture, "@{0}@", idMap.Get(partnerLeft.Id)));
      }

      // PartnerRight.
      if (partnerRight != null && partnerRight.Gender == Gender.Male)
      {
        WriteLine(1, "HUSB", string.Format(CultureInfo.InvariantCulture, "@{0}@", idMap.Get(partnerRight.Id)));
      }

      if (partnerRight != null && partnerRight.Gender == Gender.Female)
      {
        WriteLine(1, "WIFE", string.Format(CultureInfo.InvariantCulture, "@{0}@", idMap.Get(partnerRight.Id)));
      }

      if (relationship == null)
      {
        return;
      }

      // Marriage.
      if (relationship.SpouseModifier == SpouseModifier.Current)
      {
        WriteLine(1, "MARR", "Y");

        // Date if it exist.
        if (relationship.MarriageDate != null)
        {
          WriteLine(2, "DATE", ExportDate(relationship.MarriageDate));
        }
      }

      // Divorce.
      if (relationship.SpouseModifier == SpouseModifier.Former)
      {
        WriteLine(1, "DIV", "Y");

        // Date if it exist.
        if (relationship.DivorceDate != null)
        {
          WriteLine(2, "DATE", ExportDate(relationship.DivorceDate));
        }
      }
    }

    private void ExportName(Person person)
    {
      string firstMiddleNameSpace = (!string.IsNullOrEmpty(person.FirstName) && !string.IsNullOrEmpty(person.MiddleName)) ? " " : "";

      string value = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}/{3}/", person.FirstName, firstMiddleNameSpace, person.MiddleName, person.LastName);

      WriteLine(1, "NAME", value);
    }

    private void ExportPhotos(Person person)
    {
      foreach (Photo photo in person.Photos)
      {
        WriteLine(1, "OBJE", "");
        WriteLine(2, "FILE", photo.FullyQualifiedPath);
      }
    }

    private void ExportEvent(string tag, DateTime? date, string place)
    {
      // Return right away if don't have a date or place to export.
      if (date == null && string.IsNullOrEmpty(place))
      {
        return;
      }

      // Start the new event tag.
      WriteLine(1, tag, "");

      // Date.
      if (date != null)
      {
        string Date = ExportDate(date);
        if (!string.IsNullOrEmpty(Date))
        {
          WriteLine(2, "DATE", Date);
        }
      }

      // Place.
      if (!string.IsNullOrEmpty(place))
      {
        WriteLine(2, "PLAC", place);
      }
    }

    private static string ExportDate(DateTime? date)
    {
      if (date == null)
        return string.Empty;
      else
      {

        string day = date.Value.Day.ToString();
        string year = date.Value.Year.ToString();
        int month = date.Value.Month;
        string monthString = GetMMM(month);
        return day + " " + monthString + " " + year;
      }
    }

    //converts month number to 3 letter month abbreviation as used in GEDCOM
    private static string GetMMM(int month)
    {
      return month switch
      {
        1 => "JAN",
        2 => "FEB",
        3 => "MAR",
        4 => "APR",
        5 => "MAY",
        6 => "JUN",
        7 => "JUL",
        8 => "AUG",
        9 => "SEP",
        10 => "OCT",
        11 => "NOV",
        12 => "DEC",
        _ => throw new NotImplementedException()
      };
    }

    private void ExportGender(Person person)
    {
      WriteLine(1, "SEX", (person.Gender == Gender.Female) ? "F" : "M");
    }

    /// <summary>
    /// Write a GEDCOM line, this is more involved since the line cannot contain 
    /// carriage returns or exceed 255 characters. First, divide the value by carriage 
    /// return. Then divide each carriage-return line into chunks of 200 characters. 
    /// The first line contains the original tag name and level, carriage returns contain
    /// the CONT tag and continue lines contains CONC.
    /// </summary>
    /// <param name="level">The level</param>
    /// <param name="tag">The tag of the line to be written.</param>
    /// <param name="value">The value of the line to be written.</param>
    private void WriteLine(int level, string tag, string value)
    {
      // The entire line length cannot exceed 255 characters using
      // 200 for the value which should say below the 255 line length.
      const int ValueLimit = 200;

      // Most lines do not need special processing, export the line if it
      // does not contain carriage returns or exceed the line length.
      if (value.Length < ValueLimit && !value.Contains('\r') && !value.Contains('\n'))
      {
        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", level, tag, value));
        return;
      }

      // First divide the value by carriage returns.
      value = value.Replace("\r\n", "\n");
      value = value.Replace("\r", "\n");
      string[] lines = value.Split('\n');

      // Process each line.
      for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
      {
        // The current line processing.
        string line = lines[lineIndex];

        // Write each line but don't exceed the line limit, loop here
        // and write each chunk out at a time.
        int chunkCount = (line.Length + ValueLimit - 1) / ValueLimit;

        for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
        {
          // Current position in the value.
          int pos = chunkIndex * ValueLimit;

          // Current value chunk to write.
          string chunk = line.Substring(pos, Math.Min(line.Length - pos, ValueLimit));

          // Always use the original level and tag for the first line, but use
          // the concatenation tag (CONT) for all other lines.
          if (lineIndex == 0 && chunkIndex == 0)
          {
            writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", level, tag, chunk));
          }
          else
          {
            writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", level + 1, "CONC", chunk));
          }
        }

        // All lines except the last line have the continue (CONT) tag.
        if (lineIndex < lines.Length - 1)
        {
          writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} {1}", level + 1, "CONT"));
        }
      }
    }
  }
}