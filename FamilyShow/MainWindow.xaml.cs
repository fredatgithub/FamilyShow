using FamilyShowLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Xml;

namespace FamilyShow
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    #region fields

    // The list of people, this is a global list shared by the application.
    People familyCollection = App.FamilyCollection;
    PeopleCollection family = App.Family;
    //static string exportPath = @"D:\productions\Généalogie\FICHIER_ENQUETES\";
    // should be my documents by default
    static string exportPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private Properties.Settings appSettings = Properties.Settings.Default;

    #endregion

    #region menu routed commands

    public static readonly RoutedCommand ImportGedcomCommand = new RoutedCommand("ImportGedcom", typeof(MainWindow));
    public static readonly RoutedCommand ExportGedcomCommand = new RoutedCommand("ExportGedcom", typeof(MainWindow));
    public static readonly RoutedCommand WhatIsGedcomCommand = new RoutedCommand("WhatIsGedcom", typeof(MainWindow));
    public static readonly RoutedCommand ExportXpsCommand = new RoutedCommand("ExportXps", typeof(MainWindow));
    public static readonly RoutedCommand ChangeSkinCommand = new RoutedCommand("ChangeSkin", typeof(MainWindow));
    public static readonly RoutedCommand ExportBirthCommand = new RoutedCommand("ExportBirth", typeof(MainWindow));
    public bool changesNeedToBeSave = false;

    #endregion

    public MainWindow()
    {
      InitializeComponent();

      family.CurrentChanged += new EventHandler(People_CurrentChanged);

      // Build the Open Menu, recent opened files are part of the open menu
      BuildOpenMenu();

      BuildSkinsMenu();

      // The welcome screen is the initial view
      ShowWelcomeScreen();
    }

    #region event handlers

    /// <summary>
    /// Event handler when the primary person has changed.
    /// </summary>
    private void People_CurrentChanged(object sender, EventArgs e)
    {
      if (family.Current != null)
      {
        DetailsControl.DataContext = family.Current;
      }
    }

    private void NewUserControl_AddButtonClick(object sender, RoutedEventArgs e)
    {
      HideNewUserControl();
      ShowDetailsPane();
    }

    private void NewUserControl_CloseButtonClick(object sender, RoutedEventArgs e)
    {
      HideNewUserControl();
      ShowDetailsPane();
    }

    private void DetailsControl_PersonInfoClick(object sender, RoutedEventArgs e)
    {
      // Uses an animation to show the Person Info Control
      ((Storyboard)Resources["ShowPersonInfo"]).Begin(this);

      PersonInfoControl.DataContext = family.Current;
    }

    private void DetailsControl_FamilyDataClick(object sender, RoutedEventArgs e)
    {
      FamilyDataControl.Refresh();

      ((Storyboard)Resources["ShowFamilyData"]).Begin(this);
    }

    /// <summary>
    /// Event handler when all people in the people collection have been deleted
    /// </summary>
    private void DetailsControl_EveryoneDeleted(object sender, RoutedEventArgs e)
    {
      // Everyone was deleted show the create new user control
      NewFamily(sender, e);
    }

    private void PersonInfoControl_CloseButtonClick(object sender, RoutedEventArgs e)
    {
      // Uses an animation to hide the Person Info Control
      ((Storyboard)Resources["HidePersonInfo"]).Begin(this);
    }

    private void FamilyDataControl_CloseButtonClick(object sender, RoutedEventArgs e)
    {
      HideFamilyDataControl();
    }

    /// <summary>
    /// The focus can be set only after the animation has stopped playing.
    /// </summary>
    private void ShowPersonInfo_StoryboardCompleted(object sender, EventArgs e)
    {
      PersonInfoControl.SetDefaultFocus();
    }

    /// <summary>
    /// The focus can be set only after the animation has stopped playing.
    /// </summary>
    private void HidePersonInfo_StoryboardCompleted(object sender, EventArgs e)
    {
      DetailsControl.SetDefaultFocus();
    }

    /// <summary>
    /// The focus can be set only after the animation has stopped playing.
    /// </summary>
    private void ShowFamilyData_StoryboardCompleted(object sender, EventArgs e)
    {
      FamilyDataControl.SetDefaultFocus();
    }

    /// <summary>
    /// The focus can be set only after the animation has stopped playing.
    /// </summary>
    private void HideFamilyData_StoryboardCompleted(object sender, EventArgs e)
    {
      DetailsControl.SetDefaultFocus();
    }

    private void Vertigo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      // Open the Vertigo website in the user's default browser
      System.Diagnostics.Process.Start("http://www.vertigo.com/familyshow");
    }

    private void WelcomeUserControl_NewButtonClick(object sender, RoutedEventArgs e)
    {
      NewFamily(sender, e);
    }

    private void WelcomeUserControl_OpenButtonClick(object sender, RoutedEventArgs e)
    {
      OpenFamily(sender, e);
    }

    private void WelcomeUserControl_ImportButtonClick(object sender, RoutedEventArgs e)
    {
      ImportGedcom(sender, e);
    }

    private void WelcomeUserControl_OpenRecentFileButtonClick(object sender, RoutedEventArgs e)
    {
      Button item = (Button)e.OriginalSource;
      string file = item.CommandParameter as string;

      if (!string.IsNullOrEmpty(file))
      {
        // Load the selected family file
        LoadFamily(file);

        ShowDetailsPane();

        // This will tell the diagram to redraw and the details panel to update.
        family.OnContentChanged();

        // Remove the file from its current position and add it back to the top/most recent position.
        App.RecentFiles.Remove(file);
        App.RecentFiles.Insert(0, file);
        BuildOpenMenu();
        family.IsDirty = false;
      }
    }

    private void OldVersionMessageControl_ContinueButtonClick(object sender, RoutedEventArgs e)
    {
      HideOldVersionMessage();

      // Prompt to save if the file has not been saved before, otherwise just save to the existing file.
      if (string.IsNullOrEmpty(familyCollection.FullyQualifiedFilename) || family.IsOldVersion)
      {
        CommonDialog dialog = new CommonDialog
        {
          InitialDirectory = People.ApplicationFolderPath
        };

        dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyV3Files, Properties.Resources.FamilyV3Extension));
        dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
        dialog.Title = Properties.Resources.SaveAs;
        dialog.DefaultExtension = Properties.Resources.DefaultFamilyExtension;
        dialog.ShowSave();

        if (!string.IsNullOrEmpty(dialog.FileName))
        {
          familyCollection.Save(dialog.FileName);
          family.IsOldVersion = false;

          // Remove the file from its current position and add it back to the top/most recent position.
          App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
          App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
          BuildOpenMenu();
        }
      }
      else
      {
        familyCollection.Save();

        // Remove the file from its current position and add it back to the top/most recent position.
        App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
        App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
        BuildOpenMenu();
      }
    }

    #endregion

    #region command handlers

    /// <summary>
    /// Command handler for New Command
    /// </summary>
    private void NewFamily(object sender, RoutedEventArgs e)
    {
      PromptToSave();

      family.Clear();
      familyCollection.FullyQualifiedFilename = null;
      family.OnContentChanged();

      ShowNewUserControl();
      family.IsDirty = false;
    }

    /// <summary>
    /// Command handler for Open Command
    /// </summary>
    private void OpenFamily(object sender, RoutedEventArgs e)
    {
      PromptToSave();

      CommonDialog dialog = new CommonDialog
      {
        InitialDirectory = People.ApplicationFolderPath
      };

      dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyFiles, Properties.Resources.FamilyExtensions));
      dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyV3Files, Properties.Resources.FamilyV3Extension));
      dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyV2Files, Properties.Resources.FamilyV2Extension));
      dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
      dialog.Title = Properties.Resources.Open;
      dialog.ShowOpen();

      if (!string.IsNullOrEmpty(dialog.FileName))
      {
        LoadFamily(dialog.FileName);

        ShowDetailsPane();

        // This will tell other views using this data to update themselves using the new data
        family.OnContentChanged();

        if (familyCollection.FullyQualifiedFilename.EndsWith(Properties.Resources.DefaultFamilyExtension))
        {
          // Remove the file from its current position and add it back to the top/most recent position.
          App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
          App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
          BuildOpenMenu();
          family.IsDirty = false;
        }
      }
    }

    /// <summary>
    /// Load the selected family file
    /// </summary>
    /// <param name="fileName"></param>
    private void LoadFamily(string fileName)
    {
      familyCollection.FullyQualifiedFilename = fileName;

      // Load the selected family file based on the file extension
      if (fileName.EndsWith(Properties.Resources.DefaultFamilyExtension))
      {
        familyCollection.LoadOPC();
      }
      else
      {
        family.IsOldVersion = true;
        familyCollection.LoadVersion2();
      }
    }

    /// <summary>
    /// Command handler for Open Recent Command
    /// </summary>
    private void OpenRecentFile_Click(object sender, RoutedEventArgs e)
    {
      MenuItem item = (MenuItem)sender;
      string file = item.CommandParameter as string;

      if (!string.IsNullOrEmpty(file))
      {
        PromptToSave();

        LoadFamily(file);

        ShowDetailsPane();

        // This will tell the diagram to redraw and the details panel to update.
        family.OnContentChanged();

        // Remove the file from its current position and add it back to the top/most recent position.
        App.RecentFiles.Remove(file);
        App.RecentFiles.Insert(0, file);
        BuildOpenMenu();
        family.IsDirty = false;
      }
    }

    /// <summary>
    /// Command handler for Save Command
    /// </summary>
    private void SaveFamily(object sender, RoutedEventArgs e)
    {
      if (family.IsOldVersion && !appSettings.DontShowOldVersionMessage)
      {
        //Show message that the file will be saved in the new format
        ShowOldVersionMessage();
      }
      else
      {
        // Prompt to save if the file has not been saved before, otherwise just save to the existing file.
        if (string.IsNullOrEmpty(familyCollection.FullyQualifiedFilename) || family.IsOldVersion)
        {
          CommonDialog dialog = new CommonDialog
          {
            InitialDirectory = People.ApplicationFolderPath
          };

          dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyV3Files, Properties.Resources.FamilyV3Extension));
          dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
          dialog.Title = Properties.Resources.SaveAs;
          dialog.DefaultExtension = Properties.Resources.DefaultFamilyExtension;
          dialog.ShowSave();

          if (!string.IsNullOrEmpty(dialog.FileName))
          {
            familyCollection.Save(dialog.FileName);
            family.IsOldVersion = false;

            // Remove the file from its current position and add it back to the top/most recent position.
            App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
            App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
            BuildOpenMenu();
          }
        }
        else
        {
          familyCollection.Save();

          // Remove the file from its current position and add it back to the top/most recent position.
          App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
          App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
          BuildOpenMenu();
        }
      }
    }

    /// <summary>
    /// Command handler for Save As Command
    /// </summary>
    private void SaveFamilyAs(object sender, RoutedEventArgs e)
    {
      if (family.IsOldVersion && !appSettings.DontShowOldVersionMessage)
      {
        //Show message that the file will be saved in the new format
        ShowOldVersionMessage();
      }
      else
      {

        CommonDialog dialog = new CommonDialog
        {
          InitialDirectory = People.ApplicationFolderPath
        };

        dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyFiles, Properties.Resources.FamilyV3Extension));
        dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
        dialog.Title = Properties.Resources.SaveAs;
        dialog.DefaultExtension = Properties.Resources.DefaultFamilyExtension;
        dialog.ShowSave();

        if (!string.IsNullOrEmpty(dialog.FileName))
        {
          familyCollection.Save(dialog.FileName);

          if (familyCollection.FullyQualifiedFilename.EndsWith(Properties.Resources.DefaultFamilyExtension))
          {
            // Remove the file from its current position and add it back to the top/most recent position.
            App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
            App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
            BuildOpenMenu();
          }
        }
      }
    }

    /// <summary>
    /// Command handler for Print Command
    /// </summary>
    private void PrintFamily(object sender, RoutedEventArgs e)
    {
      PrintDialog dlg = new PrintDialog();

      if ((bool)dlg.ShowDialog().GetValueOrDefault())
      {
        // Hide the zoom control before the diagram is saved
        DiagramControl.ZoomSliderPanel.Visibility = Visibility.Hidden;

        // Send the diagram to the printer
        dlg.PrintVisual(DiagramBorder, familyCollection.FullyQualifiedFilename);

        // Show the zoom control again
        DiagramControl.ZoomSliderPanel.Visibility = Visibility.Visible;
      }
    }

    /// <summary>
    /// Command handler for ImportGedcomCommand
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private void ImportGedcom(object sender, EventArgs e)
    {
      PromptToSave();

      CommonDialog dialog = new CommonDialog
      {
        InitialDirectory = People.ApplicationFolderPath
      };

      dialog.Filter.Add(new FilterEntry(Properties.Resources.GedcomFiles, Properties.Resources.GedcomExtension));
      dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
      dialog.Title = Properties.Resources.Import;
      dialog.ShowOpen();

      if (!string.IsNullOrEmpty(dialog.FileName))
      {
        try
        {
          GedcomImport ged = new GedcomImport();
          ged.Import(family, dialog.FileName);
          familyCollection.FullyQualifiedFilename = string.Empty;

          ShowDetailsPane();
          family.IsDirty = false;
        }
        catch
        {
          // Could not import the GEDCOM for some reason. Handle
          // all exceptions the same, display message and continue
          /// without importing the GEDCOM file.
          MessageBox.Show(this, Properties.Resources.GedcomFailedMessage,
              Properties.Resources.GedcomFailed, MessageBoxButton.OK,
              MessageBoxImage.Information);
        }
      }
    }

    /// <summary>
    /// Command handler for ExportGedcomCommand
    /// </summary>
    private void ExportGedcom(object sender, EventArgs e)
    {
      CommonDialog dialog = new CommonDialog
      {
        InitialDirectory = People.ApplicationFolderPath
      };

      dialog.Filter.Add(new FilterEntry(Properties.Resources.GedcomFiles, Properties.Resources.GedcomExtension));
      dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
      dialog.Title = Properties.Resources.Export;
      dialog.DefaultExtension = Properties.Resources.DefaultGedcomExtension;
      dialog.ShowSave();

      if (!string.IsNullOrEmpty(dialog.FileName))
      {
        GedcomExport ged = new GedcomExport();
        ged.Export(family, dialog.FileName);
      }
    }

    /// <summary>
    /// Command handler for ExportGedcomCommand
    /// </summary>
    private void WhatIsGedcom(object sender, EventArgs e)
    {
      // Open the Wikipedia entry about GEDCOM in the user's default browser
      System.Diagnostics.Process.Start("http://en.wikipedia.org/wiki/GEDCOM");
    }

    /// <summary>
    /// Command handler for ExportXPSCommand
    /// </summary>
    private void ExportXps(object sender, EventArgs e)
    {
      CommonDialog dialog = new CommonDialog
      {
        InitialDirectory = People.ApplicationFolderPath
      };

      dialog.Filter.Add(new FilterEntry(Properties.Resources.XpsFiles, Properties.Resources.XpsExtension));
      dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
      dialog.Title = Properties.Resources.Export;
      dialog.DefaultExtension = Properties.Resources.DefaultXpsExtension;
      dialog.ShowSave();

      if (!string.IsNullOrEmpty(dialog.FileName))
      {
        // Create the XPS document from the window's main container (in this case, a grid) 
        Package package = Package.Open(dialog.FileName, FileMode.Create);
        XpsDocument xpsDoc = new XpsDocument(package);
        XpsDocumentWriter xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);

        // Hide the zoom control before the diagram is saved
        DiagramControl.ZoomSliderPanel.Visibility = Visibility.Hidden;

        // Since DiagramBorder derives from FrameworkElement, the XpsDocument writer knows
        // how to output it's contents. The border is used instead of the DiagramControl
        // so that the diagram background is output as well as the digram control itself.
        xpsWriter.Write(DiagramBorder);
        xpsDoc.Close();
        package.Close();

        // Show the zoom control again
        DiagramControl.ZoomSliderPanel.Visibility = Visibility.Visible;
      }
    }

    /// <summary>
    /// Command handler for Clear Recent Files Command
    /// </summary>
    private void ClearRecentFiles_Click(object sender, RoutedEventArgs e)
    {
      App.RecentFiles.Clear();
      App.SaveRecentFiles();
      BuildOpenMenu();
    }

    /// <summary>
    /// Command handler for ChangeSkinCommand
    /// </summary>
    private void ChangeSkin(object sender, ExecutedRoutedEventArgs e)
    {
      ResourceDictionary rd = new ResourceDictionary();
      rd.MergedDictionaries.Add(Application.LoadComponent(new Uri(e.Parameter as string, UriKind.Relative)) as ResourceDictionary);
      Application.Current.Resources = rd;

      // save the skin setting
      appSettings.Skin = e.Parameter as string;
      appSettings.Save();

      family.OnContentChanged();
      PersonInfoControl.OnSkinChanged();
    }

    /// <summary>
    /// Export en xml toutes les naissances depuis 2019
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ExportBirth(object sender, ExecutedRoutedEventArgs e)
    {
      var rootTag = new ExportTagPerson("naissances", ExportTagPersonType.Root);
      var naissanceTag = new ExportTagPerson("naissance", ExportTagPersonType.Root);
      rootTag.Childs.Add(naissanceTag);
      naissanceTag.Childs.Add(new ExportTagPerson("prenom", ExportTagPersonType.FirstName));
      naissanceTag.Childs.Add(new ExportTagPerson("nom", ExportTagPersonType.LastName));
      naissanceTag.Childs.Add(new ExportTagPerson("dateNaissance", ExportTagPersonType.BirthDate));
      naissanceTag.Childs.Add(new ExportTagPerson("lieuNaissance", ExportTagPersonType.BirthPlace));
      naissanceTag.Childs.Add(new ExportTagPerson("rangDansFamille", ExportTagPersonType.GenealogicalNumber));
      naissanceTag.Childs.Add(new ExportTagPerson("numero", ExportTagPersonType.OrderIntoSiblings));
      naissanceTag.Childs.Add(new ExportTagPerson("prenomPere", ExportTagPersonType.FatherFirstName));
      naissanceTag.Childs.Add(new ExportTagPerson("prenomMere", ExportTagPersonType.MotherFirstName));
      naissanceTag.Childs.Add(new ExportTagPerson("nomMere", ExportTagPersonType.MotherLastName));

      XmlWriter xmlDoc = XmlWriter.Create(exportPath + "Naissance_2019b.xml");
      // on écrit la racine du fichier
      xmlDoc.WriteStartElement(rootTag.Name);
      WriteXmlNaissances(xmlDoc, naissanceTag, family.Current, "", 0, 2019, FilterBirth);
      xmlDoc.WriteEndElement();
      xmlDoc.Close();


      var rootTag2 = new ExportTagPerson("mariages", ExportTagPersonType.Root);
      var mariageTag = new ExportTagPerson("mariage", ExportTagPersonType.Root);
      rootTag2.Childs.Add(mariageTag);
      mariageTag.Childs.Add(new ExportTagPerson("prenom", ExportTagPersonType.FirstName));
      mariageTag.Childs.Add(new ExportTagPerson("nom", ExportTagPersonType.LastName));
      mariageTag.Childs.Add(new ExportTagPerson("numeroGenealogique", ExportTagPersonType.GenealogicalNumber));
      mariageTag.Childs.Add(new ExportTagPerson("numeroGenealogique", ExportTagPersonType.GenealogicalNumber));
      mariageTag.Childs.Add(new ExportTagPerson("prenomRapportee", ExportTagPersonType.MariagePartnerFirstName));
      mariageTag.Childs.Add(new ExportTagPerson("nomRapportee", ExportTagPersonType.MariagePartnerLastName));
      mariageTag.Childs.Add(new ExportTagPerson("genreRapportee", ExportTagPersonType.MariagePartnerGenre));
      mariageTag.Childs.Add(new ExportTagPerson("date", ExportTagPersonType.MariageDate));
      mariageTag.Childs.Add(new ExportTagPerson("lieu", ExportTagPersonType.MariagePlace));

      xmlDoc = XmlWriter.Create(exportPath + "Mariages_2014b.xml");
      // on écrit la racine du fichier
      xmlDoc.WriteStartElement(rootTag.Name);
      WriteXmlNaissances(xmlDoc, mariageTag, family.Current, "", 0, 2014, FilterMariage);
      xmlDoc.WriteEndElement();
      xmlDoc.Close();
    }

    private static void WriteXmlNaissances(XmlWriter xmlDoc, IExportTag rootTag, Person rootPers, string parentArbreLevelStr, int levelChild, int dateDepart, Func<Person, int, List<object>> filter)
    {

      string currentArbreLevelStr = parentArbreLevelStr;
      int currentArbreLevel = levelChild;
      if (levelChild > 0)
      {
        if (string.IsNullOrEmpty(currentArbreLevelStr))
        {
          currentArbreLevelStr += currentArbreLevel;
        }
        else
        {
          currentArbreLevelStr += "." + currentArbreLevel;
        }
      }

      var filterObj = filter(rootPers, dateDepart);

      foreach (var item in filterObj)
      {
        WriteTag(xmlDoc, rootTag, rootPers, item, currentArbreLevelStr, levelChild);
      }

      //int yearOfBirth;
      //if (int.TryParse(rootPers.YearOfBirth, out yearOfBirth))
      //{
      //    if (yearOfBirth >= dateDepart)
      //    {
      //        WriteTag(xmlDoc, rootTag, rootPers, currentArbreLevelStr, levelChild);
      //    }
      //}

      // on ajoute les enfants
      int numberOfChild = 1;
      foreach (Person child in rootPers.Children.OrderBy(X => X.BirthDate))
      {
        WriteXmlNaissances(xmlDoc, rootTag, child, currentArbreLevelStr, numberOfChild++, dateDepart, filter);
      }
    }

    public static List<object> FilterBirth(Person person, int startYear)
    {
      int yearOfBirth;
      var result = new List<object>();
      if (int.TryParse(person.YearOfBirth, out yearOfBirth))
      {
        if (yearOfBirth >= startYear)
        {
          result.Add(person);
        }
      }

      return result;
    }

    public static List<object> FilterMariage(Person person, int startYear)
    {
      var result = new List<object>();
      foreach (var spouseRelationship in person.ListSpousesRelationShip)
      {
        if (spouseRelationship.MarriageDate != null && spouseRelationship.MarriageDate?.Year >= startYear)
        {
          result.Add(spouseRelationship);
        }
      }

      return result;
    }

    public static void WriteTag(XmlWriter xmlDoc, IExportTag tag, Person p, object filterObj, string parentArbreLevelStr, int levelChild)
    {
      Console.WriteLine($"RACINE: {tag.Name}");
      //on écrit la racine
      xmlDoc.WriteStartElement(tag.Name);

      //s'il y a des enfants, il faut les écrire
      if (tag.Childs.Count > 0)
      {
        foreach (var subtag in tag.Childs)
        {
          WriteTag(xmlDoc, subtag, p, filterObj, parentArbreLevelStr, levelChild);
        }
      }
      // sinon, il faut écrire la valeur du tag courant !
      else
      {
        if (filterObj is Person)
        {
          xmlDoc.WriteString(tag.GetValue(p, parentArbreLevelStr, levelChild));
        }
        else if (filterObj is Relationship)
        {

        }
      }

      // dans tous les cas, on ferme!
      xmlDoc.WriteEndElement();
      Console.WriteLine($"RACINE END: {tag.Name}");
    }

    #endregion

    #region helper methods

    /// <summary>
    /// Displays the details pane
    /// </summary>
    private void ShowDetailsPane()
    {
      // Add the cloned column to layer 0:
      if (!DiagramPane.ColumnDefinitions.Contains(column1CloneForLayer0))
      {
        DiagramPane.ColumnDefinitions.Add(column1CloneForLayer0);
      }

      if (family.Current != null)
      {
        DetailsControl.DataContext = family.Current;
      }

      DetailsPane.Visibility = Visibility.Visible;
      DetailsControl.SetDefaultFocus();

      HideNewUserControl();
      HideWelcomeScreen();

      NewMenu.IsEnabled = true;
      OpenMenu.IsEnabled = true;
      SaveMenu.IsEnabled = true;
      GedcomMenu.IsEnabled = true;
      SkinsMenu.IsEnabled = true;
    }

    /// <summary>
    /// Hides the details pane
    /// </summary>
    private void HideDetailsPane()
    {
      DetailsPane.Visibility = Visibility.Collapsed;

      // Remove the cloned columns from layers 0
      if (DiagramPane.ColumnDefinitions.Contains(column1CloneForLayer0))
      {
        DiagramPane.ColumnDefinitions.Remove(column1CloneForLayer0);
      }

      NewMenu.IsEnabled = false;
      OpenMenu.IsEnabled = false;
      SaveMenu.IsEnabled = false;
      GedcomMenu.IsEnabled = false;
      SkinsMenu.IsEnabled = false;
    }

    /// <summary>
    /// Hide the family data control
    /// </summary>
    /// <returns></returns>
    private void HideFamilyDataControl()
    {
      // Uses an animation to hide the Family Data Control
      if (FamilyDataControl.IsVisible)
      {
        ((Storyboard)Resources["HideFamilyData"]).Begin(this);
      }
    }

    /// <summary>
    /// Hides the New User Control.
    /// </summary>
    private void HideNewUserControl()
    {
      NewUserControl.Visibility = Visibility.Hidden;
      DiagramControl.Visibility = Visibility.Visible;

      if (family.Current != null)
      {
        DetailsControl.DataContext = family.Current;
      }
    }

    /// <summary>
    /// Shows the New User Control.
    /// </summary>
    private void ShowNewUserControl()
    {
      HideFamilyDataControl();
      HideDetailsPane();
      DiagramControl.Visibility = Visibility.Collapsed;
      WelcomeUserControl.Visibility = Visibility.Collapsed;

      if (PersonInfoControl.Visibility == Visibility.Visible)
      {
        ((Storyboard)Resources["HidePersonInfo"]).Begin(this);
      }

      NewUserControl.Visibility = Visibility.Visible;
      NewUserControl.ClearInputFields();
      NewUserControl.SetDefaultFocus();

      // Delete to clear existing files and re-Create the necessary directories
      string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.ApplicationFolderName);
      tempFolder = Path.Combine(tempFolder, FamilyShowLib.App.AppDataFolderName);
      People.RecreateDirectory(tempFolder);

      string photoFolder = Path.Combine(tempFolder, Photo.Const.PhotosFolderName);
      People.RecreateDirectory(photoFolder);

      string storyFolder = Path.Combine(tempFolder, Story.Const.StoriesFolderName);
      People.RecreateDirectory(storyFolder);
    }

    /// <summary>
    /// Shows the Welcome Screen user control and hides the other controls.
    /// </summary>
    private void ShowWelcomeScreen()
    {
      HideDetailsPane();
      HideNewUserControl();
      DiagramControl.Visibility = Visibility.Hidden;

      WelcomeUserControl.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Hides the Welcome Screen.
    /// </summary>
    private void HideWelcomeScreen()
    {
      WelcomeUserControl.Visibility = Visibility.Hidden;
    }

    /// <summary>
    /// Builds the Recent Files Menu
    /// </summary>
    private void BuildOpenMenu()
    {
      // Clear existing menu items
      OpenMenu.Items.Clear();

      // MenuItem for opening files
      MenuItem openMenuItem = new MenuItem
      {
        Header = "Open",
        Command = ApplicationCommands.Open
      };

      OpenMenu.Items.Add(openMenuItem);

      // Add the recent files to the menu as menu items
      if (App.RecentFiles.Count > 0)
      {
        // Separator between the open menu and the recent files
        OpenMenu.Items.Add(new Separator());

        foreach (string file in App.RecentFiles)
        {
          MenuItem item = new MenuItem
          {
            Header = Path.GetFileName(file),
            CommandParameter = file
          };

          item.Click += new RoutedEventHandler(OpenRecentFile_Click);
          OpenMenu.Items.Add(item);
        }
      }
    }

    /// <summary>
    /// Builds the Skins Menu
    /// </summary>
    private void BuildSkinsMenu()
    {
      NameValueCollection skins = App.Skins;
      foreach (string skinName in skins.AllKeys)
      {
        MenuItem skin = new MenuItem
        {
          Header = skinName,
          CommandParameter = skins[skinName],
          Command = ChangeSkinCommand
        };

        SkinsMenu.Items.Add(skin);
      }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      // Make sure the file is saved before the app is closed.
      PromptToSave();
      base.OnClosing(e);
    }

    /// <summary>
    /// Prompts the user to save the current family if it has been changed
    /// </summary>
    private void PromptToSave()
    {
      if (!family.IsDirty)
      {
        return;
      }

      MessageBoxResult result = MessageBox.Show(Properties.Resources.NotSavedMessage, Properties.Resources.NotSaved, MessageBoxButton.YesNo, MessageBoxImage.Warning);

      if (result == MessageBoxResult.Yes)
      {
        CommonDialog dialog = new CommonDialog
        {
          InitialDirectory = People.ApplicationFolderPath
        };

        dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyFiles, Properties.Resources.FamilyV3Extension));
        dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
        dialog.Title = Properties.Resources.SaveAs;
        dialog.DefaultExtension = Properties.Resources.DefaultFamilyExtension;
        dialog.ShowSave();

        if (!string.IsNullOrEmpty(dialog.FileName))
        {
          familyCollection.Save(dialog.FileName);

          if (!App.RecentFiles.Contains(familyCollection.FullyQualifiedFilename))
          {
            App.RecentFiles.Add(familyCollection.FullyQualifiedFilename);
            BuildOpenMenu();
          }
        }
      }
    }

    /// <summary>
    /// Shows the Old Verssion Message user control .
    /// </summary>
    private void ShowOldVersionMessage()
    {
      OldVersionMessageControl.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Hides the Old Version Message
    /// </summary>
    private void HideOldVersionMessage()
    {
      OldVersionMessageControl.Visibility = Visibility.Hidden;
    }

    public static DateTime StringToDate(string theDateInStringFormat)
    {
      DateTime result;
      if (DateTime.TryParse(theDateInStringFormat, out result))
      {
        return result;
      }

      return new DateTime();
    }

    #endregion
  }
}