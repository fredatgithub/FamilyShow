/*
 * One node in the diagram. The control is a Button with a redefined control template.
 *
 * The control templates (and other resources) are specified in Skins/<name>/Resources/DiagramResources.xaml. 
 * This is a resource dictionary that is part of the application resources. If the resources
 * were specified as part of the control, every instance of the control would allocate memory
 * for each resource. Specifying the resources at the application level only allocates one copy
 * of the resources which are shared with all instances of the control. The resources could be
 * specified in the application's generic dictionary also (themes/generic.xaml).
 * 
 * Instead of specifying every possible node combination as a resource, only four control templates 
 * are specified: female, male, primary female, and primary male. Then different brushes are used 
 * depending on the node state. For example: sibling brush, related deceased brush, spouse brush.
 * This reduces the amount of code in the XAML file, but requires code that determines what control
 * template and brush resource to use based on the node's state.
 * 
*/

using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Globalization;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow
{
  /// <summary>
  /// The type of node.
  /// </summary>
  public enum NodeType
  {
    Primary,
    Related,
    Spouse,
    Sibling,
    SiblingLeft,
    SiblingRight
  }

  /// <summary>
  /// Node in the diagram.
  /// </summary>
  public partial class DiagramNode : Button
  {
    private static class Const
    {
      public static double OpacityFiltered = 0.15;
      public static double OpacityNormal = 1.0;
      public static double AnimationDuration = 300;
    }

    #region fields

    // Person object associated with the node.
    private Person person;

    // Location of the node, relative to its parent group.
    private Point location = new Point();

    // The type of node.
    private NodeType type = NodeType.Related;

    // The amount to scale the node. 
    private double scale = 1;

    // The current display year, this is used for the time filter.
    private double displayYear = DateTime.Now.Year;

    // Flag, true if this node is currently filtered. This means
    // its still displayed but in a dim state.
    private bool isFiltered;

    #endregion

    #region properties

    /// <summary>
    /// Get the fill brush for the node based on the node type.
    /// </summary>
    public Brush NodeFill
    {
      get { return GetBrushResource("Fill"); }
    }

    /// <summary>
    /// Get the hover fill brush for the node based on the node type.
    /// </summary>
    public Brush NodeHoverFill
    {
      get { return GetBrushResource("HoverFill"); }
    }

    /// <summary>
    /// Get the stroke brush for the node based on the node type.
    /// </summary>
    public Brush NodeStroke
    {
      get { return GetBrushResource("Stroke"); }
    }

    /// <summary>
    /// Get the fill brush for the group indicator.
    /// </summary>
    public Brush GroupFill
    {
      get { return GetGroupBrushResource("GroupFill"); }
    }

    /// <summary>
    /// Get the stroke brush for the group indicator.
    /// </summary>
    public Brush GroupStroke
    {
      get { return GetGroupBrushResource("GroupStroke"); }
    }

    /// <summary>
    /// Get or set the display year. This filters the node based on date information.
    /// </summary>
    public double DisplayYear
    {
      get { return displayYear; }
      set
      {
        displayYear = value;

        // Update the filtered state based on the birth date.
        IsFiltered = (person != null && person.BirthDate != null &&
            person.BirthDate.Value.Year > displayYear);

        // Recompuate the bottom label which contains the age,
        // the new age is relative to the new display year
        UpdateBottomLabel();
      }
    }

    /// <summary>
    /// Get or set if the node is filtered.
    /// </summary>
    public bool IsFiltered
    {
      get { return isFiltered; }
      set
      {
        if (isFiltered != value)
        {
          // The filtered state changed, create a new animation.
          isFiltered = value;
          double newOpacity = isFiltered ? Const.OpacityFiltered : Const.OpacityNormal;
          BeginAnimation(DiagramNode.OpacityProperty,
              new DoubleAnimation(Opacity, newOpacity,
              App.GetAnimationDuration(Const.AnimationDuration)));
        }
      }
    }

    /// <summary>
    /// Born, died and age information. 
    /// </summary>
    private string DateInformation
    {
      get
      {
        // Living, example: 1900 | 107
        if (person.IsLiving)
        {
          if (person.BirthDate == null)
            return string.Empty;

          if (!person.Age.HasValue)
            return string.Empty;

          int age = person.Age.Value - (DateTime.Now.Year - (int)displayYear);
          return string.Format(CultureInfo.CurrentUICulture,
              "{0} | {1}", person.BirthDate.Value.Year, Math.Max(0, age));
        }

        // Deceased, example: 1900 - 1950 | 50                    
        if (person.BirthDate != null && person.DeathDate != null)
        {
          if (!person.Age.HasValue)
            return string.Empty;

          int age = (displayYear >= person.DeathDate.Value.Year) ?
              person.Age.Value : person.Age.Value - (person.DeathDate.Value.Year - (int)displayYear);

          return string.Format(CultureInfo.CurrentUICulture,
              "{0} - {1} | {2}", person.BirthDate.Value.Year,
              person.DeathDate.Value.Year, Math.Max(0, age));
        }

        // Deceased, example: ? - 1950 | ?
        if (person.BirthDate == null && person.DeathDate != null)
        {
          return string.Format(CultureInfo.CurrentUICulture,
              "? - {0} | ?", person.DeathDate.Value.Year);
        }

        // Deceased, example: 1900 - ? | ?
        if (person.BirthDate != null && person.DeathDate == null)
        {
          return string.Format(CultureInfo.CurrentUICulture,
              "{0} - ? | ?", person.BirthDate.Value.Year);
        }

        return string.Empty;
      }
    }

    /// <summary>
    /// Person object associated with the node.
    /// </summary>
    public Person Person
    {
      get { return person; }
      set
      {
        person = value;
        DataContext = this;

        // Update the template to reflect the gender.
        UpdateTemplate();

        UpdateBottomLabel();
      }
    }

    /// <summary>
    /// Set the scale value of the node.
    /// </summary>
    public double Scale
    {
      get { return scale; }
      set
      {
        // Save the scale value, used later after apply the node template.
        scale = value;
      }
    }

    /// <summary>
    /// Location of the node relative to the parent group.
    /// </summary>
    public Point Location
    {
      get { return location; }
      set { location = value; }
    }

    /// <summary>
    /// Get the center of the node.
    /// </summary>
    public Point Center
    {
      get
      {
        return new Point(
            location.X + (DesiredSize.Width / 2),
            location.Y + (DesiredSize.Height / 2));
      }
    }

    /// <summary>
    /// Get the top center of the node. The center is shifted to the left since the 
    /// person drawing is not located in the true center of the node, it's shifted
    /// to the left due to the shadow.
    /// </summary>
    public Point TopCenter
    {
      get
      {
        // The real center of the node.
        Point point = new Point(location.X + (DesiredSize.Width / 2), location.Y);

        // Shift the center to the left. This is an estimate since we don't 
        // know the exact position of the person drawing within the node.
        FrameworkElement personElement = Template.FindName("Person", this) as FrameworkElement;
        double offset = (type == NodeType.Primary) ? 12 : 5;
        point.X -= (personElement.ActualWidth / offset);
        return point;
      }
    }

    /// <summary>
    /// Get the top right of the node.
    /// </summary>
    public Point TopRight
    {
      get { return new Point(location.X + DesiredSize.Width, location.Y); }
    }

    /// <summary>
    /// Get the top left of the node.
    /// </summary>
    public Point TopLeft
    {
      get { return new Point(location.X, location.Y); }
    }

    /// <summary>
    /// Get the bottom center of the node.
    /// </summary>
    public Point BottomCenter
    {
      get
      {
        return new Point(location.X + (DesiredSize.Width / 2),
          location.Y + DesiredSize.Height);
      }
    }

    /// <summary>
    /// Get the left center of the node.
    /// </summary>
    public Point LeftCenter
    {
      get { return new Point(location.X, location.Y + (DesiredSize.Height / 2)); }
    }

    /// <summary>
    /// Get the right center of the node.
    /// </summary>
    public Point RightCenter
    {
      get { return new Point(location.X + DesiredSize.Width, location.Y + (DesiredSize.Height / 2)); }
    }

    /// <summary>
    /// The type of node.
    /// </summary>
    public NodeType Type
    {
      get { return type; }
      set
      {
        type = value;
        UpdateTemplate();
      }
    }

    #endregion

    #region dependency properties

    public static readonly DependencyProperty BottomLabelProperty =
        DependencyProperty.Register("BottomLabel", typeof(string), typeof(DiagramNode));

    /// <summary>
    /// The text displayed below the node.
    /// </summary>
    public string BottomLabel
    {
      get { return (String)GetValue(DiagramNode.BottomLabelProperty); }
      set { SetValue(DiagramNode.BottomLabelProperty, value); }
    }

    #endregion

    #region overrides

    public override void OnApplyTemplate()
    {
      // The template has been applied to the node. See if the person drawing needs to be scaled.
      if (scale != 1)
      {
        // Scale the person drawing part of the node, not the entire node.
        FrameworkElement personElement = Template.FindName("Person", this) as FrameworkElement;
        if (personElement != null)
        {
          ScaleTransform transform = new ScaleTransform(scale, scale);
          personElement.LayoutTransform = transform;
        }
      }

      // The template changed, determine if the group
      // indicator should be displayed.
      UpdateGroupIndicator();

      base.OnApplyTemplate();
    }

    #endregion

    /// <summary>
    /// Return the brush resouse based on the node type.
    /// </summary>
    private Brush GetBrushResource(string part)
    {
      // Format string, the resource is in the XAML file.
      string resourceName = string.Format(
          CultureInfo.InvariantCulture, "{0}{1}{2}{3}",
          (person.Gender == Gender.Female) ? "Female" : "Male",
          type.ToString(),
          person.IsLiving ? "Living" : "Deceased",
          part);

      return (Brush)TryFindResource(resourceName);
    }

    private Brush GetGroupBrushResource(string part)
    {
      // Format string, the resource is in the XAML file.
      string resourceName = string.Format(CultureInfo.InvariantCulture,
          "{0}{1}", type.ToString(), part);

      return (Brush)TryFindResource(resourceName);
    }

    /// <summary>
    /// Update the node template based on the node type.
    /// </summary>
    private void UpdateTemplate()
    {
      // Determine the node template based on node properties.
      string template = string.Format(
          CultureInfo.InvariantCulture, "{0}{1}NodeTemplate",
          (person.Gender == Gender.Female) ? "Female" : "Male",
          (type == NodeType.Primary) ? "Primary" : "");

      // Assign the node template.                
      Template = (ControlTemplate)FindResource(template);
    }

    /// <summary>
    /// Hide or show the group indicator for this node.
    /// </summary>
    private void UpdateGroupIndicator()
    {
      // Primary templates don't have the group xaml section.
      if (type == NodeType.Primary)
        return;

      // Determine if the group indicator should be displayed.
      bool isGrouping = ShouldDisplayGroupIndicator();

      FrameworkElement element = Template.FindName("Group", this) as FrameworkElement;
      if (element != null)
        element.Visibility = isGrouping ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Return true if the group indicator should be displayed.
    /// </summary>
    private bool ShouldDisplayGroupIndicator()
    {
      // Primary and related nodes never display the group indicator.
      if (type == NodeType.Primary || type == NodeType.Related)
        return false;

      bool show = false;
      switch (type)
      {
        // Spouse - if have parents, siblings, or ex spouses.
        case NodeType.Spouse:
          if (person.Parents.Count > 0 ||
              person.Siblings.Count > 0 ||
              person.PreviousSpouses.Count > 0)
            show = true;
          break;

        // Sibling - if have spouse, or children.
        case NodeType.Sibling:
          if (person.Spouses.Count > 0 ||
              person.Children.Count > 0)
            show = true;
          break;

        // Half sibling - like sibling, but also inherits the 
        // group status from all parents.
        case NodeType.SiblingLeft:
        case NodeType.SiblingRight:
          if (person.Spouses.Count > 0 ||
              person.Children.Count > 0)
            show = true;
          break;
      }

      return show;
    }

    /// <summary>
    /// Update the bottom label which contains the name, year range and age.
    /// </summary>
    private void UpdateBottomLabel()
    {
      string label = string.Format(CultureInfo.CurrentCulture, "{0}\r{1}",
          person.FullName, DateInformation);
      BottomLabel = label;
    }
  }
}
