/*
 * Imports data from a GEDCOM file to the People collection. The GEDCOM file
 * is first converted to an XML file so it's easier to parse, then individuals
 * are parsed from the file, and then families. Only data that is used in the 
 * FamilyShow application is imported. 
 *
 * More information on the GEDCOM format is at http://en.wikipedia.org/wiki/Gedcom.
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.FamilyShowLib
{
  /// <summary>
  /// Import data from a GEDCOM file to the Person collection.
  /// </summary>
  public class GedcomImport
  {
    #region fields

    // The collection to add entries.
    private PeopleCollection people;

    // Convert the GEDCOM file to an XML file which is easier 
    // to parse, this contains the GEDCOM info in an XML format.
    private XmlDocument doc;

    #endregion

    /// <summary>
    /// Populate the people collection with information from the GEDCOM file.
    /// </summary>
    public void Import(PeopleCollection peopleCollection, string gedcomFilePath)
    {
      // Clear current content.
      peopleCollection.Clear();

      // First convert the GEDCOM file to an XML file so it's easier to parse,
      // the temp XML file is deleted when importing is complete.
      string xmlFilePath = Path.GetTempFileName();

      try
      {
        people = peopleCollection;

        // Convert the GEDCOM file to a temp XML file.
        GedcomConverter.ConvertToXml(gedcomFilePath, xmlFilePath, true);
        doc = new XmlDocument();
        doc.Load(xmlFilePath);

        // Import data from the temp XML file to the people collection.
        ImportPeople();
        ImportFamilies();

        // The collection requires a primary-person, use the first
        // person added to the collection as the primary-person.
        if (peopleCollection.Count > 0)
        {
          peopleCollection.Current = peopleCollection[0];
        }
      }
      finally
      {
        // Delete the temp XML file.
        File.Delete(xmlFilePath);
      }
    }

    /// <summary>
    /// Imports the individuals (INDI tags) from the GEDCOM XML file.
    /// </summary>
    private void ImportPeople()
    {
      // Get list of people.
      XmlNodeList list = doc.SelectNodes("/root/INDI");

      foreach (XmlNode node in list)
      {
        // Create a new person that will be added to the collection.
        Person person = new Person();

        // Import details about the person.
        person.FirstName = GetFirstName(node);
        person.LastName = GetLastName(node);
        person.NickName = GetNickName(node);
        person.Suffix = GetSuffix(node);
        person.MarriedName = GetMarriedName(node);

        person.Id = GetId(node);
        person.Gender = GetGender(node);

        ImportBirth(person, node);
        ImportDeath(person, node);

        ImportPhotos(person, node);
        ImportNote(person, node);

        people.Add(person);
      }
    }

    /// <summary>
    /// Imports the families (FAM tags) from the GEDCOM XML file.
    /// </summary>
    private void ImportFamilies()
    {
      // Get list of families.
      XmlNodeList list = doc.SelectNodes("/root/FAM");
      foreach (XmlNode node in list)
      {
        // Get family (husband, wife and children) IDs from the GEDCOM file.
        string husband = GetHusbandID(node);
        string wife = GetWifeID(node);
        string[] children = GetChildrenIDs(node);

        // Get the Person objects for the husband and wife,
        // required for marriage info and adding children.
        Person husbandPerson = people.Find(husband);
        Person wifePerson = people.Find(wife);

        // Add any marriage / divoirce details.
        ImportMarriage(husbandPerson, wifePerson, node);

        // Import the children.
        foreach (string child in children)
        {
          // Get the Person object for the child.
          Person childPerson = people.Find(child);

          // Calling RelationshipHelper.AddChild hooks up all of the
          // child relationships for the husband and wife. Also hooks up
          // the sibling relationships.
          if (husbandPerson != null && childPerson != null)
          {
            RelationshipHelper.AddChild(people, husbandPerson, childPerson);
          }

          if (husbandPerson == null && wifePerson != null & childPerson != null)
          {
            RelationshipHelper.AddChild(people, wifePerson, childPerson);
          }
        }
      }
    }

    /// <summary>
    /// Update the marriage / divorce information for the two people.
    /// </summary>
    private static void ImportMarriage(Person husband, Person wife, XmlNode node)
    {
      // Return right away if there are not two people.
      if (husband == null || wife == null)
      {
        return;
      }

      // See if a marriage (or divorce) is specified.
      if (node.SelectSingleNode("MARR") != null || node.SelectSingleNode("DIV") != null)
      {
        // Get dates.
        DateTime? marriageDate = GetValueDate(node, "MARR/DATE");
        DateTime? divorceDate = GetValueDate(node, "DIV/DATE");
        SpouseModifier modifier = GetDivorced(node) ? SpouseModifier.Former : SpouseModifier.Current;

        // Add info to husband.
        if (husband.GetSpouseRelationship(wife) == null)
        {
          SpouseRelationship husbandMarriage = new SpouseRelationship(wife, modifier);
          husbandMarriage.MarriageDate = marriageDate;
          husbandMarriage.DivorceDate = divorceDate;
          husband.Relationships.Add(husbandMarriage);
        }

        // Add info to wife.
        if (wife.GetSpouseRelationship(husband) == null)
        {
          SpouseRelationship wifeMarriage = new SpouseRelationship(husband, modifier);
          wifeMarriage.MarriageDate = marriageDate;
          wifeMarriage.DivorceDate = divorceDate;
          wife.Relationships.Add(wifeMarriage);
        }
      }
    }

    /// <summary>
    /// Import photo information from the GEDCOM XML file.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private static void ImportPhotos(Person person, XmlNode node)
    {
      try
      {
        // Get list of photos for this person.
        string[] photos = GetPhotos(node);
        if (photos == null || photos.Length == 0)
        {
          return;
        }

        // Import each photo. Make the first photo specified
        // the default photo (avatar).
        for (int i = 0; i < photos.Length; i++)
        {
          Photo photo = new Photo(photos[i]);
          photo.IsAvatar = (i == 0) ? true : false;
          person.Photos.Add(photo);
        }
      }
      catch
      {
        // There was an error importing a photo, ignore 
        // and continue processing the GEDCOM XML file.
      }
    }

    /// <summary>
    /// Import the note info from the GEDCOM XMl file.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private static void ImportNote(Person person, XmlNode node)
    {
      try
      {
        string value = GetValue(node, "NOTE");
        if (!string.IsNullOrEmpty(value))
        {
          person.Story = new Story();
          string storyFileName = new StringBuilder(person.Name).Append(".rtf").ToString();
          person.Story.Save(value, storyFileName);
        }
      }
      catch
      {
        // There was an error importing the note, ignore
        // and continue processing the GEDCOM XML file.
      }
    }

    /// <summary>
    /// Import the birth info from the GEDCOM XML file.
    /// </summary>
    private static void ImportBirth(Person person, XmlNode node)
    {
      person.BirthDate = GetValueDate(node, "BIRT/DATE");
      person.BirthPlace = GetValue(node, "BIRT/PLAC");
    }

    /// <summary>
    /// Import the death info from the GEDCOM XML file.
    /// </summary>
    private static void ImportDeath(Person person, XmlNode node)
    {
      person.IsLiving = (node.SelectSingleNode("DEAT") == null) ? true : false;
      person.DeathDate = GetValueDate(node, "DEAT/DATE");
      person.DeathPlace = GetValue(node, "DEAT/PLAC");
    }

    /// <summary>
    /// Return a list of photos specified in the GEDCOM XML file.
    /// </summary>
    private static string[] GetPhotos(XmlNode node)
    {
      string[] photos;
      XmlNodeList list = node.SelectNodes("OBJE");
      photos = new string[list.Count];

      for (int i = 0; i < list.Count; i++)
      {
        photos[i] = GetFile(list[i]);
      }

      return photos;
    }

    private static string GetSuffix(XmlNode node)
    {
      return GetValue(node, "NAME/NPFX");
    }

    private static string GetMarriedName(XmlNode node)
    {
      return GetValue(node, "NAME/_MARNM");
    }

    private static string GetNickName(XmlNode node)
    {
      return GetValue(node, "NAME/NICK");
    }

    private static string GetHusbandID(XmlNode node)
    {
      return GetValueId(node, "HUSB");
    }

    private static string GetWifeID(XmlNode node)
    {
      return GetValueId(node, "WIFE");
    }

    private static Gender GetGender(XmlNode node)
    {
      string value = GetValue(node, "SEX");
      if (string.Compare(value, "f", true, CultureInfo.InvariantCulture) == 0)
      {
        return Gender.Female;
      }

      return Gender.Male;
    }

    private static bool GetDivorced(XmlNode node)
    {
      string value = GetValue(node, "DIV");
      if (string.Compare(value, "n", true, CultureInfo.InvariantCulture) == 0)
      {
        return false;
      }

      // Divorced if the tag exists.
      return node.SelectSingleNode("DIV") != null ? true : false;
    }

    private static string GetFile(XmlNode node)
    {
      return GetValue(node, "FILE");
    }

    private static string[] GetChildrenIDs(XmlNode node)
    {
      string[] children;
      XmlNodeList list = node.SelectNodes("CHIL");
      children = new string[list.Count];

      for (int i = 0; i < list.Count; i++)
      {
        children[i] = GetId(list[i]);
      }

      return children;
    }

    private static string GetFirstName(XmlNode node)
    {
      string name = GetValue(node, "NAME");
      string[] parts = name.Split('/');
      if (parts.Length > 0)
      {
        return parts[0].Trim();
      }

      return string.Empty;
    }

    private static string GetLastName(XmlNode node)
    {
      string name = GetValue(node, "NAME");
      string[] parts = name.Split('/');
      if (parts.Length > 1)
      {
        return parts[1].Trim();
      }

      return string.Empty;
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private static DateTime? GetValueDate(XmlNode node, string xpath)
    {
      DateTime? result = null;

      try
      {
        string value = GetValue(node, xpath);
        if (!string.IsNullOrEmpty(value))
        {
          result = DateTime.Parse(value, CultureInfo.InvariantCulture);
        }
      }
      catch
      {
        // The date is invalid, ignore and continue processing.
      }

      return result;
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private static string GetId(XmlNode node)
    {
      try
      {
        return node.Attributes["Value"].Value.Replace('@', ' ').Trim();
      }
      catch
      {
        // Invalid line, keep processing the file.
        return string.Empty;
      }
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private static string GetValueId(XmlNode node, string xpath)
    {
      string result = string.Empty;
      try
      {
        XmlNode valueNode = node.SelectSingleNode(xpath);
        if (valueNode != null)
        {
          result = valueNode.Attributes["Value"].Value.Replace('@', ' ').Trim();
        }
      }
      catch
      {
        // Invalid line, keep processing the file.
      }
      return result;
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private static string GetValue(XmlNode node, string xpath)
    {
      string result = string.Empty;
      try
      {
        XmlNode valueNode = node.SelectSingleNode(xpath);
        if (valueNode != null)
        {
          result = valueNode.Attributes["Value"].Value.Trim();
        }
      }
      catch
      {
        // Invalid line, keep processing the file.
      }
      return result;
    }
  }
}