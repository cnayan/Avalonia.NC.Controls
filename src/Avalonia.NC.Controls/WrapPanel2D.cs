using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Utilities;
using static System.Math;

namespace Avalonia.NC.Controls;

public sealed class WrapPanel2D : WrapPanel, INavigableContainer
{
    private readonly List<int> _widths = [];

    /// <summary>
    /// Gets the next control in the specified direction.
    /// </summary>
    /// <param name="direction">The movement direction.</param>
    /// <param name="from">The control from which movement begins.</param>
    /// <param name="wrap">Whether to wrap around when the first or last item is reached.</param>
    /// <returns>The control.</returns>
    IInputElement? INavigableContainer.GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        var orientation = Orientation;
        var children = Children;
        bool horiz = orientation == Orientation.Horizontal;
        int index = from != null ? Children.IndexOf((Control)from) : -1;

        switch (direction)
        {
            case NavigationDirection.First:
                index = 0;
                break;

            case NavigationDirection.Last:
                index = children.Count - 1;
                break;

            case NavigationDirection.Next:
                ++index;
                break;

            case NavigationDirection.Previous:
                --index;
                break;

            case NavigationDirection.Left:
                index = horiz ? index - 1 : -1;
                break;

            case NavigationDirection.Right:
                index = horiz ? index + 1 : -1;
                break;

            case NavigationDirection.Up:
                if (horiz)
                {
                    int currentLineNum = ComputeCurrentLine(index);
                    if (currentLineNum > 0 && index - _widths[currentLineNum - 1] > -1)
                    {
                        index -= _widths[currentLineNum - 1];
                    }
                    else
                    {
                        index = -1;
                    }
                }
                else
                {
                    index--;
                }

                break;

            case NavigationDirection.Down:
                if (horiz)
                {
                    int currentLineNum = ComputeCurrentLine(index);
                    if (index + _widths[currentLineNum] < children.Count)
                    {
                        index += _widths[currentLineNum];
                    }
                    else
                    {
                        index = -1;
                    }
                }
                else
                {
                    index++;
                }

                break;
        }

        if (index < 0 || index >= children.Count)
        {
            return null;
        }

        return children[index];
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size constraint)
    {
        _widths.Clear();

        double itemWidth = ItemWidth;
        double itemHeight = ItemHeight;
        double itemSpacing = ItemSpacing;
        double lineSpacing = LineSpacing;
        var orientation = Orientation;
        var children = Children;
        var curLineSize = new UVSize(orientation);
        var panelSize = new UVSize(orientation);
        var uvConstraint = new UVSize(orientation, constraint.Width, constraint.Height);
        bool itemWidthSet = !double.IsNaN(itemWidth);
        bool itemHeightSet = !double.IsNaN(itemHeight);
        bool itemExists = false;
        bool lineExists = false;

        var childConstraint = new Size(
            itemWidthSet ? itemWidth : constraint.Width,
            itemHeightSet ? itemHeight : constraint.Height);

        int c = 0, r = 0;
        for (int i = 0, count = children.Count; i < count; ++i)
        {
            var child = children[i];
            // Flow passes its own constraint to children
            child.Measure(childConstraint);

            // This is the size of the child in UV space
            UVSize childSize = new UVSize(orientation,
                itemWidthSet ? itemWidth : child.DesiredSize.Width,
                itemHeightSet ? itemHeight : child.DesiredSize.Height);

            var nextSpacing = itemExists && child.IsVisible ? itemSpacing : 0;
            if (MathUtilities.GreaterThan(curLineSize.U + childSize.U + nextSpacing, uvConstraint.U)) // Need to switch to another line
            {
                _widths.Add(c);

                r++;
                c = 1;

                panelSize.U = Max(curLineSize.U, panelSize.U);
                panelSize.V += curLineSize.V + (lineExists ? lineSpacing : 0);
                curLineSize = childSize;

                itemExists = child.IsVisible;
                lineExists = true;
            }
            else // Continue to accumulate a line
            {
                curLineSize.U += childSize.U + nextSpacing;
                curLineSize.V = Max(childSize.V, curLineSize.V);

                itemExists |= child.IsVisible; // keep true
                c++;
            }
        }

        if (c > 0)
        {
            _widths.Add(c);
        }

        // The last line size, if any should be added
        panelSize.U = Max(curLineSize.U, panelSize.U);
        panelSize.V += curLineSize.V + (lineExists ? lineSpacing : 0);

        // Go from UV space to W/H space
        return new Size(panelSize.Width, panelSize.Height);
    }

    /// <summary>
    /// Computes the line/row where the selected child is.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private int ComputeCurrentLine(int index)
    {
        int currentLineNum = 0;
        int idx = index;
        while (true)
        {
            if (_widths[currentLineNum] >= idx)
            {
                break;
            }

            idx -= _widths[currentLineNum];
            currentLineNum++;
        }

        return currentLineNum;
    }
    private struct UVSize
    {
        internal double U;

        internal double V;

        private Orientation _orientation;

        internal UVSize(Orientation orientation, double width, double height)
        {
            U = V = 0d;
            _orientation = orientation;
            Width = width;
            Height = height;
        }

        internal UVSize(Orientation orientation)
        {
            U = V = 0d;
            _orientation = orientation;
        }
        internal double Height
        {
            get => _orientation == Orientation.Horizontal ? V : U;
            set { if (_orientation == Orientation.Horizontal) V = value; else U = value; }
        }

        internal double Width
        {
            get => _orientation == Orientation.Horizontal ? U : V;
            set { if (_orientation == Orientation.Horizontal) U = value; else V = value; }
        }
    }
}