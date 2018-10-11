﻿/* ------------------------------------------------------------------------- */
//
// Copyright (c) 2010 CubeSoft, Inc.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
/* ------------------------------------------------------------------------- */
using Cube.Xui;
using Cube.Xui.Behaviors;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cube.Pdf.App.Editor
{
    /* --------------------------------------------------------------------- */
    ///
    /// MouseMoveBehavior
    ///
    /// <summary>
    /// Represents the action to move items through the drag&amp;drop
    /// event.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public sealed class MouseMoveBehavior : CommandBehavior<ListView>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// MouseMove
        ///
        /// <summary>
        /// Initializes a new instance of the MouseMove class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MouseMoveBehavior()
        {
            Drawing = new Border
            {
                BorderBrush     = SystemColors.HotTrackBrush.Clone(),
                BorderThickness = new Thickness(1),
                Background      = SystemColors.HotTrackBrush.Clone(),
                CornerRadius    = new CornerRadius(1),
            };

            Drawing.Background.Opacity = 0.1;
            Drawing.MouseEnter += WhenMouseEnter;
            Drawing.DragOver   += WhenDragOver;
            Drawing.Drop       += WhenDrop;

            DrawingCanvas = new Canvas { Visibility = Visibility.Collapsed };
            DrawingCanvas.Children.Add(Drawing);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Selection
        ///
        /// <summary>
        /// Gets or sets the collection of selected items.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ImageSelection Selection
        {
            get => GetValue(SelectionProperty) as ImageSelection;
            set => SetValue(SelectionProperty, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// DrawingCanvas
        ///
        /// <summary>
        /// Gets the canvas to draw the moving position.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Canvas DrawingCanvas { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Drawing
        ///
        /// <summary>
        /// Gets the drawing object.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Border Drawing { get; }

        #endregion

        #region Dependencies

        /* ----------------------------------------------------------------- */
        ///
        /// SelectionProperty
        ///
        /// <summary>
        /// Gets a dependency object for the Selection property.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static DependencyProperty SelectionProperty =
            DependencyFactory.Create<MouseMoveBehavior, ImageSelection>(
                nameof(Selection), (s, e) => s.Selection = e
            );

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnAttached
        ///
        /// <summary>
        /// Called when the action is attached to an AssociatedObject.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AllowDrop = true;
            AssociatedObject.PreviewMouseLeftButtonDown += WhenMouseDown;
            AssociatedObject.MouseMove += WhenMouseMove;
            AssociatedObject.MouseEnter += WhenMouseEnter;
            AssociatedObject.DragOver += WhenDragOver;
            AssociatedObject.Drop += WhenDrop;

            _attached = AssociatedObject.GetParent<Panel>();
            _attached?.Children.Add(DrawingCanvas);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnDetaching
        ///
        /// <summary>
        /// Called when the action is being detached from its
        /// AssociatedObject, but before it has actually occurred.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= WhenMouseDown;
            AssociatedObject.MouseMove -= WhenMouseMove;
            AssociatedObject.MouseEnter -= WhenMouseEnter;
            AssociatedObject.DragOver -= WhenDragOver;
            AssociatedObject.Drop -= WhenDrop;

            _attached?.Children.Remove(DrawingCanvas);
            base.OnDetaching();
        }

        #region EventHandler

        /* ----------------------------------------------------------------- */
        ///
        /// WhenMouseDown
        ///
        /// <summary>
        /// Occurs when the LeftMouseButtonDown event is fired.
        /// </summary>
        ///
        /// <remarks>
        /// TODO: 複数項目の Drag&amp;Drop 処理に問題があるため対応を
        /// 要検討。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenMouseDown(object s, MouseEventArgs e)
        {
            Debug.Assert(AssociatedObject.Items != null);

            var pt    = e.GetPosition(AssociatedObject);
            var item  = AssociatedObject.GetObject<ListViewItem>(pt);
            var index = (item != null) ? AssociatedObject.Items.IndexOf(item.Content) : -1;

            e.Handled = item?.IsSelected ?? false;
            if (e.Handled) Drag(index);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenMouseMove
        ///
        /// <summary>
        /// Occurs when the MouseMove event is fired.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenMouseMove(object s, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) WhenMouseDown(s, e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenMouseEnter
        ///
        /// <summary>
        /// Occurs when the MouseEnter event is fired.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenMouseEnter(object s, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) return;
            DrawingCanvas.Visibility = Visibility.Collapsed;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenDragOver
        ///
        /// <summary>
        /// Occurs when the DragOver event is fired.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenDragOver(object s, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.Serializable) is DragDropObject obj && obj.DragIndex >= 0)
            {
                if (DrawingCanvas.Visibility != Visibility.Visible) DrawingCanvas.Visibility = Visibility.Visible;

                e.Handled = true;
                e.Effects = DragDropEffects.Move;

                var pt   = e.GetPosition(AssociatedObject);
                var unit = GetBounds();

                Scroll(obj, pt, unit);
                Draw(obj, pt, unit);
            }
            else e.Handled = false;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenDrop
        ///
        /// <summary>
        /// Occurs when the Drop event is fired.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenDrop(object s, DragEventArgs e)
        {
            DrawingCanvas.Visibility = Visibility.Collapsed;
            if (e.Data.GetData(DataFormats.Serializable) is DragDropObject obj)
            {
                obj.DropIndex = GetTargetIndex(obj, e.GetPosition(AssociatedObject), GetBounds());
                if (Command?.CanExecute(obj) ?? false) Command.Execute(obj);
            }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// Drag
        ///
        /// <summary>
        /// Starts the Drag&amp;Drop action to move items.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Drag(int index) => DragDrop.DoDragDrop(AssociatedObject,
            new DataObject(DataFormats.Serializable, new DragDropObject
            {
                Pid       = Process.GetCurrentProcess().Id,
                DragIndex = index,
                DropIndex = -1,
                Selection = Selection.Items.Select(e => e.RawObject).ToList(),
            }),
            DragDropEffects.Copy | DragDropEffects.Move);

        /* ----------------------------------------------------------------- */
        ///
        /// Draw
        ///
        /// <summary>
        /// Draws the moving position.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Draw(DragDropObject src, Point pt, Rect unit)
        {
            var dest = GetIndex(src, pt, unit);
            var ok   = src.DragIndex >= 0 && dest >= 0;

            DrawingCanvas.Visibility = ok ? Visibility.Visible : Visibility.Collapsed;
            if (!ok) return;

            var n    = AssociatedObject.Items.Count;
            var rect = AssociatedObject.GetBounds(Math.Max(Math.Min(dest, n - 1), 0));
            var cvt  = Conver(pt, _attached);

            var w = rect.Width + 6;
            var h = rect.Height + 6;
            var o = Conver(new Point(rect.Left + w / 2, rect.Top + h / 6), _attached);
            var x = (dest == n || cvt.X >= o.X) ? o.X : o.X - w;
            var y = o.Y;

            Canvas.SetLeft(Drawing, x);
            Canvas.SetTop(Drawing, y);

            Drawing.Width  = w;
            Drawing.Height = h * (2 / 3.0);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Scroll
        ///
        /// <summary>
        /// Moves the vertical scroll bar.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Scroll(DragDropObject src, Point pt, Rect unit)
        {
            var sv = AssociatedObject.GetChild<ScrollViewer>();
            if (sv == null) return;

            var height = AssociatedObject.ActualHeight;
            var margin = height / 5.0;
            var offset = Math.Max(unit.Height / 10, 50);

            if (pt.Y < margin) sv.ScrollToVerticalOffset(sv.VerticalOffset - offset);
            else if (pt.Y > height - margin) sv.ScrollToVerticalOffset(sv.VerticalOffset + offset);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetTargetIndex
        ///
        /// <summary>
        /// Gets the item index located at the specified point.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private int GetTargetIndex(DragDropObject src, Point pt, Rect unit)
        {
            var index = GetIndex(src, pt, unit);
            if (index == -1 || index == AssociatedObject.Items.Count) return index;

            var rect = AssociatedObject.GetBounds(index);
            var x    = rect.Left + rect.Width / 2;
            var y    = rect.Top + rect.Height / 2;
            var cmp  = Conver(new Point(x, y), _attached);
            var cvt  = Conver(pt, _attached);

            var n = AssociatedObject.Items.Count;
            if (src.DragIndex < index && cvt.X < cmp.X) return Math.Max(index - 1, 0);
            else if (src.DragIndex > index && cvt.X >= cmp.X) return Math.Min(index + 1, n);
            else return index;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetIndex
        ///
        /// <summary>
        /// Gets the item index located at the specified point.
        /// </summary>
        ///
        /// <remarks>
        /// 右端のマージンを考慮してインデックスを決定します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private int GetIndex(DragDropObject src, Point pt, Rect unit)
        {
            var obj = AssociatedObject;
            var dest = obj.GetIndex(pt);
            if (dest >= 0) return dest;

            // 最後の項目の右側
            if (pt.Y > unit.Bottom || (pt.X > unit.Right && pt.Y > unit.Top)) return obj.Items.Count;

            var w = obj.ActualWidth;
            var m = obj.GetItem(0)?.Margin.Right ?? 0;
            var x = (w - pt.X < unit.Width) ? (w - unit.Width) : (pt.X - m);
            return (x != pt.X) ? obj.GetIndex(new Point(x, pt.Y)) : dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetBounds
        ///
        /// <summary>
        /// Gets the bound of the first item.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private Rect GetBounds() =>
            AssociatedObject.Items.Count > 0 ?
            AssociatedObject.GetBounds(0) :
            new Rect();

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        ///
        /// <summary>
        /// Converts the specified point based on the specified control.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private Point Conver<T>(Point pt, T control) where T : UIElement =>
            control != null ?
            control.PointFromScreen(AssociatedObject.PointToScreen(pt)) :
            pt;

        #endregion

        #region Fields
        private Panel _attached;
        #endregion
    }
}
