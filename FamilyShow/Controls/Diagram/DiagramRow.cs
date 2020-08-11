/*
 * One row in the diagram. Contains a collection of DiagramGroup objects 
 * that are arranged within the row.
 * 
 * Supports the VerticalAlignment property so groups can be top or bottom 
 * aligned. Supports the Margin property which is used to control space
 * between the rows on the diagram.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.FamilyShow.Controls.Diagram
{
  /// <summary>
  /// Row in the diagram that contains group objects.
  /// </summary>
  public class DiagramRow : FrameworkElement
  {
    #region fields

    // Space between each group.
    private double groupSpace = 80;

    // Location of the row, relative to the diagram.
    private Point location = new Point();

    // List of groups in the row.
    private List<DiagramGroup> groups = new List<DiagramGroup>();

    #endregion

    #region properties

    /// <summary>
    /// Space between each group.
    /// </summary>
    public double GroupSpace
    {
      get { return groupSpace; }
      set { groupSpace = value; }
    }

    /// <summary>
    /// Location of the row, relative to the diagram.
    /// </summary>
    public Point Location
    {
      get { return location; }
      set { location = value; }
    }

    /// <summary>
    /// List of groups in the row.
    /// </summary>
    public ReadOnlyCollection<DiagramGroup> Groups
    {
      get { return new ReadOnlyCollection<DiagramGroup>(groups); }
    }

    public int NodeCount
    {
      get
      {
        int count = 0;
        foreach (DiagramGroup group in groups)
          count += group.Nodes.Count;
        return count;
      }
    }

    #endregion

    #region overrides

    protected override Size MeasureOverride(Size availableSize)
    {
      // Let each group determine how large they want to be.
      Size size = new Size(double.PositiveInfinity, double.PositiveInfinity);
      foreach (DiagramGroup group in groups)
      {
        group.Measure(size);
      }

      // Return the total size of the row.
      return ArrangeGroups(false);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      // Arrange the groups in the row, return the total size.
      return ArrangeGroups(true);
    }

    protected override int VisualChildrenCount
    {
      // Return the number of groups.
      get { return groups.Count; }
    }

    protected override Visual GetVisualChild(int index)
    {
      // Return the requested group.
      return groups[index];
    }

    #endregion

    /// <summary>
    /// Add the group to the row.
    /// </summary>
    public void Add(DiagramGroup group)
    {
      groups.Add(group);
      AddVisualChild(group);
    }

    /// <summary>
    /// Remove all groups from the row.
    /// </summary>
    public void Clear()
    {
      foreach (DiagramGroup group in groups)
      {
        group.Clear();
        RemoveVisualChild(group);
      }

      groups.Clear();
    }

    /// <summary>
    /// Arrange the groups in the row, return the total size.
    /// </summary>
    private Size ArrangeGroups(bool arrange)
    {
      // Position of the next group.
      double pos = 0;

      // Bounding area of the group.
      Rect bounds = new Rect();

      // Total size of the row.
      Size totalSize = new Size(0, 0);

      foreach (DiagramGroup group in groups)
      {
        // Group location.
        bounds.X = pos;
        bounds.Y = 0;

        // Group size.                    
        bounds.Width = group.DesiredSize.Width;
        bounds.Height = group.DesiredSize.Height;

        // Arrange the group, save the location.
        if (arrange)
        {
          group.Arrange(bounds);
          group.Location = bounds.TopLeft;
        }

        // Update the size of the row.
        totalSize.Width = pos + group.DesiredSize.Width;
        totalSize.Height = Math.Max(totalSize.Height, group.DesiredSize.Height);

        pos += (bounds.Width + groupSpace);
      }

      return totalSize;
    }
  }
}
