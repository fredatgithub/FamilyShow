using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Data;
using System.Windows.Markup;

namespace FamilyShow
{
  internal sealed class LocalizationManager : INotifyPropertyChanged
  {
    private static readonly LocalizationManager instance = new LocalizationManager();
    private static string language = "fr";

    public static LocalizationManager Instance => instance;
    public static string Language => language;

    public string this[string key] => Resources.Strings.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    public event PropertyChangedEventHandler PropertyChanged;

    public static string GetString(string key)
    {
      return Instance[key];
    }

    public static void SetLanguage(string value, bool save = true)
    {
      string selectedLanguage = string.Equals(value, "en", StringComparison.OrdinalIgnoreCase)
        ? "en"
        : string.Equals(value, "de", StringComparison.OrdinalIgnoreCase) ? "de"
        : string.Equals(value, "es", StringComparison.OrdinalIgnoreCase) ? "es" : "fr";
      CultureInfo culture = CultureInfo.GetCultureInfo(selectedLanguage);
      language = selectedLanguage;
      CultureInfo.DefaultThreadCurrentCulture = culture;
      CultureInfo.DefaultThreadCurrentUICulture = culture;
      Thread.CurrentThread.CurrentCulture = culture;
      Thread.CurrentThread.CurrentUICulture = culture;
      Resources.Strings.Culture = culture;

      if (save)
      {
        Properties.Settings.Default.Language = selectedLanguage;
      }

      instance.PropertyChanged?.Invoke(instance, new PropertyChangedEventArgs(string.Empty));
    }
  }

  [MarkupExtensionReturnType(typeof(object))]
  public sealed class LocExtension : MarkupExtension
  {
    public LocExtension(string key)
    {
      Key = key;
    }

    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return new Binding($"[{Key}]") { Source = LocalizationManager.Instance }.ProvideValue(serviceProvider);
    }
  }
}