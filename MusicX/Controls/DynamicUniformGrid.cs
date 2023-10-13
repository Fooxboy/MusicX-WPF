using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MusicX.Controls;

public class DynamicUniformGrid : Panel
{
    //-------------------------------------------------------------------
    //
    //  Constructors
    //
    //-------------------------------------------------------------------

    #region Constructors

    /// <summary>
    /// Default constructor.
    /// </summary>
    public DynamicUniformGrid()
    {
    }

    #endregion Constructors

    //-------------------------------------------------------------------
    //
    //  Public Properties
    //
    //-------------------------------------------------------------------

    #region Public Properties

    /// <summary>
    /// the start column to arrange children. Leave first 'FirstColumn' 
    /// cells blank.
    /// </summary>
    public int FirstColumn
    {
        get { return (int)GetValue(FirstColumnProperty); }
        set { SetValue(FirstColumnProperty, value); }
    }

    /// <summary>
    /// FirstColumnProperty
    /// </summary>
    public static readonly DependencyProperty FirstColumnProperty =
            DependencyProperty.Register(
                    "FirstColumn",
                    typeof(int),
                    typeof(DynamicUniformGrid),
                    new FrameworkPropertyMetadata(
                            (int)0,
                            FrameworkPropertyMetadataOptions.AffectsMeasure),
                    new ValidateValueCallback(ValidateFirstColumn));

    private static bool ValidateFirstColumn(object o)
    {
        return (int)o >= 0;
    }

    /// <summary>
    /// Specifies the number of columns in the grid
    /// A value of 0 indicates that the column count should be dynamically 
    /// computed based on the number of rows (if specified) and the 
    /// number of non-collapsed children in the grid
    /// </summary>
    public int Columns
    {
        get { return (int)GetValue(ColumnsProperty); }
        set { SetValue(ColumnsProperty, value); }
    }

    /// <summary>
    /// DependencyProperty for <see cref="Columns" /> property.
    /// </summary>
    public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(
                    "Columns",
                    typeof(int),
                    typeof(DynamicUniformGrid),
                    new FrameworkPropertyMetadata(
                            (int)0,
                            FrameworkPropertyMetadataOptions.AffectsMeasure),
                    new ValidateValueCallback(ValidateColumns));

    private static bool ValidateColumns(object o)
    {
        return (int)o >= 0;
    }

    /// <summary>
    /// Specifies the number of rows in the grid
    /// A value of 0 indicates that the row count should be dynamically 
    /// computed based on the number of columns (if specified) and the 
    /// number of non-collapsed children in the grid
    /// </summary>
    public int Rows
    {
        get { return (int)GetValue(RowsProperty); }
        set { SetValue(RowsProperty, value); }
    }

    /// <summary>
    /// DependencyProperty for <see cref="Rows" /> property.
    /// </summary>
    public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(
                    "Rows",
                    typeof(int),
                    typeof(DynamicUniformGrid),
                    new FrameworkPropertyMetadata(
                            (int)0,
                            FrameworkPropertyMetadataOptions.AffectsMeasure),
                    new ValidateValueCallback(ValidateRows));

    private static bool ValidateRows(object o)
    {
        return (int)o >= 0;
    }


    #endregion Public Properties

    //-------------------------------------------------------------------
    //
    //  Protected Methods
    //
    //-------------------------------------------------------------------

    #region Protected Methods

    /// <summary>
    /// Compute the desired size of this UniformGrid by measuring all of the
    /// children with a constraint equal to a cell's portion of the given
    /// constraint (e.g. for a 2 x 4 grid, the child constraint would be
    /// constraint.Width*0.5 x constraint.Height*0.25).  The maximum child
    /// width and maximum child height are tracked, and then the desired size
    /// is computed by multiplying these maximums by the row and column count
    /// (e.g. for a 2 x 4 grid, the desired size for the UniformGrid would be
    /// maxChildDesiredWidth*2 x maxChildDesiredHeight*4).
    /// </summary>
    /// <param name="constraint">Constraint</param>
    /// <returns>Desired size</returns>
    protected override Size MeasureOverride(Size constraint)
    {
        UpdateComputedValues();

        var childConstraint = new Size(constraint.Width / _columns, constraint.Height / _rows);
        var maxChildDesiredWidth = 0.0;
        var maxChildDesiredHeight = 0.0;

        //  Measure each child, keeping track of maximum desired width and height.
        for (int i = 0, count = InternalChildren.Count; i < count; ++i)
        {
            var child = InternalChildren[i];

            // Measure the child.
            child.Measure(childConstraint);
            var childDesiredSize = child.DesiredSize;

            if (maxChildDesiredWidth < childDesiredSize.Width)
            {
                maxChildDesiredWidth = childDesiredSize.Width;
            }

            if (maxChildDesiredHeight < childDesiredSize.Height)
            {
                maxChildDesiredHeight = childDesiredSize.Height;
            }
        }

        return new Size(maxChildDesiredWidth * _columns, maxChildDesiredHeight * _rows);
    }

    /// <summary>
    /// Arrange the children of this UniformGrid by distributing space evenly 
    /// among all of the children, making each child the size equal to a cell's
    /// portion of the given arrangeSize (e.g. for a 2 x 4 grid, the child size
    /// would be arrangeSize*0.5 x arrangeSize*0.25)
    /// </summary>
    /// <param name="arrangeSize">Arrange size</param>
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        var childBounds = new Rect(0, 0, arrangeSize.Width / _columns, arrangeSize.Height / _rows);
        var yStep = childBounds.Height;
        var yBound = arrangeSize.Height - 1.0;

        childBounds.X += childBounds.Width * FirstColumn;

        // Arrange and Position each child to the same cell size
        foreach (UIElement child in InternalChildren)
        {
            child.Arrange(childBounds);

            // only advance to the next grid cell if the child was not collapsed
            if (child.Visibility != Visibility.Collapsed)
            {
                childBounds.Y += yStep;
                if (childBounds.Y >= yBound)
                {
                    childBounds.X += childBounds.Width;
                    childBounds.Y = 0;
                }
            }
        }

        return arrangeSize;
    }

    #endregion Protected Methods

    //------------------------------------------------------
    //
    //  Private Methods
    //
    //------------------------------------------------------

    #region Private Methods

    /// <summary>
    /// If either Rows or Columns are set to 0, then dynamically compute these
    /// values based on the actual number of non-collapsed children.
    ///
    /// In the case when both Rows and Columns are set to 0, then make Rows 
    /// and Columns be equal, thus laying out in a square grid.
    /// </summary>
    private void UpdateComputedValues()
    {
        _columns = Columns;
        _rows = Rows;

        //parameter checking. 
        if (FirstColumn >= _columns)
        {
            //NOTE: maybe we shall throw here. But this is somewhat out of 
            //the MCC itself. We need a whole new panel spec.
            FirstColumn = 0;
        }

        if (_rows != 0 && _columns != 0) return;
        
        var nonCollapsedCount = 0;

        // First compute the actual # of non-collapsed children to be laid out
        for (int i = 0, count = Children.Count; i < count; ++i)
        {
            var child = Children[i];
            if (child.Visibility != Visibility.Collapsed)
            {
                nonCollapsedCount++;
            }
        }

        // to ensure that we have at leat one row & column, make sure
        // that nonCollapsedCount is at least 1
        if (nonCollapsedCount == 0)
        {
            nonCollapsedCount = 1;
        }

        if (_rows == 0)
        {
            if (_columns > 0)
            {
                // take FirstColumn into account, because it should really affect the result
                _rows = (nonCollapsedCount + FirstColumn + (_columns - 1)) / _columns;
            }
            else
            {
                // both rows and columns are unset -- lay out in a square
                _rows = (int)Math.Sqrt(nonCollapsedCount);
                _columns = _rows;
                if (_rows * _rows < nonCollapsedCount)
                {
                    _columns++;
                }
            }
        }
        else if (_columns == 0)
        {
            // guaranteed that _rows is not 0, because we're in the else clause of the check for _rows == 0
            _columns = (nonCollapsedCount + (_rows - 1)) / _rows;
        }
    }

    #endregion Private Properties

    private int _rows;
    private int _columns;
}
