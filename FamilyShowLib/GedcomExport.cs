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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace Microsoft.FamilyShowLib
{
    /// <summary>
    /// Exports data from a People collection to a GEDCOM file.
    /// </summary>
    public class GedcomExport
    {
        #region fields

        // Writes the text (GEDCOM) file.
        private TextWriter writer;

        // Maps GUID IDs (which are too long for GEDCOM) to smaller IDs.
        private GedcomIdMap idMap = new GedcomIdMap();

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
            this.people = peopleCollection;

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
                WriteLine(0, string.Format(CultureInfo.InvariantCulture, 
                    "@{0}@", idMap.Get(person.Id)), "INDI");

                // Export details.

                // Name.
                ExportName(person);

                // Nickname.
                if (!string.IsNullOrEmpty(person.NickName))
                    WriteLine(2, "NICK", person.NickName);

                // Prefix.
                if (!string.IsNullOrEmpty(person.Suffix))
                    WriteLine(2, "NPFX", person.Suffix);

                // Married name.
                if (!string.IsNullOrEmpty(person.MarriedName))
                    WriteLine(2, "_MARNM", person.MarriedName);

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
            FamilyMap map = new FamilyMap();
            map.Create(people);

            // Created the family groups, now export each family.
            foreach (Family family in map.Values)
                ExportFamily(family);
        }

        /// <summary>
        /// Export one family group to the GEDCOM file.
        /// </summary>
        private void ExportFamily(Family family)
        {
            // Return right away if this is only a single person without any children.
            if (family.ParentRight == null && family.Children.Count == 0)
                return;

            // Start of new family record.
            WriteLine(0, string.Format(CultureInfo.InvariantCulture, "@F{0}@", familyId++), "FAM");

            // Marriage info.
            ExportMarriage(family.ParentLeft, family.ParentRight, family.Relationship);

            // Children.
            foreach (Person child in family.Children)
            {
                WriteLine(1, "CHIL", string.Format(
                    CultureInfo.InvariantCulture, "@{0}@", idMap.Get(child.Id)));
            }
        }

        /// <summary>
        /// Export marriage / divorce information.
        /// </summary>
        private void ExportMarriage(Person partnerLeft, Person partnerRight, SpouseRelationship relationship)
        {
            // PartnerLeft.
            if (partnerLeft != null && partnerLeft.Gender == Gender.Male)
                WriteLine(1, "HUSB", string.Format(CultureInfo.InvariantCulture, 
                "@{0}@", idMap.Get(partnerLeft.Id)));
                
            if (partnerLeft != null && partnerLeft.Gender == Gender.Female)
                WriteLine(1, "WIFE", string.Format(CultureInfo.InvariantCulture, 
                "@{0}@", idMap.Get(partnerLeft.Id)));

            // PartnerRight.
            if (partnerRight != null && partnerRight.Gender == Gender.Male)
                WriteLine(1, "HUSB", string.Format(CultureInfo.InvariantCulture, 
                "@{0}@", idMap.Get(partnerRight.Id)));
                
            if (partnerRight != null && partnerRight.Gender == Gender.Female)
                WriteLine(1, "WIFE", string.Format(CultureInfo.InvariantCulture, 
                "@{0}@", idMap.Get(partnerRight.Id)));

            if (relationship == null)
                return;

            // Marriage.
            if (relationship.SpouseModifier == SpouseModifier.Current)
            {
                WriteLine(1, "MARR", "Y");

                // Date if it exist.
                if (relationship.MarriageDate != null)
                    WriteLine(2, "DATE", relationship.MarriageDate.Value.ToShortDateString());
            }

            // Divorce.
            if (relationship.SpouseModifier == SpouseModifier.Former)
            {
                WriteLine(1, "DIV", "Y");

                // Date if it exist.
                if (relationship.DivorceDate != null)
                    WriteLine(2, "DATE", relationship.DivorceDate.Value.ToShortDateString());
            }
        }

        private void ExportName(Person person)
        {
            string firstMiddleNameSpace = (!string.IsNullOrEmpty(person.FirstName) &&
                !string.IsNullOrEmpty(person.MiddleName)) ? " " : "";

            string value = string.Format(CultureInfo.InvariantCulture, 
                "{0}{1}{2}/{3}/", person.FirstName, firstMiddleNameSpace,
                person.MiddleName, person.LastName);

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
                return;

            // Start the new event tag.
            WriteLine(1, tag, "");

            // Date.
            if (date != null)
                WriteLine(2, "DATE", date.Value.ToShortDateString());

            // Place.
            if (!string.IsNullOrEmpty(place))
                WriteLine(2, "PLAC", place);
        }

        private void ExportGender(Person person)
        {
            WriteLine(1, "SEX", (person.Gender == Gender.Female) ? "F" : "M");
        }

        // Write a GEDCOM line, this is more involved since the line cannot contain 
        // carriage returns or exceed 255 characters. First, divide the value by carriage 
        // return. Then divide each carriage-return line into chunks of 200 characters. 
        // The first line contains the original tag name and level, carriage returns contain
        // the CONT tag and continue lines contains CONC.
        private void WriteLine(int level, string tag, string value)
        {
            // The entire line length cannot exceed 255 characters using
            // 200 for the value which should say below the 255 line length.
            const int ValueLimit = 200;

            // Most lines do not need special processing, export the line if it
            // does not contain carriage returns or exceed the line length.
            if (value.Length < ValueLimit && !value.Contains("\r") && !value.Contains("\n"))
            {
                writer.WriteLine(string.Format(
                    CultureInfo.InvariantCulture, 
                    "{0} {1} {2}", level, tag, value));

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
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, 
                            "{0} {1} {2}", level, tag, chunk));
                    }
                    else
                    {
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, 
                            "{0} {1} {2}", level + 1, "CONC", chunk));
                    }
                }

                // All lines except the last line have the continue (CONT) tag.
                if (lineIndex < lines.Length - 1)
                {
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, 
                       "{0} {1}", level + 1, "CONT"));
                }
            }
        }
    }

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
                return map[guid];

            // Assign a new GEDCOM ID and add to map.
            string id = string.Format(CultureInfo.InvariantCulture, "I{0}", nextId++);
            map[guid] = id;
            return id;
        }
    }


    /// <summary>
    /// One family group. 
    /// </summary>
    class Family
    {
        #region fields

        private Person parentLeft;
        private Person parentRight;
        private SpouseRelationship relationship;
        private List<Person> children = new List<Person>();

        #endregion

        /// <summary>
        /// Get the left-side parent.
        /// </summary>
        public Person ParentLeft
        {
            get { return parentLeft; }
        }

        /// <summary>
        /// Get the right-side parent.
        /// </summary>
        public Person ParentRight
        {
            get { return parentRight; }
        }

        /// <summary>
        /// Get or set the relationship for the two parents.
        /// </summary>
        public SpouseRelationship Relationship
        {
            get { return relationship; }
            set { relationship = value; }
        }

        /// <summary>
        /// Get the list of children.
        /// </summary>
        public List<Person> Children
        {
            get { return children; }
        }

        public Family(Person parentLeft, Person parentRight)
        {
            this.parentLeft = parentLeft;
            this.parentRight = parentRight;
        }
    }

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
            this.Clear();

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
                    if (!this.ContainsKey(key))
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
                    if (!this.ContainsKey(key))
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