using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.FamilyShowLib
{
    /// <summary>
    /// Contains the relationship logic rules for adding people and how they relate to each other.
    /// </summary>
    public static class RelationshipHelper
    {
        /// <summary>
        /// Performs the business logic for adding the Child relationship between the person and the child.
        /// </summary>
        public static void AddChild(PeopleCollection family, Person person, Person child)
        {
            // Add the new child as a sibling to any existing children
            foreach (Person existingSibling in person.Children)
            {
                family.AddSibling(existingSibling, child);
            }

            switch (person.Spouses.Count)
            {
                // Single parent, add the child to the person
                case 0:
                    family.AddChild(person, child, ParentChildModifier.Natural);
                    break;

                // Has existing spouse, add the child to the person's spouse as well.
                case 1:
                    family.AddChild(person, child, ParentChildModifier.Natural);
                    family.AddChild(person.Spouses[0], child, ParentChildModifier.Natural);
                    break;
            }
        }

        /// <summary>
        /// Performs the business logic for adding the Parent relationship between the person and the parent.
        /// </summary>
        public static void AddParent(PeopleCollection family, Person person, Person parent)
        {
            // A person can only have 2 parents, do nothing
            if (person.Parents.Count == 2)
                return;
            
            // Add the parent to the main collection of people.
            family.Add(parent);

            switch (person.Parents.Count)
            {
                // No exisitng parents
                case 0: 
                    family.AddChild(parent, person, ParentChildModifier.Natural);
                    break;

                // An existing parent
                case 1: 
                    family.AddChild(parent, person, ParentChildModifier.Natural);
                    family.AddSpouse(parent, person.Parents[0], SpouseModifier.Current);
                    break;
            }

            // Handle siblings
            if (person.Siblings.Count > 0)
            {
                // Make siblings the child of the new parent
                foreach (Person sibling in person.Siblings)
                {
                    family.AddChild(parent, sibling, ParentChildModifier.Natural);
                }
            }

            // Setter for property change notification
            person.HasParents = true;
        }

        /// <summary>
        /// Performs the business logic for adding the Parent relationship between the person and the parents.
        /// </summary>
        public static void AddParent(PeopleCollection family, Person person, ParentSet parentSet)
        {
            // First add child to parents.
            family.AddChild(parentSet.FirstParent, person, ParentChildModifier.Natural);
            family.AddChild(parentSet.SecondParent, person, ParentChildModifier.Natural);
            
            // Next update the siblings. Get the list of full siblings for the person. 
            // A full sibling is a sibling that has both parents in common. 
            List<Person> siblings = GetChildren(parentSet);
            foreach (Person sibling in siblings)
            {
                if (sibling != person)
                    family.AddSibling(person, sibling);
            }
        }
        
        /// <summary>
        /// Return a list of children for the parent set.
        /// </summary>
        private static List<Person> GetChildren(ParentSet parentSet)
        {
            // Get list of both parents.
            List<Person> firstParentChildren = new List<Person>(parentSet.FirstParent.Children);
            List<Person> secondParentChildren = new List<Person>(parentSet.SecondParent.Children);
            
            // Combined children list that is returned.
            List<Person> children = new List<Person>();

            // Go through and add the children that have both parents.            
            foreach (Person child in firstParentChildren)
            {
                if (secondParentChildren.Contains(child))
                    children.Add(child);
            }

            return children;
        }

        /// <summary>
        /// Performs the business logic for adding the Spousal relationship between the person and the spouse.
        /// </summary>
        public static void AddSpouse(PeopleCollection family, Person person, Person spouse, SpouseModifier modifier)
        {
            // Assume the spouse's gender based on the counterpart of the person's gender
            if (person.Gender == Gender.Male)
                spouse.Gender = Gender.Female;
            else
                spouse.Gender = Gender.Male;

            if (person.Spouses != null)
            {
                switch (person.Spouses.Count)
                {
                    // No existing spouse	
                    case 0:
                        family.AddSpouse(person, spouse, modifier);

                        // Add any of the children as the child of the spouse.
                        if (person.Children != null || person.Children.Count > 0)
                        {
                            foreach (Person child in person.Children)
                            {
                                family.AddChild(spouse, child, ParentChildModifier.Natural);
                            }
                        }
                        break;

                    // Existing spouse(s)
                    default:
                        // If specifying a new married spouse, make existing spouses former.
                        if (modifier == SpouseModifier.Current)
                        {
                            foreach (Relationship relationship in person.Relationships)
                            {
                                if (relationship.RelationshipType == RelationshipType.Spouse)
                                    ((SpouseRelationship)relationship).SpouseModifier = SpouseModifier.Former;
                            }
                        }
                        
                        family.AddSpouse(person, spouse, modifier);
                        break;
                }

                // Setter for property change notification
                person.HasSpouse = true;
            }
        }

        /// <summary>
        /// Performs the business logic for adding the Sibling relationship between the person and the sibling.
        /// </summary>
        public static void AddSibling(PeopleCollection family, Person person, Person sibling)
        {
            // Handle siblings
            if (person.Siblings.Count > 0)
            {
                // Make the siblings siblings to each other.
                foreach (Person existingSibling in person.Siblings)
                {
                    family.AddSibling(existingSibling, sibling);
                }
            }

            if (person.Parents != null)
            {
                switch (person.Parents.Count)
                {
                    // No parents
                    case 0:
                        family.AddSibling(person, sibling);
                        break;

                    // Single parent
                    case 1:
                        family.AddSibling(person, sibling);
                        family.AddChild(person.Parents[0], sibling, ParentChildModifier.Natural);
                        break;

                    // 2 parents
                    case 2:
                        // Add the sibling as a child of the same parents
                        foreach (Person parent in person.Parents)
                        {
                            family.AddChild(parent, sibling, ParentChildModifier.Natural);
                        }

                        family.AddSibling(person, sibling);
                        break;

                    default:
                        family.AddSibling(person, sibling);
                        break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the spouse status
        /// </summary>
        public static void UpdateSpouseStatus(Person person, Person spouse, SpouseModifier modifier)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).SpouseModifier = modifier;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).SpouseModifier = modifier;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the marriage date
        /// </summary>
        public static void UpdateMarriageDate(Person person, Person spouse, DateTime? dateTime)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).MarriageDate = dateTime;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).MarriageDate = dateTime;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the divorce date
        /// </summary>
        public static void UpdateDivorceDate(Person person, Person spouse, DateTime? dateTime)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).DivorceDate = dateTime;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).DivorceDate = dateTime;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for changing the person parents
        /// </summary>
        public static void ChangeParents(PeopleCollection family, Person person, ParentSet newParentSet)
        {
            // Don't do anything if there is nothing to change or if the parents are the same
            if (person.ParentSet == null || newParentSet == null || person.ParentSet.Equals(newParentSet))
                return;

            // store the current parent set which will be removed
            ParentSet formerParentSet = person.ParentSet;

            // Remove the first parent
            RemoveParentChildRelationship(person, formerParentSet.FirstParent);

            // Remove the person as a child of the second parent
            RemoveParentChildRelationship(person, formerParentSet.SecondParent);

            // Remove the sibling relationships
            RemoveSiblingRelationships(person);

            // Add the new parents
            AddParent(family, person, newParentSet);
        }

        /// <summary>
        /// Helper function for removing sibling relationships
        /// </summary>
        private static void RemoveSiblingRelationships(Person person)     
        {
            for (int i = person.Relationships.Count - 1; i >= 0; i--)
            {
                if (person.Relationships[i].RelationshipType == RelationshipType.Sibling)
                    person.Relationships.RemoveAt(i);
            }
        }

        /// <summary>
        /// Helper function for removing a parent relationship
        /// </summary>
        private static void RemoveParentChildRelationship(Person person, Person parent)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Parent && relationship.RelationTo.Equals(parent))
                {
                    person.Relationships.Remove(relationship);
                    break;
                }
            }

            foreach (Relationship relationship in parent.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Child && relationship.RelationTo.Equals(person))
                {
                    parent.Relationships.Remove(relationship);
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for changing the deleting the person
        /// </summary>
        public static void DeletePerson(PeopleCollection family, Person personToDelete)
        {
            if (!personToDelete.IsDeletable)
                return;

            // Remove the personToDelete from the relationships that contains the personToDelete.
            foreach (Relationship relationship in personToDelete.Relationships)
            {
                foreach (Relationship rel in relationship.RelationTo.Relationships)
                {
                    if (rel.RelationTo.Equals(personToDelete))
                    {
                        relationship.RelationTo.Relationships.Remove(rel);
                        break;
                    }                    
                } 
            }

            // Delete the person's photos and story
            personToDelete.DeletePhotos();
            personToDelete.DeleteStory();

            family.Remove(personToDelete);
        }
    }
}
