using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace FamilyShowLib
{
  /// <summary>
  /// Representation for a single serializable Person.
  /// INotifyPropertyChanged allows properties of the Person class to participate as source in data bindings.
  /// </summary>
  [Serializable]
  public class Person : INotifyPropertyChanged, IEquatable<Person>, IDataErrorInfo
  {
    #region Fields and Constants

    // The constants specific to this class
    private static class Const
    {
      public const string DefaultFirstName = "Unknown";
    }

    private string id;
    private string firstName;
    private string lastName;
    private string middleName;
    private string suffix;
    private string nickName;
    private string marriedName;
    private Gender gender;
    private DateTime? birthDate;
    private string birthPlace;
    private DateTime? deathDate;
    private string deathPlace;
    private bool isLiving;
    private PhotoCollection photos;
    private Story story;
    private RelationshipCollection relationships;
    private Contact contact;
    private EventBaptism baptism;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the unique identifier for each person.
    /// </summary>
    [XmlAttribute]
    public string Id
    {
      get { return id; }
      set
      {
        if (id != value)
        {
          id = value;
          OnPropertyChanged(nameof(Id));
        }
      }
    }

    /// <summary>
    /// Gets or sets the name that occurs first in a given name
    /// </summary>
    public string FirstName
    {
      get { return firstName; }
      set
      {
        if (firstName != value)
        {
          firstName = value;
          OnPropertyChanged(nameof(FirstName));
          OnPropertyChanged(nameof(Name));
          OnPropertyChanged(nameof(FullName));
        }
      }
    }

    /// <summary>
    /// Gets or sets the part of a given name that indicates what family the person belongs to. 
    /// </summary>
    public string LastName
    {
      get { return lastName; }
      set
      {
        if (lastName != value)
        {
          lastName = value;
          OnPropertyChanged(nameof(LastName));
          OnPropertyChanged(nameof(Name));
          OnPropertyChanged(nameof(FullName));
        }
      }
    }

    /// <summary>
    /// Gets or sets the name that occurs between the first and last name.
    /// </summary>
    public string MiddleName
    {
      get { return middleName; }
      set
      {
        if (middleName != value)
        {
          middleName = value;
          OnPropertyChanged(nameof(MiddleName));
          OnPropertyChanged(nameof(FullName));
        }
      }
    }

    /// <summary>
    /// Gets the person's name in the format FirstName LastName.
    /// </summary>
    public string Name
    {
      get
      {
        string name = string.Empty;
        if (!string.IsNullOrEmpty(firstName))
        {
          name += firstName;
        }

        if (!string.IsNullOrEmpty(lastName))
        {
          name += " " + lastName;
        }

        return name;
      }
    }

    /// <summary>
    /// Gets the person's fully qualified name: Firstname MiddleName LastName Suffix
    /// </summary>
    public string FullName
    {
      get
      {
        string fullName = string.Empty;
        if (!string.IsNullOrEmpty(firstName))
        {
          fullName += firstName;
        }

        if (!string.IsNullOrEmpty(middleName))
        {
          fullName += " " + middleName;
        }

        if (!string.IsNullOrEmpty(lastName))
        {
          fullName += " " + lastName;
        }

        if (!string.IsNullOrEmpty(suffix))
        {
          fullName += " " + suffix;
        }

        return fullName;
      }
    }

    /// <summary>
    /// Gets or sets the text that appear behind the last name providing additional information about the person.
    /// </summary>
    public string Suffix
    {
      get { return suffix; }
      set
      {
        if (suffix != value)
        {
          suffix = value;
          OnPropertyChanged(nameof(Suffix));
          OnPropertyChanged(nameof(FullName));
        }
      }
    }

    /// <summary>
    /// Gets or sets the person's familiar or shortened name
    /// </summary>
    public string NickName
    {
      get { return nickName; }
      set
      {
        if (nickName != value)
        {
          nickName = value;
          OnPropertyChanged(nameof(NickName));
        }
      }
    }

    /// <summary>
    /// Gets or sets the person's name carried before marriage
    /// </summary>
    public string MarriedName
    {
      get { return marriedName; }
      set
      {
        if (marriedName != value)
        {
          marriedName = value;
          OnPropertyChanged(nameof(MarriedName));
        }
      }
    }

    /// <summary>
    /// Gets or sets the person's gender
    /// </summary>
    public Gender Gender
    {
      get { return gender; }
      set
      {
        if (gender != value)
        {
          gender = value;
          OnPropertyChanged(nameof(Gender));
        }
      }
    }

    /// <summary>
    /// The age of the person.
    /// </summary>
    public int? Age
    {
      get
      {
        if (BirthDate == null)
        {
          return null;
        }

        // Determine the age of the person based on just the year.
        DateTime startDate = BirthDate.Value;
        DateTime endDate = (IsLiving || DeathDate == null) ? DateTime.Now : DeathDate.Value;
        int age = endDate.Year - startDate.Year;

        // Compensate for the month and day of month (if they have not had a birthday this year).
        if (endDate.Month < startDate.Month || (endDate.Month == startDate.Month && endDate.Day < startDate.Day))
        {
          age--;
        }

        return Math.Max(0, age);
      }
    }

    /// <summary>
    /// The age of the person.
    /// </summary>
    [XmlIgnore]
    public AgeGroup AgeGroup
    {
      get
      {
        AgeGroup ageGroup = AgeGroup.Unknown;

        if (Age.HasValue)
        {
          // The AgeGroup enumeration is defined later in this file. It is up to the Person
          // class to define the ages that fall into the particular age groups
          if (Age >= 0 && Age < 20)
          {
            ageGroup = AgeGroup.Youth;
          }
          else if (Age >= 20 && Age < 40)
          {
            ageGroup = AgeGroup.Adult;
          }
          else if (Age >= 40 && Age < 65)
          {
            ageGroup = AgeGroup.MiddleAge;
          }
          else
          {
            ageGroup = AgeGroup.Senior;
          }
        }

        return ageGroup;
      }
    }

    /// <summary>
    /// The year the person was born
    /// </summary>
    public string YearOfBirth
    {
      get
      {
        if (birthDate.HasValue)
        {
          return birthDate.Value.Year.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
          return "-";
        }
      }
    }

    /// <summary>
    /// The year the person died
    /// </summary>
    public string YearOfDeath
    {
      get
      {
        if (deathDate.HasValue && !isLiving)
        {
          return deathDate.Value.Year.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
          return "-";
        }
      }
    }

    /// <summary>
    /// Gets or sets the person's birth date.  This property can be null.
    /// </summary>
    public DateTime? BirthDate
    {
      get { return birthDate; }
      set
      {
        if (birthDate == null || birthDate != value)
        {
          birthDate = value;
          OnPropertyChanged(nameof(BirthDate));
          OnPropertyChanged(nameof(Age));
          OnPropertyChanged(nameof(AgeGroup));
          OnPropertyChanged(nameof(YearOfBirth));
          OnPropertyChanged(nameof(BirthMonthAndDay));
          OnPropertyChanged(nameof(BirthDateAndPlace));
        }
      }
    }

    /// <summary>
    /// Gets or sets the person's place of birth
    /// </summary>
    public string BirthPlace
    {
      get { return birthPlace; }
      set
      {
        if (birthPlace != value)
        {
          birthPlace = value;
          OnPropertyChanged(nameof(BirthPlace));
          OnPropertyChanged(nameof(BirthDateAndPlace));
        }
      }
    }

    /// <summary>
    /// Gets the month and day the person was born in. This property can be null.
    /// </summary>
    [XmlIgnore]
    public string BirthMonthAndDay
    {
      get
      {
        if (birthDate == null)
        {
          return null;
        }
        else
        {
          return birthDate.Value.ToString(DateTimeFormatInfo.CurrentInfo.MonthDayPattern, CultureInfo.CurrentCulture);
        }
      }
    }

    /// <summary>
    /// Gets a friendly string for BirthDate and Place
    /// </summary>
    [XmlIgnore]
    public string BirthDateAndPlace
    {
      get
      {
        if (birthDate == null)
        {
          return null;
        }
        else
        {
          StringBuilder returnValue = new StringBuilder();
          returnValue.Append("Born ");
          returnValue.Append(birthDate.Value.ToString(DateTimeFormatInfo.CurrentInfo.ShortDatePattern, CultureInfo.CurrentCulture));

          if (!string.IsNullOrEmpty(birthPlace))
          {
            returnValue.Append(", ");
            returnValue.Append(birthPlace);
          }

          return returnValue.ToString();
        }
      }
    }

    /// <summary>
    /// Gets or sets the person's death of death.  This property can be null.
    /// </summary>
    public DateTime? DeathDate
    {
      get { return deathDate; }
      set
      {
        if (deathDate == null || deathDate != value)
        {
          deathDate = value;
          OnPropertyChanged(nameof(DeathDate));
          OnPropertyChanged(nameof(Age));
          OnPropertyChanged(nameof(YearOfDeath));
        }
      }
    }

    /// <summary>
    /// Gets or sets the person's place of death
    /// </summary>
    public string DeathPlace
    {
      get { return deathPlace; }
      set
      {
        if (deathPlace != value)
        {
          deathPlace = value;
          OnPropertyChanged(nameof(DeathPlace));
        }
      }
    }

    /// <summary>
    /// Gets or sets whether the person is still alive or deceased.
    /// </summary>
    public bool IsLiving
    {
      get { return isLiving; }
      set
      {
        if (isLiving != value)
        {
          isLiving = value;
          OnPropertyChanged(nameof(IsLiving));
        }
      }
    }

    /// <summary>
    /// Gets or sets the photos associated with the person
    /// </summary>
    public PhotoCollection Photos
    {
      get { return photos; }
    }


    /// <summary>
    /// Get or set the contact for the person (addresse, phone, mail)
    /// </summary>
    public Contact Contact
    {
      get { return contact; }
      set
      {
        if (contact != value)
        {
          contact = value;
          OnPropertyChanged(nameof(Contact));
        }
      }
    }


    public EventBaptism Baptism
    {
      get { return baptism; }
      set
      {
        if (baptism != value)
        {
          baptism = value;
          OnPropertyChanged(nameof(Baptism));
        }
      }
    }

    /// <summary>
    /// Gets or sets the person's story file.
    /// </summary>
    public Story Story
    {
      get { return story; }
      set
      {
        if (story != value)
        {
          story = value;
          OnPropertyChanged(nameof(Story));
        }
      }
    }

    /// <summary>
    /// Gets or sets the person's graphical identity
    /// </summary>
    [XmlIgnore]
    public string Avatar
    {
      get
      {
        string avatar = string.Empty;

        if (photos != null && photos.Count > 0)
        {
          foreach (Photo photo in photos)
          {
            if (photo.IsAvatar)
            {
              return photo.FullyQualifiedPath;
            }
          }
        }

        return avatar;
      }
      set
      {
        // This setter is used for change notification.
        OnPropertyChanged(nameof(Avatar));
        OnPropertyChanged(nameof(HasAvatar));
      }
    }

    /// <summary>
    /// Determines whether a person is deletable.
    /// </summary>
    [XmlIgnore]
    public bool IsDeletable
    {
      get
      {
        // With a few exceptions, anyone with less than 3 relationships is deletable
        if (relationships.Count < 3)
        {
          // The person has 2 spouses. Since they connect their spouses, they are not deletable.
          if (Spouses.Count == 2)
          {
            return false;
          }

          // The person is connecting two generations
          if (Parents.Count == 1 && Children.Count == 1)
          {
            return false;
          }

          // The person is connecting inlaws
          if (Parents.Count == 1 && Spouses.Count == 1)
          {
            return false;
          }

          // Anyone else with less than 3 relationships is deletable
          return true;
        }

        // More than 3 relationships, however the relationships are from only Children. 
        if (Children.Count > 0 && Parents.Count == 0 && Siblings.Count == 0 && Spouses.Count == 0)
        {
          return true;
        }

        // More than 3 relationships. The relationships are from siblings. Deletable since siblings are connected to each other or the parent.
        if (Siblings.Count > 0 && Parents.Count >= 0 && Spouses.Count == 0 && Children.Count == 0)
        {
          return true;
        }

        // This person has complicated dependencies that does not allow deletion.
        return false;
      }
    }

    /// <summary>
    /// Collections of relationship connection for the person
    /// </summary>
    public RelationshipCollection Relationships
    {
      get { return relationships; }
    }

    /// <summary>
    /// Accessor for the person's spouse(s)
    /// </summary>
    [XmlIgnore]
    public Collection<Person> Spouses
    {
      get
      {
        Collection<Person> spouses = new Collection<Person>();
        foreach (Relationship relationship in relationships)
        {
          if (relationship.RelationshipType == RelationshipType.Spouse)
          {
            spouses.Add(relationship.RelationTo);
          }
        }

        return spouses;
      }
    }

    [XmlIgnore]
    public Collection<Person> CurrentSpouses
    {
      get
      {
        Collection<Person> spouses = new Collection<Person>();
        foreach (Relationship relationship in relationships)
        {
          if (relationship.RelationshipType == RelationshipType.Spouse)
          {
            SpouseRelationship spouseRel = relationship as SpouseRelationship;

            if (spouseRel != null && spouseRel.SpouseModifier == SpouseModifier.Current)
            {
              spouses.Add(relationship.RelationTo);
            }
          }
        }

        return spouses;
      }
    }

    [XmlIgnore]
    public Collection<Person> PreviousSpouses
    {
      get
      {
        Collection<Person> spouses = new Collection<Person>();
        foreach (Relationship rel in relationships)
        {
          if (rel.RelationshipType == RelationshipType.Spouse)
          {
            SpouseRelationship spouseRel = rel as SpouseRelationship;

            if (spouseRel != null && spouseRel.SpouseModifier == SpouseModifier.Former)
            {
              spouses.Add(rel.RelationTo);
            }
          }
        }

        return spouses;
      }
    }

    [XmlIgnore]
    public List<SpouseRelationship> ListSpousesRelationShip
    {
      get
      {
        var lst = new List<SpouseRelationship>();

        // Search into all relationship
        foreach (Relationship rel in this.Relationships)
        {
          if (rel.RelationshipType == RelationshipType.Spouse)
          {
            SpouseRelationship spouseRel = ((SpouseRelationship)rel);
            lst.Add(spouseRel);
          }
        }

        return lst;
      }
    }

    /// <summary>
    /// Accessor for the person's children
    /// </summary>
    [XmlIgnore]
    public Collection<Person> Children
    {
      get
      {
        Collection<Person> children = new Collection<Person>();
        foreach (Relationship relationship in relationships)
        {
          if (relationship.RelationshipType == RelationshipType.Child)
          {
            children.Add(relationship.RelationTo);
          }
        }

        return children;
      }
    }

    /// <summary>
    /// Accessor for all of the person's parents
    /// </summary>
    [XmlIgnore]
    public Collection<Person> Parents
    {
      get
      {
        Collection<Person> parents = new Collection<Person>();
        foreach (Relationship relationship in relationships)
        {
          if (relationship.RelationshipType == RelationshipType.Parent)
          {
            parents.Add(relationship.RelationTo);
          }
        }

        return parents;
      }
    }

    /// <summary>
    /// Accessor for the person's siblings
    /// </summary>
    [XmlIgnore]
    public Collection<Person> Siblings
    {
      get
      {
        Collection<Person> siblings = new Collection<Person>();
        foreach (Relationship relationship in relationships)
        {
          if (relationship.RelationshipType == RelationshipType.Sibling)
          {
            siblings.Add(relationship.RelationTo);
          }
        }

        return siblings;
      }
    }

    /// <summary>
    /// Accessor for the person's half siblings. A half sibling is a person
    /// that contains one or more same parents as the person, but does not 
    /// contain all of the same parents.
    /// </summary>
    [XmlIgnore]
    public Collection<Person> HalfSiblings
    {
      get
      {
        // List that is returned.
        Collection<Person> halfSiblings = new Collection<Person>();

        // Get list of full siblings (a full sibling cannot be a half sibling).
        Collection<Person> siblings = Siblings;

        // Iterate through each parent, and determine if the parent's children
        // are half siblings.
        foreach (Person parent in Parents)
        {
          foreach (Person child in parent.Children)
          {
            if (child != this && !siblings.Contains(child) && !halfSiblings.Contains(child))
            {
              halfSiblings.Add(child);
            }
          }
        }

        return halfSiblings;
      }
    }

    /// <summary>
    /// Get the person's parents as a ParentSet object
    /// </summary>
    [XmlIgnore]
    public ParentSet ParentSet
    {
      get
      {
        // Only need to get the parent set if there are two parents.
        if (Parents.Count == 2)
        {
          ParentSet parentSet = new ParentSet(Parents[0], Parents[1]);
          return parentSet;
        }
        else
        {
          return null;
        }
      }
    }

    /// <summary>
    /// Get the possible combination of parents when editting this person or adding this person's sibling.
    /// </summary>
    [XmlIgnore]
    public ParentSetCollection PossibleParentSets
    {
      get
      {
        ParentSetCollection parentSets = new ParentSetCollection();

        foreach (Person parent in Parents)
        {
          foreach (Person spouse in parent.Spouses)
          {
            ParentSet parentSet = new ParentSet(parent, spouse);

            // Don't add the same parent set twice.
            if (!parentSets.Contains(parentSet))
            {
              parentSets.Add(parentSet);
            }
          }
        }

        return parentSets;
      }
    }

    /// <summary>
    /// Get the Father of the person
    /// </summary>
    public Person Father
    {
      get
      {
        foreach (Person item in Parents)
        {
          if (item.Gender == Gender.Male)
            return item;
        }
        return null;
      }
    }

    /// <summary>
    /// Get the mother of the person
    /// </summary>
    public Person Mother
    {
      get
      {
        foreach (Person item in Parents)
        {
          if (item.Gender == Gender.Female)
            return item;
        }
        return null;
      }
    }

    /// <summary>
    /// Calculated property that returns whether the person has 2 or more parents.
    /// </summary>
    public bool HasParents
    {
      get
      {
        return (Parents.Count >= 2);
      }
      set
      {
        // This setter is used for change notification.
        OnPropertyChanged(nameof(HasParents));
        OnPropertyChanged(nameof(PossibleParentSets));
      }
    }

    /// <summary>
    /// Calculated property that returns whether the person has 1 or more spouse(s).
    /// </summary>
    public bool HasSpouse
    {
      get
      {
        return (Spouses.Count >= 1);
      }
      set
      {
        // This setter is used for change notification.
        OnPropertyChanged(nameof(HasSpouse));
        OnPropertyChanged(nameof(Spouses));
      }
    }

    /// <summary>
    /// Calculated property that returns whether the person has an avatar photo.
    /// </summary>
    [XmlIgnore]
    public bool HasAvatar
    {
      get
      {
        if (photos != null && photos.Count > 0)
        {
          foreach (Photo photo in photos)
          {
            if (photo.IsAvatar)
            {
              return true;
            }
          }
        }

        return false;
      }
    }

    /// <summary>
    /// Calculated property that returns string that describes this person to their parents.
    /// </summary>
    [XmlIgnore]
    public string ParentRelationshipText
    {
      get
      {
        if (gender == Gender.Male)
        {
          return "Son";
        }
        else
        {
          return "Daughter";
        }
      }
    }

    /// <summary>
    /// Calculated property that returns string text for this person's parents
    /// </summary>
    [XmlIgnore]
    public string ParentsText
    {
      get
      {
        Collection<Person> parents = Parents;

        string parentsText = string.Empty;
        if (parents.Count > 0)
        {
          parentsText = parents[0].Name;

          if (parents.Count == 2)
          {
            parentsText += " and " + parents[1].Name;
          }
          else
          {
            for (int i = 1; i < parents.Count; i++)
            {
              if (i == parents.Count - 1)
              {
                parentsText += ", and " + parents[i].Name;
              }
              else
              {
                parentsText += ", " + parents[i].Name;
              }
            }
          }
        }

        return parentsText;
      }
    }

    /// <summary>
    /// Calculated property that returns string that describes this person to their siblings.
    /// </summary>
    [XmlIgnore]
    public string SiblingRelationshipText
    {
      get
      {
        if (gender == Gender.Male)
        {
          return "Brother";
        }
        else
        {
          return "Sister";
        }
      }
    }

    /// <summary>
    /// Calculated property that returns string text for this person's siblings
    /// </summary>
    [XmlIgnore]
    public string SiblingsText
    {
      get
      {
        Collection<Person> siblings = Siblings;

        string siblingsText = string.Empty;
        if (siblings.Count > 0)
        {
          siblingsText = siblings[0].Name;

          if (siblings.Count == 2)
          {
            siblingsText += " and " + siblings[1].Name;
          }
          else
          {
            for (int i = 1; i < siblings.Count; i++)
            {
              if (i == siblings.Count - 1)
              {
                siblingsText += ", and " + siblings[i].Name;
              }
              else
              {
                siblingsText += ", " + siblings[i].Name;
              }
            }
          }
        }

        return siblingsText;
      }
    }

    /// <summary>
    /// Calculated property that returns string that describes this person to their spouses.
    /// </summary>
    [XmlIgnore]
    public string SpouseRelationshipText
    {
      get
      {
        if (gender == Gender.Male)
        {
          return "Husband";
        }
        else
        {
          return "Wife";
        }
      }
    }

    /// <summary>
    /// Calculated property that returns string text for this person's spouses.
    /// </summary>
    [XmlIgnore]
    public string SpousesText
    {
      get
      {
        Collection<Person> spouses = Spouses;

        string spousesText = string.Empty;
        if (spouses.Count > 0)
        {
          spousesText = spouses[0].Name;

          if (spouses.Count == 2)
          {
            spousesText += " and " + spouses[1].Name;
          }
          else
          {
            for (int i = 1; i < spouses.Count; i++)
            {
              if (i == spouses.Count - 1)
              {
                spousesText += ", and " + spouses[i].Name;
              }
              else
              {
                spousesText += ", " + spouses[i].Name;
              }
            }
          }
        }

        return spousesText;
      }
    }

    /// <summary>
    /// Calculated property that returns string that describes this person to their children.
    /// </summary>
    [XmlIgnore]
    public string ChildRelationshipText
    {
      get
      {
        if (gender == Gender.Male)
        {
          return "Father";
        }
        else
        {
          return "Mother";
        }
      }
    }

    /// <summary>
    /// Calculated property that returns string text for this person's children.
    /// </summary>
    [XmlIgnore]
    public string ChildrenText
    {
      get
      {
        Collection<Person> children = Children;
        string childrenText = string.Empty;
        if (children.Count > 0)
        {
          childrenText = children[0].Name;

          if (children.Count == 2)
          {
            childrenText += " and " + children[1].Name;
          }
          else
          {
            for (int i = 1; i < children.Count; i++)
            {
              if (i == children.Count - 1)
              {
                childrenText += ", and " + children[i].Name;
              }
              else
              {
                childrenText += ", " + children[i].Name;
              }
            }
          }
        }

        return childrenText;
      }
    }
    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new instance of a person object.
    /// Each new instance will be given a unique identifier.
    /// This parameterless constructor is also required for serialization.
    /// </summary>
    public Person()
    {
      id = Guid.NewGuid().ToString();
      relationships = new RelationshipCollection();
      photos = new PhotoCollection();
      firstName = Const.DefaultFirstName;
      isLiving = true;
    }

    /// <summary>
    /// Creates a new instance of the person class with the firstname and the lastname.
    /// </summary>
    public Person(string firstName, string lastName) : this()
    {
      // Use the first name if specified, if not, the default first name is used.
      if (!string.IsNullOrEmpty(firstName))
      {
        this.firstName = firstName;
      }

      this.lastName = lastName;
    }

    /// <summary>
    /// Creates a new instance of the person class with the firstname, the lastname, and gender
    /// </summary>
    public Person(string firstName, string lastName, Gender gender) : this(firstName, lastName)
    {
      this.gender = gender;
    }

    #endregion

    #region INotifyPropertyChanged Members

    /// <summary>
    /// INotifyPropertyChanged requires a property called PropertyChanged.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Fires the event for the property when it changes.
    /// </summary>
    protected virtual void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    #endregion

    #region IEquatable Members

    /// <summary>
    /// Determine equality between two person classes
    /// </summary>
    public bool Equals(Person other)
    {
      return (Id == other.Id);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get the spouse relationship for the specified spouse.
    /// </summary>
    public SpouseRelationship GetSpouseRelationship(Person spouse)
    {
      foreach (Relationship relationship in relationships)
      {
        SpouseRelationship spouseRelationship = relationship as SpouseRelationship;
        if (spouseRelationship != null)
        {
          if (spouseRelationship.RelationTo.Equals(spouse))
          {
            return spouseRelationship;
          }
        }
      }

      return null;
    }


    /// <summary>
    /// Gets the combination of parent sets for this person and his/her spouses
    /// </summary>
    /// <returns></returns>
    public ParentSetCollection MakeParentSets()
    {
      ParentSetCollection parentSets = new ParentSetCollection();

      foreach (Person spouse in Spouses)
      {
        ParentSet ps = new ParentSet(this, spouse);

        // Don't add the same parent set twice.
        if (!parentSets.Contains(ps))
        {
          parentSets.Add(ps);
        }
      }

      return parentSets;
    }

    /// <summary>
    /// Called to delete the person's photos
    /// </summary>
    public void DeletePhotos()
    {
      // Delete the person's photos
      foreach (Photo photo in photos)
      {
        photo.Delete();
      }
    }

    /// <summary>
    /// Called to delete the person's story
    /// </summary>
    public void DeleteStory()
    {
      if (story != null)
      {
        story.Delete();
        story = null;
      }
    }

    public override string ToString()
    {
      return Name;
    }

    #endregion

    #region IDataErrorInfo Members

    public string Error
    {
      get { return null; }
    }

    public string this[string columnName]
    {
      get
      {
        string result = null;

        if (columnName == "BirthDate")
        {
          if (BirthDate == DateTime.MinValue)
          {
            result = "This does not appear to be a valid date.";
          }
        }

        if (columnName == "DeathDate")
        {
          if (DeathDate == DateTime.MinValue)
          {
            result = "This does not appear to be a valid date.";
          }
        }

        return result;
      }
    }

    #endregion
  }
}