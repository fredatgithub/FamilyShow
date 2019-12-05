using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.FamilyShowLib;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using System.Drawing;

namespace Microsoft.FamilyShow
{
  /// <summary>
  /// Interaction logic for App.xaml.
  /// </summary>
  public partial class App : Application
  {
    #region fields

    // The name of the application folder.  This folder is used to save the files 
    // for this application such as the photos, stories and family data.
    internal const string ApplicationFolderName = "Family.Show";

    // The main list of family members that is shared for the entire application.
    // The FamilyCollection and Family fields are accessed from the same thread,
    // so suppressing the CA2211 code analysis warning.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
    public static People FamilyCollection = new People();
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
    public static PeopleCollection Family = FamilyCollection.PeopleCollection;

    // The number of recent files to keep track of.
    private const int NumberOfRecentFiles = 10;

    // The path to the recent files file.
    private readonly static string RecentFilesFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.Combine(ApplicationFolderName, "RecentFiles.xml"));

    // The global list of recent files.
    private static StringCollection recentFiles = new StringCollection();

    #endregion

    #region overrides

    /// <summary>
    /// Occurs when the application starts.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    protected override void OnStartup(StartupEventArgs e)
    {
      InstallSampleFiles();

      // Load the collection of recent files.
      LoadRecentFiles();

      Properties.Settings appSettings = FamilyShow.Properties.Settings.Default;

      if (!string.IsNullOrEmpty(appSettings.Skin))
      {
        try
        {
          ResourceDictionary resourceDictionary = new ResourceDictionary();
          resourceDictionary.MergedDictionaries.Add(LoadComponent(new Uri(appSettings.Skin, UriKind.Relative)) as ResourceDictionary);
          Current.Resources = resourceDictionary;
        }
        catch
        {
        }
      }

      base.OnStartup(e);
    }

    /// <summary>
    /// Occurs when the application exits.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
      SaveRecentFiles();
      base.OnExit(e);
    }
    #endregion

    #region methods

    /// <summary>
    /// Gets the list of recent files.
    /// </summary>
    public static StringCollection RecentFiles
    {
      get { return recentFiles; }
    }

    /// <summary>
    /// Gets the collection of skins
    /// </summary>
    public static NameValueCollection Skins
    {
      get
      {
        NameValueCollection skins = new NameValueCollection();

        foreach (string folder in Directory.GetDirectories(FamilyShow.Properties.Resources.Skins))
        {
          foreach (string file in Directory.GetFiles(folder))
          {
            FileInfo fileInfo = new FileInfo(file);
            if (string.Compare(fileInfo.Extension, FamilyShow.Properties.Resources.XamlExtension, true, CultureInfo.InvariantCulture) == 0)
            {
              // Use the first part of the resource file name for the menu item name.
              skins.Add(fileInfo.Name.Remove(fileInfo.Name.IndexOf(FamilyShow.Properties.Resources.ResourcesString)), Path.Combine(folder, fileInfo.Name));
            }
          }
        }

        return skins;
      }
    }

    /// <summary>
    /// Return the animation duration. The duration is extended
    /// if special keys are currently pressed (for demo purposes)  
    /// otherwise the specified duration is returned. 
    /// </summary>
    public static TimeSpan GetAnimationDuration(double milliseconds)
    {
      return TimeSpan.FromMilliseconds(Keyboard.IsKeyDown(Key.F12) ? milliseconds * 5 : milliseconds);
    }

    /// <summary>
    /// Load the list of recent files from disk.
    /// </summary>
    public static void LoadRecentFiles()
    {
      if (File.Exists(RecentFilesFilePath))
      {
        // Load the Recent Files from disk
        XmlSerializer ser = new XmlSerializer(typeof(StringCollection));
        using (TextReader reader = new StreamReader(RecentFilesFilePath))
        {
          recentFiles = (StringCollection)ser.Deserialize(reader);
        }

        // Remove files from the Recent Files list that no longer exists.
        for (int i = 0; i < recentFiles.Count; i++)
        {
          if (!File.Exists(recentFiles[i]))
          {
            recentFiles.RemoveAt(i);
          }
        }

        // Only keep the 5 most recent files, trim the rest.
        while (recentFiles.Count > NumberOfRecentFiles)
          recentFiles.RemoveAt(NumberOfRecentFiles);
      }
    }

    /// <summary>
    /// Save the list of recent files to disk.
    /// </summary>
    public static void SaveRecentFiles()
    {
      XmlSerializer ser = new XmlSerializer(typeof(StringCollection));
      using (TextWriter writer = new StreamWriter(RecentFilesFilePath))
      {
        ser.Serialize(writer, recentFiles);
      }
    }

    /// <summary>
    /// Install sample files the first time the application runs.
    /// </summary>
    private static void InstallSampleFiles()
    {
      // Full path to the document file location.
      string location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ApplicationFolderName);

      // Return right away if the data file already exist.
      if (Directory.Exists(location))
      {
        return;
      }

      try
      {
        // Sample data files.
        Directory.CreateDirectory(location);
        CreateSampleFile(location, "Windsor.family", FamilyShow.Properties.Resources.WindsorSampleFile);
        CreateSampleFile(location, "Kennedy.ged", FamilyShow.Properties.Resources.KennedySampleFile);

        // Sample image files.
        string imageLocation = Path.Combine(location, Photo.Const.PhotosFolderName);
        Directory.CreateDirectory(imageLocation);
        CreateSampleFile(imageLocation, "Edward VII.jpg", FamilyShow.Properties.Resources.Image_Edward_VII);
        CreateSampleFile(imageLocation, "Edward VIII.jpg", FamilyShow.Properties.Resources.Image_Edward_VIII);
        CreateSampleFile(imageLocation, "Elizabeth II.jpg", FamilyShow.Properties.Resources.Image_Elizabeth_II);
        CreateSampleFile(imageLocation, "George V.jpg", FamilyShow.Properties.Resources.Image_George_V);
        CreateSampleFile(imageLocation, "George VI.jpg", FamilyShow.Properties.Resources.Image_George_VI);
        CreateSampleFile(imageLocation, "Margaret Rose.jpg", FamilyShow.Properties.Resources.Image_Margaret_Rose);
        CreateSampleFile(imageLocation, "Prince Charles.jpg", FamilyShow.Properties.Resources.Image_Prince_Charles);
        CreateSampleFile(imageLocation, "Prince Henry.jpg", FamilyShow.Properties.Resources.Image_Prince_Henry);
        CreateSampleFile(imageLocation, "Prince William.jpg", FamilyShow.Properties.Resources.Image_Prince_William);
        CreateSampleFile(imageLocation, "Princess Diana.jpg", FamilyShow.Properties.Resources.Image_Princess_Diana);

        // Sample strory files.
        string storyLocation = Path.Combine(location, Story.Const.StoriesFolderName);
        Directory.CreateDirectory(storyLocation);
        CreateSampleFile(storyLocation, "Camilla Shand {cb2c1f69-5311-403a-948f-eaf74dd8e72d}.rtf", FamilyShow.Properties.Resources.Story_Camilla_Shand);
        CreateSampleFile(storyLocation, "Edward VII Wettin {I1}.rtf", FamilyShow.Properties.Resources.Story_Edward_VII_Wettin);
        CreateSampleFile(storyLocation, "Edward VIII Windsor {I5}.rtf", FamilyShow.Properties.Resources.Story_Edward_VIII_Windsor);
        CreateSampleFile(storyLocation, "Elizabeth II Alexandra Mary Windsor {I9}.rtf", FamilyShow.Properties.Resources.Story_Elizabeth_II_Alexandra_Mary_Windsor);
        CreateSampleFile(storyLocation, "George V Windsor {I3}.rtf", FamilyShow.Properties.Resources.Story_George_V_Windsor);
        CreateSampleFile(storyLocation, "George VI Windsor {I7}.rtf", FamilyShow.Properties.Resources.Story_George_VI_Windsor);
        CreateSampleFile(storyLocation, "Margaret Rose Windsor {I24}.rtf", FamilyShow.Properties.Resources.Story_Margaret_Rose_Windsor);
        CreateSampleFile(storyLocation, "Charles Philip Arthur Windsor {I11}.rtf", FamilyShow.Properties.Resources.Story_Charles_Philip_Arthur_Windsor);
        CreateSampleFile(storyLocation, "Diana Frances Spencer {I12}.rtf", FamilyShow.Properties.Resources.Story_Diana_Frances_Spencer);
      }
      catch
      {
        // Could not install the sample files, handle all exceptions the same
        // ignore and continue without installing the sample files.
      }
    }

    /// <summary>
    /// Extract the sample family files from the executable and write it to the file system.
    /// </summary>
    private static void CreateSampleFile(string location, string fileName, byte[] fileContent)
    {
      // Full path to the sample file.
      string path = Path.Combine(location, fileName);

      // Return right away if the file already exists.
      if (File.Exists(path))
      {
        return;
      }

      // Create the file.
      using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
      {
        writer.Write(fileContent);
      }
    }

    private static void CreateSampleFile(string location, string fileName, string fileContent)
    {
      // Full path to the sample file.
      string path = Path.Combine(location, fileName);

      // Return right away if the file already exists.
      if (File.Exists(path))
      {
        return;
      }

      // Create the file.
      using (StreamWriter writer = new StreamWriter(File.Open(path, FileMode.Create)))
      {
        writer.Write(fileContent);
      }
    }

    private static void CreateSampleFile(string location, string fileName, Bitmap image)
    {
      // Full path to the sample file.
      string path = Path.Combine(location, fileName);

      // Return right away if the file already exists.
      if (File.Exists(path))
      {
        return;
      }

      // Create the file.
      image.Save(path);
    }

    /// <summary>
    /// Converts string to date time object using DateTime.TryParse.  
    /// Also accepts just the year for dates. 1977 = 1/1/1977.
    /// </summary>
    internal static DateTime StringToDate(string dateString)
    {
      // Append first month and day if just the year was entered.
      if (dateString.Length == 4)
      {
        dateString = "1/1/" + dateString;
      }

      DateTime.TryParse(dateString, out DateTime date);

      return date;
    }

    #endregion
  }
}