using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        GenealogicalNumber
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

        string GetValue(Person rootPers);
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

        public string GetValue(Person rootPers)
        {
            switch (Type)
            {
                case ExportTagPersonType.Root:
                    return string.Empty;
                case ExportTagPersonType.BirthDate:
                    return rootPers.BirthDate?.ToString("dd/MM/yyyy");
                case ExportTagPersonType.BirthPlace:
                    return rootPers.BirthPlace;
                case ExportTagPersonType.DeathDate:
                    break;
                case ExportTagPersonType.DeathPlace:
                    break;
                case ExportTagPersonType.LastName:
                    return rootPers.LastName;
                case ExportTagPersonType.FirstName:
                    return rootPers.FirstName;
                case ExportTagPersonType.FatherFirstName:
                    break;
                case ExportTagPersonType.FatherLastName:
                    break;
                case ExportTagPersonType.MotherFirstName:
                    break;
                case ExportTagPersonType.MotherLastName:
                    break;
                case ExportTagPersonType.OrderIntoSiblings:
                    break;
                case ExportTagPersonType.GenealogicalNumber:
                    break;
                default:
                    break;
            }

            return "NO YET IMPLEMENTED";
        }
    }
}
