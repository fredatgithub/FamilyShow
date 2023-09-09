using System.Collections.Generic;

namespace Microsoft.FamilyShowLib
{
  public enum ExportTagPersonType
  {
    Root,
    BirthDate,
    BirthPlace,
    DeathDate,
    DeathPlace,
    LastName,
    FirstName,
    FatherFirstName,
    FatherLastName,
    MotherFirstName,
    MotherLastName,
    OrderIntoSiblings,
    GenealogicalNumber,
    MariageDate,
    MariagePlace,
    MariagePartnerGenre,
    MariagePartnerFirstName,
    MariagePartnerLastName
  }

  public enum ExportTagGenerationType
  {
    Root,
    NumberBirth,
    NumberDeath,
    NumberPerson
  }

  public interface IExportTag
  {
    string Name { get; set; }
    List<IExportTag> Childs { get; set; }

    string GetValue(Person rootPers, string genealogicalNumber, int childNumber);
  }

  public class ExportTagPerson : IExportTag
  {
    public string Name { get; set; }

    public List<IExportTag> Childs { get; set; }

    public ExportTagPersonType Type { get; set; }
    //public ExportTagGenerationType? GenerationType { get; set; }


    public ExportTagPerson(string name, ExportTagPersonType persontype)
    {
      Childs = new List<IExportTag>();
      Name = name;
      Type = persontype;
    }

    public string GetValue(Person rootPers, string genealogicalNumber, int childNumber)
    {
      switch (Type)
      {
        case ExportTagPersonType.Root:
          return string.Empty;

        case ExportTagPersonType.BirthDate:
          return rootPers.BirthDate?.ToString("dd MMMM yyyy");

        case ExportTagPersonType.BirthPlace:
          return rootPers.BirthPlace;

        case ExportTagPersonType.DeathDate:
          return rootPers.DeathDate?.ToString("dd MMMM yyyy");

        case ExportTagPersonType.DeathPlace:
          return rootPers.DeathPlace;

        case ExportTagPersonType.LastName:
          return rootPers.LastName;

        case ExportTagPersonType.FirstName:
          return rootPers.FirstName;

        case ExportTagPersonType.FatherFirstName:
          return rootPers.Father?.FirstName;

        case ExportTagPersonType.FatherLastName:
          return rootPers.Father?.LastName;

        case ExportTagPersonType.MotherFirstName:
          return rootPers.Mother?.FirstName;

        case ExportTagPersonType.MotherLastName:
          return rootPers.Mother?.LastName;

        case ExportTagPersonType.OrderIntoSiblings:
          return $"{childNumber}";

        case ExportTagPersonType.GenealogicalNumber:
          return genealogicalNumber;

        default:
          break;
      }

      return "NOT YET IMPLEMENTED";
    }

    public string GetValue(Person rootPers, SpouseRelationship spouseRelationShip, string genealogicalNumber, int childNumber)
    {
      switch (Type)
      {
        case ExportTagPersonType.MariageDate:
          return spouseRelationShip.MarriageDate?.ToString("dd MMMM yyyy");
        case ExportTagPersonType.MariagePlace:
          return spouseRelationShip.MarriagePlace;
        case ExportTagPersonType.MariagePartnerGenre:
          break;
        case ExportTagPersonType.MariagePartnerFirstName:
          break;
        case ExportTagPersonType.MariagePartnerLastName:
          break;
        default:
          break;
      }

      return "NOT YET IMPLEMENTED";
    }

    internal static List<SpouseRelationship> ListSpouseRelationShip(Person person, int startYear)
    {
      List<SpouseRelationship> listOfSpouse = new List<SpouseRelationship>();

      // looking for all relationships
      foreach (Relationship relationship in person.Relationships)
      {
        if (relationship.RelationshipType == RelationshipType.Spouse)
        {
          SpouseRelationship spouseRelationship = ((SpouseRelationship)relationship);

          if (spouseRelationship.MarriageDate != null && spouseRelationship.MarriageDate?.Year >= startYear)
          {
            listOfSpouse.Add(spouseRelationship);
          }
        }
      }

      return listOfSpouse;
    }
  }
}
