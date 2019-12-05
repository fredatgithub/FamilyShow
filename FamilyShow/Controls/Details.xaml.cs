using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow
{
  /// <summary>
  /// Interaction logic for Details.xaml
  /// </summary>

  public partial class Details : UserControl
  {
    #region fields

    PeopleCollection family = App.Family;

    // Setting the ItemsSource selects the first item which raises the SelectionChanged event.
    // This flag prevents the initialization code from making the selection.
    bool ignoreSelection = true;

    #endregion

    public Details()
    {
      InitializeComponent();

      // These sections are collapsed within code instead of in the xaml 
      // so that they show up as visible in Blend.
      DetailsAdd.Visibility = Visibility.Collapsed;
      DetailsAddIntermediate.Visibility = Visibility.Collapsed;
      DetailsEdit.Visibility = Visibility.Collapsed;
      DetailsEditRelationship.Visibility = Visibility.Collapsed;
      AddExisting.Visibility = Visibility.Collapsed;

      // Bind the Family ListView and turn off the allow the selection 
      // change event to change the selected item.
      FamilyListView.ItemsSource = family;
      ignoreSelection = false;

      // Set the default sort order for the Family ListView to 
      // the person's first name.
      ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(family);
      view.SortDescriptions.Add(new SortDescription("FirstName", ListSortDirection.Ascending));

      // Handle event when the selected person changes so can select 
      // the item in the list.
      family.CurrentChanged += new EventHandler(Family_CurrentChanged);

      ExistingPeopleListBox.ItemsSource = family;
    }

    #region routed events

    public static readonly RoutedEvent PersonInfoClickEvent = EventManager.RegisterRoutedEvent(
        "PersonInfoClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Details));

    /// <summary>
    /// Expose the PersonInfoClick event
    /// </summary>
    public event RoutedEventHandler PersonInfoClick
    {
      add { AddHandler(PersonInfoClickEvent, value); }
      remove { RemoveHandler(PersonInfoClickEvent, value); }
    }

    public static readonly RoutedEvent EveryoneDeletedEvent = EventManager.RegisterRoutedEvent(
        "EveryoneDeleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Details));

    /// <summary>
    /// Expose the EveryoneDeleted event
    /// </summary>
    public event RoutedEventHandler EveryoneDeleted
    {
      add { AddHandler(EveryoneDeletedEvent, value); }
      remove { RemoveHandler(EveryoneDeletedEvent, value); }
    }

    public static readonly RoutedEvent FamilyDataClickEvent = EventManager.RegisterRoutedEvent(
        "FamilyDataClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Details));

    /// <summary>
    /// Expose the FamilyDataClick event
    /// </summary>
    public event RoutedEventHandler FamilyDataClick
    {
      add { AddHandler(FamilyDataClickEvent, value); }
      remove { RemoveHandler(FamilyDataClickEvent, value); }
    }
    #endregion

    #region event handlers

    #region Details Add event handlers

    /// <summary>
    /// Handles the Family Member Add Button click event
    /// </summary>
    private void FamilyMemberAddButton_Click(object sender, RoutedEventArgs e)
    {
      if (!(FamilyMemberAddButton.CommandParameter == null))
      {
        FamilyMemberComboBox.SelectedItem =
            (FamilyMemberComboBoxValue)(FamilyMemberAddButton.CommandParameter);
      }
    }

    /// <summary>
    /// Shows the Details Add section with the selected family member relationship choice.
    /// </summary>
    private void FamilyMemberComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (FamilyMemberComboBox.SelectedIndex != -1)
      {
        ClearDetailsAddFields();

        string lastname = string.Empty;
        bool isExisting = false;

        switch ((FamilyMemberComboBoxValue)FamilyMemberComboBox.SelectedValue)
        {
          case FamilyMemberComboBoxValue.Father:
          case FamilyMemberComboBoxValue.Sister:
          case FamilyMemberComboBoxValue.Brother:
            // Assume that the new person has the same last name.
            lastname = family.Current.LastName;
            break;
          case FamilyMemberComboBoxValue.Daughter:
          case FamilyMemberComboBoxValue.Son:
            // Assume that the new person has the same last name as the husband
            if ((family.Current.Gender == Gender.Female) && (family.Current.Spouses.Count > 0) && (family.Current.Spouses[0].Gender == Gender.Male))
              lastname = family.Current.Spouses[0].LastName;
            else
              lastname = family.Current.LastName;
            break;
          case FamilyMemberComboBoxValue.Existing:
            isExisting = true;
            break;
          case FamilyMemberComboBoxValue.Spouse:
          case FamilyMemberComboBoxValue.Mother:
          default:
            break;
        }

        if (isExisting)
          // Use animation to expand the Add Existing section
          ((Storyboard)Resources["ExpandAddExisting"]).Begin(this);
        else
          // Use animation to expand the Details Add section
          ((Storyboard)Resources["ExpandDetailsAdd"]).Begin(this);

        LastNameInputTextBox.Text = lastname;

        FirstNameInputTextBox.Focus();
      }
    }

    /// <summary>
    /// Handles adding new people
    /// </summary>
    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
      // To make it a little more user friendly, set the next action for the family member button to be the same as the current relationship being added.
      SetNextFamilyMemberAction((FamilyMemberComboBoxValue)FamilyMemberComboBox.SelectedValue);

      // The new person to be added
      Person newPerson = new Person(FirstNameInputTextBox.Text, LastNameInputTextBox.Text);
      newPerson.IsLiving = (IsLivingInputCheckbox.IsChecked == null) ? true : (bool)IsLivingInputCheckbox.IsChecked;

      DateTime birthdate = App.StringToDate(BirthDateInputTextBox.Text);
      if (birthdate != DateTime.MinValue)
        newPerson.BirthDate = birthdate;

      newPerson.BirthPlace = BirthPlaceInputTextBox.Text;

      bool SelectParent = false;
      ParentSetCollection possibleParents = family.Current.PossibleParentSets;

      // Perform the action based on the selected relationship
      switch ((FamilyMemberComboBoxValue)FamilyMemberComboBox.SelectedValue)
      {
        case FamilyMemberComboBoxValue.Father:
          newPerson.Gender = Gender.Male;
          RelationshipHelper.AddParent(family, family.Current, newPerson);
          SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Mother);
          break;

        case FamilyMemberComboBoxValue.Mother:
          newPerson.Gender = Gender.Female;
          RelationshipHelper.AddParent(family, family.Current, newPerson);
          SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Brother);
          break;

        case FamilyMemberComboBoxValue.Brother:
          newPerson.Gender = Gender.Male;

          // Check to see if there are multiple parents
          if (possibleParents.Count > 1)
            SelectParent = true;
          else
            RelationshipHelper.AddSibling(family, family.Current, newPerson);
          break;

        case FamilyMemberComboBoxValue.Sister:
          newPerson.Gender = Gender.Female;

          // Check to see if there are multiple parents
          if (possibleParents.Count > 1)
            SelectParent = true;
          else
            RelationshipHelper.AddSibling(family, family.Current, newPerson);
          break;

        case FamilyMemberComboBoxValue.Spouse:
          RelationshipHelper.AddSpouse(family, family.Current, newPerson, SpouseModifier.Current);
          SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Son);
          break;

        case FamilyMemberComboBoxValue.Son:
          newPerson.Gender = Gender.Male;

          if (family.Current.Spouses.Count > 1)
          {
            possibleParents = family.Current.MakeParentSets();
            SelectParent = true;
          }
          else
            RelationshipHelper.AddChild(family, family.Current, newPerson);
          break;

        case FamilyMemberComboBoxValue.Daughter:
          newPerson.Gender = Gender.Female;
          if (family.Current.Spouses.Count > 1)
          {
            possibleParents = family.Current.MakeParentSets();
            SelectParent = true;
          }
          else
            RelationshipHelper.AddChild(family, family.Current, newPerson);
          break;
      }

      if (SelectParent)
        ShowDetailsAddIntermediate(possibleParents);
      else
      {
        // Use animation to hide the Details Add section
        ((Storyboard)Resources["CollapseDetailsAdd"]).Begin(this);

        FamilyMemberComboBox.SelectedIndex = -1;
        FamilyMemberAddButton.Focus();
      }

      family.OnContentChanged(newPerson);
    }

    /// <summary>
    /// Handles adding new people and choosing the parents within the Intermediate Add section.
    /// </summary>
    private void IntermediateAddButton_Click(object sender, RoutedEventArgs e)
    {
      Person newPerson = new Person(FirstNameInputTextBox.Text, LastNameInputTextBox.Text);
      newPerson.IsLiving = (IsLivingInputCheckbox.IsChecked == null) ? true : (bool)IsLivingInputCheckbox.IsChecked;

      DateTime birthdate = App.StringToDate(BirthDateInputTextBox.Text);
      if (birthdate != DateTime.MinValue)
        newPerson.BirthDate = birthdate;

      newPerson.BirthPlace = BirthPlaceInputTextBox.Text;

      switch ((FamilyMemberComboBoxValue)FamilyMemberComboBox.SelectedValue)
      {
        case FamilyMemberComboBoxValue.Brother:
          newPerson.Gender = Gender.Male;
          RelationshipHelper.AddParent(family, newPerson, (ParentSet)ParentsListBox.SelectedValue);
          break;

        case FamilyMemberComboBoxValue.Sister:
          newPerson.Gender = Gender.Female;
          RelationshipHelper.AddParent(family, newPerson, (ParentSet)ParentsListBox.SelectedValue);
          break;
        case FamilyMemberComboBoxValue.Son:
          newPerson.Gender = Gender.Male;
          RelationshipHelper.AddParent(family, newPerson, (ParentSet)ParentsListBox.SelectedValue);
          break;

        case FamilyMemberComboBoxValue.Daughter:
          newPerson.Gender = Gender.Female;
          RelationshipHelper.AddParent(family, newPerson, (ParentSet)ParentsListBox.SelectedValue);
          break;
      }

      FamilyMemberComboBox.SelectedIndex = -1;
      FamilyMemberAddButton.Focus();

      // Use animation to hide the Details Add Intermediate section
      ((Storyboard)Resources["CollapseDetailsAddIntermediate"]).Begin(this);

      family.OnContentChanged(newPerson);
    }


    private void AddExistingButton_Click(object sender, RoutedEventArgs e)
    {
      Person existingPerson = (Person)ExistingPeopleListBox.SelectedItem;

      bool SelectParent = false;
      ParentSetCollection possibleParents = family.Current.PossibleParentSets;

      // Perform the action based on the selected relationship
      switch ((FamilyMemberComboBoxValue)ExistingFamilyMemberComboBox.SelectedValue)
      {
        case FamilyMemberComboBoxValue.Father:
          existingPerson.Gender = Gender.Male;
          RelationshipHelper.AddParent(family, family.Current, existingPerson);
          SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Mother);
          break;

        case FamilyMemberComboBoxValue.Mother:
          existingPerson.Gender = Gender.Female;
          RelationshipHelper.AddParent(family, family.Current, existingPerson);
          SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Brother);
          break;

        case FamilyMemberComboBoxValue.Brother:
          existingPerson.Gender = Gender.Male;

          // Check to see if there are multiple parents
          if (possibleParents.Count > 1)
            SelectParent = true;
          else
            RelationshipHelper.AddSibling(family, family.Current, existingPerson);
          break;

        case FamilyMemberComboBoxValue.Sister:
          existingPerson.Gender = Gender.Female;

          // Check to see if there are multiple parents
          if (possibleParents.Count > 1)
            SelectParent = true;
          else
            RelationshipHelper.AddSibling(family, family.Current, existingPerson);
          break;

        case FamilyMemberComboBoxValue.Spouse:
          RelationshipHelper.AddSpouse(family, family.Current, existingPerson, SpouseModifier.Current);
          SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Son);
          break;

        case FamilyMemberComboBoxValue.Son:
          existingPerson.Gender = Gender.Male;

          if (family.Current.Spouses.Count > 1)
          {
            possibleParents = family.Current.MakeParentSets();
            SelectParent = true;
          }
          else
            RelationshipHelper.AddChild(family, family.Current, existingPerson);
          break;

        case FamilyMemberComboBoxValue.Daughter:
          existingPerson.Gender = Gender.Female;
          if (family.Current.Spouses.Count > 1)
          {
            possibleParents = family.Current.MakeParentSets();
            SelectParent = true;
          }
          else
            RelationshipHelper.AddChild(family, family.Current, existingPerson);
          break;
      }

      if (SelectParent)
        ShowDetailsAddIntermediate(possibleParents);
      else
      {
        // Use animation to hide the Details Add section
        ((Storyboard)Resources["CollapseAddExisting"]).Begin(this);

        FamilyMemberComboBox.SelectedIndex = -1;
        FamilyMemberAddButton.Focus();
      }
    }

    /// <summary>
    /// Closes the Details Add section
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      FamilyMemberComboBox.SelectedIndex = -1;
    }

    private void IntermediateCloseButton_Click(object sender, RoutedEventArgs e)
    {
      FamilyMemberComboBox.SelectedIndex = -1;
    }

    private void ParentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      // Make it easy to add new people to the tree by pressing return
      IntermediateAddButton.Focus();
    }

    #endregion

    #region Details Edit event handlers

    private void DoneEditButton_Click(object sender, RoutedEventArgs e)
    {
      // Make sure the data binding is updated for fields that update during LostFocus.
      // This is necessary since this method can be invoked when the Enter key is pressed,
      // but the text field has not lost focus yet, so it does not update binding. This
      // manually updates the binding for those fields.
      if (BirthDateEditTextBox.IsFocused)
      {
        BirthDateEditTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
      }
      if (DeathDateEditTextBox.IsFocused)
      {
        DeathDateEditTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
      }

      // Let the collection know that it has been updated so that the diagram control will update.
      family.OnContentChanged();
    }

    private void DoneEditRelationshipButton_Click(object sender, RoutedEventArgs e)
    {
    }

    /// <summary>
    /// Handles Drop Event for setting the Avatar photo.
    /// </summary>
    private void AvatarPhoto_Drop(object sender, DragEventArgs e)
    {
      string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

      if (fileNames.Length > 0)
      {
        Photo photo = new Photo(fileNames[0]);

        // Set IsAvatar to false for the existing photos
        foreach (Photo existingPhoto in family.Current.Photos)
        {
          existingPhoto.IsAvatar = false;
        }

        // Make the dropped photo the  avatar photo
        photo.IsAvatar = true;

        // Add the avatar photo to the person photos
        family.Current.Photos.Add(photo);

        // Bitmap image for the avatar
        BitmapImage bitmap = new BitmapImage(new Uri(photo.FullyQualifiedPath));

        // Use BitmapCacheOption.OnLoad to prevent binding the source holding on to the photo file.
        bitmap.CacheOption = BitmapCacheOption.OnLoad;

        // Show the avatar
        AvatarPhoto.Source = bitmap;
      }

      // Mark the event as handled, so the control's native Drop handler is not called.
      e.Handled = true;
    }

    private void SpouseStatusListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (SpouseStatusListbox.SelectedItem != null)
      {
        RelationshipHelper.UpdateSpouseStatus(family.Current, (Person)SpousesCombobox.SelectedItem, (SpouseModifier)SpouseStatusListbox.SelectedItem);

        // The ToEditTextBox is only edittable for current spouses.
        ToEditTextBox.IsEnabled = ((SpouseModifier)SpouseStatusListbox.SelectedItem == SpouseModifier.Former);
      }
      
      family.OnContentChanged();
    }

    private void SpousesCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (family.Current == null)
      {
        return;
      }

      foreach (Relationship rel in family.Current.Relationships)
      {
        if (rel.RelationshipType == RelationshipType.Spouse && rel.RelationTo.Equals((Person)SpousesCombobox.SelectedItem))
        {
          SpouseRelationship spouseRel = ((SpouseRelationship)rel);
          SpouseStatusListbox.SelectedItem = spouseRel.SpouseModifier;

          FromEditTextBox.Text = string.Empty;
          ToEditTextBox.Text = string.Empty;

          if (spouseRel.MarriageDate.HasValue)
          {
            FromEditTextBox.Text = ((DateTime)spouseRel.MarriageDate).ToShortDateString();
          }
          if (spouseRel.DivorceDate.HasValue)
          {
            ToEditTextBox.Text = ((DateTime)spouseRel.DivorceDate).ToShortDateString();
          }
        }
      }
    }

    private void ParentsEditListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      // Update the parents
      if (ParentsEditListBox.SelectedItem != null)
      {
        RelationshipHelper.ChangeParents(family, family.Current, (ParentSet)ParentsEditListBox.SelectedValue);

        // Let the collection know that it has been updated
        family.OnContentChanged();
      }
    }

    private void FromEditTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
      // Update the spouse Marriage date info
      if (SpousesCombobox.SelectedItem != null)
      {
        #region Perform the businless logic for updating the marriage date

        DateTime marriageDate = App.StringToDate(FromEditTextBox.Text);

        if (marriageDate == DateTime.MinValue)
        {
          // Clear the marriage date
          RelationshipHelper.UpdateMarriageDate(family.Current, (Person)SpousesCombobox.SelectedItem, null);
        }
        else
        {
          RelationshipHelper.UpdateMarriageDate(family.Current, (Person)SpousesCombobox.SelectedItem, marriageDate);
        }

        // Let the collection know that it has been updated
        family.OnContentChanged();

        #endregion
      }
    }

    private void ToEditTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
      // Update the spouse Divorce date info
      if (SpousesCombobox.SelectedItem != null)
      {
        #region Perform the businless logic for updating the divorce date

        DateTime divorceDate = App.StringToDate(ToEditTextBox.Text);

        if (divorceDate == DateTime.MinValue)
        {
          // Clear the divorce date
          RelationshipHelper.UpdateDivorceDate(family.Current, (Person)SpousesCombobox.SelectedItem, null);
        }
        else
        {
          RelationshipHelper.UpdateDivorceDate(family.Current, (Person)SpousesCombobox.SelectedItem, divorceDate);
        }

        // Let the collection know that it has been updated
        family.OnContentChanged();

        #endregion
      }
    }

    #endregion

    private void InfoButton_Click(object sender, RoutedEventArgs e)
    {
      RaiseEvent(new RoutedEventArgs(PersonInfoClickEvent));
    }

    private void FamiliyDataButton_Click(object sender, RoutedEventArgs e)
    {
      RaiseEvent(new RoutedEventArgs(FamilyDataClickEvent));
    }

    /// <summary>
    /// Changes the primary person selection for diagram and details panel.
    /// </summary>
    private void FamilyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!ignoreSelection)
      {
        ignoreSelection = true;
        Person selected = (Person)((ListBox)sender).SelectedItem;
        if (selected != null)
        {
          family.Current = selected;
          DataContext = family.Current;
        }

        ignoreSelection = false;
      }
    }

    private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      FamilyListView.FilterList(FilterTextBox.Text);
    }

    /// <summary>
    /// Event handler when the selected node changes. Select the 
    /// current person in the list.
    /// </summary>
    void Family_CurrentChanged(object sender, EventArgs e)
    {
      if (!ignoreSelection)
      {
        ignoreSelection = true;
        FamilyListView.SelectedItem = family.Current;
        FamilyListView.ScrollIntoView(family.Current);
        ignoreSelection = false;
      }
    }

    /// <summary>
    /// Event handler for textbox GotFocus.  Select all of the textbox contents for quick entry.
    /// </summary>
    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
      ((TextBox)sender).SelectAll();
    }

    /// <summary>
    /// Event handler for deleting people.  Note that not all people can be deleted.  See "IsDeletable" property on the person class.
    /// </summary>

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
      // Deleting a person requires deleting that person from their relations with other people
      // Call the relationship helper to handle delete.
      RelationshipHelper.DeletePerson(family, family.Current);

      if (family.Count > 0)
      {
        // Current person is deleted, choose someone else as the current person
        family.Current = family[0];

        family.OnContentChanged();
        SetDefaultFocus();
      }
      else
      {
        // Let the container window know that everyone has been deleted
        RaiseEvent(new RoutedEventArgs(EveryoneDeletedEvent));
      }
    }

    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (DataContext != null)
      {
        if (family != null && family.Current != null && family.Current.HasParents)
        {
          // Programmatically set the possible parents that can be selected.
          ParentSetCollection psc = family.Current.PossibleParentSets;
          ParentsEditListBox.ItemsSource = psc;

          // Cannot simply set the selected parent like so: ParentsEditListBox.SelectedItem = family.Current.ParentSet;
          // Need to use .Equals and set the index
          for (int i = 0; i <= psc.Count - 1; i++)
          {
            if (psc[i].Equals(family.Current.ParentSet))
            {
              ParentsEditListBox.SelectedIndex = i;
            }
          }
        }

        SetFamilyMemberAddButton();
      }
    }

    #region Storyboard completed events

    /// <summary>
    /// The focus can be set only after the animation has stopped playing.
    /// </summary>
    private void ExpandDetailsAdd_StoryboardCompleted(object sender, EventArgs e)
    {
      FirstNameInputTextBox.Focus();
    }

    /// <summary>
    /// Make it easy to add new people to the tree by pressing return
    /// </summary>
    private void CollapseDetailsAdd_StoryboardCompleted(object sender, EventArgs e)
    {
      FamilyMemberAddButton.Focus();
    }

    /// <summary>
    /// The focus can be set only after the animation has stopped playing.
    /// </summary>
    private void ExpandDetailsEdit_StoryboardCompleted(object sender, EventArgs e)
    {
      FirstNameEditTextBox.Focus();
    }

    /// <summary>
    /// Make it easy to add new people to the tree by pressing return
    /// </summary>
    private void CollapseDetailsEdit_StoryboardCompleted(object sender, EventArgs e)
    {
      FamilyMemberAddButton.Focus();
    }

    /// <summary>
    /// The focus can be set only after the animation has stopped playing.
    /// </summary>
    private void ExpandDetailsEditRelationship_StoryboardCompleted(object sender, EventArgs e)
    {
      DoneEditRelationshipButton.Focus();
    }

    /// <summary>
    /// Make it easy to add new people to the tree by pressing return
    /// </summary>
    private void CollapseDetailsEditRelationship_StoryboardCompleted(object sender, EventArgs e)
    {
      FamilyMemberAddButton.Focus();
    }

    /// <summary>
    /// The focus can be set only after the animation has stopped playing.
    /// </summary>
    private void ExpandDetailsAddIntermediate_StoryboardCompleted(object sender, EventArgs e)
    {
      // Select the first parent set for quick entry.
      ParentsListBox.SelectedIndex = 0;
      IntermediateAddButton.Focus();
    }

    /// <summary>
    /// Make it easy to add new people to the tree by pressing return
    /// </summary>
    private void CollapseDetailsAddIntermediate_StoryboardCompleted(object sender, EventArgs e)
    {
      FamilyMemberAddButton.Focus();
    }

    /// <summary>
    /// The focus can be set only after the animation has stopped playing.
    /// </summary>
    private void ExpandAddExisting_StoryboardCompleted(object sender, EventArgs e)
    {
      AddExistingButton.Focus();
      ExistingFamilyMemberComboBox.SelectedItem = FamilyMemberComboBoxValue.Spouse;
    }

    /// <summary>
    /// Make it easy to add new people to the tree by pressing return
    /// </summary>
    private void CollapseAddExisting_StoryboardCompleted(object sender, EventArgs e)
    {
      FamilyMemberAddButton.Focus();
    }

    #endregion

    #endregion

    #region helper methods

    /// <summary>
    /// Clear the input fields
    /// </summary>
    private void ClearDetailsAddFields()
    {
      FirstNameInputTextBox.Clear();
      LastNameInputTextBox.Clear();
      BirthDateInputTextBox.Clear();
      BirthPlaceInputTextBox.Clear();
    }

    public void SetDefaultFocus()
    {
      FamilyMemberAddButton.Focus();
    }

    /// <summary>
    /// Display the Details Add Intermediate section
    /// </summary>
    private void ShowDetailsAddIntermediate(ParentSetCollection possibleParents)
    {
      // Display the Details Add Intermediate section
      ((Storyboard)Resources["ExpandDetailsAddIntermediate"]).Begin(this);

      // Bind the possible parents
      ParentsListBox.ItemsSource = possibleParents;
    }

    /// <summary>
    /// Sets the next action for the Add Family Member Button
    /// </summary>
    private void SetNextFamilyMemberAction(FamilyMemberComboBoxValue value)
    {
      FamilyMemberAddButton.CommandParameter = value;
      FamilyMemberAddButton.Content = "_Add " + value.ToString();
    }

    /// <summary>
    /// Logic for setting the default command for the family member to add
    /// </summary>
    private void SetFamilyMemberAddButton()
    {
      if (family.Current.Parents.Count == 2)
      {
        // Person has parents, choice another default.
        SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Brother);
      }
      else
      {
        // Default for everything else
        SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Father);
      }
    }

    #endregion

    private void FilterTextBox1_TextChanged(object sender, TextChangedEventArgs e)
    {
      //Use collection view to filter the listbox
      ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(family);
      view.Filter = new Predicate<object>(NameFilter);
    }

    public bool NameFilter(object item)
    {
      Person person = item as Person;
      return (person.FullName.ToLower().Contains(FilterTextBox1.Text.ToLower()));
    }
  }

  /// <summary>
  /// Enum Values for the Family Member ComboBox.
  /// </summary>
  public enum FamilyMemberComboBoxValue
  {
    Father,
    Mother,
    Brother,
    Sister,
    Spouse,
    Son,
    Daughter,
    Existing
  }
}