using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using EnvDTE;

namespace ColorizeIndent
{
    /// <summary>
    /// ColorizeIndent places set back color indent " "s in the editor window
    /// </summary>
    internal sealed class ColorizeIndent
    {
        /// <summary>
        /// The layer of the adornment.
        /// </summary>
        private readonly IAdornmentLayer layer;

        /// <summary>
        /// Text view where the adornment is created.
        /// </summary>
        private readonly IWpfTextView view;

        private Brush errorBrush;

        /// <summary>
        /// Adornment brush.
        /// </summary>
        private Brush[] brush;

        /// <summary>
        /// Adornment pen.
        /// </summary>
        private readonly Pen pen;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorizeIndent"/> class.
        /// </summary>
        /// <param name="view">Text view to create the adornment for</param>
        public ColorizeIndent(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            this.layer = view.GetAdornmentLayer("ColorizeIndent");

            this.view = view;
            this.view.LayoutChanged += this.OnLayoutChanged;

            // TODO Refresh on configuration changed

            var penBrush = new SolidColorBrush(Colors.Transparent);
            penBrush.Freeze();
            this.pen = new Pen(penBrush, 0.5);
            this.pen.Freeze();
        }

        /// <summary>
        /// Handles whenever the text displayed in the view changes by adding the adornment to any reformatted lines
        /// </summary>
        /// <remarks><para>This event is raised whenever the rendered text displayed in the <see cref="ITextView"/> changes.</para>
        /// <para>It is raised whenever the view does a layout (which happens when DisplayTextLineContainingBufferPosition is called or in response to text or classification changes).</para>
        /// <para>It is also raised whenever the view scrolls horizontally or when its size changes.</para>
        /// </remarks>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            // TODO Add ignore file type
            var list    = this.view.Properties.PropertyList;
            var options = this.view.Options;
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                this.CreateVisuals(line);
            }
        }

        private void CreateVisuals(ITextViewLine line)
        {
            IWpfTextViewLineCollection textViewLines = this.view.TextViewLines;

            this.MakeBrushes();

            // TODO Add target char configuration
            // TODO Get indent size from current file setting
            var indents = new char[] { ' ', '\t' };
            int indentSize = Option.Instance.IndentSize;
            bool isError = false;

            for (int charIndex = line.Start; charIndex < line.End; charIndex++)
            {
                if (indents.All(i => this.view.TextSnapshot[charIndex] != i))
                {
                    isError = (charIndex - line.Start) % indentSize != 0;
                    break;
                }
            }

            // TODO Add ignore pattern regex
            for (int charIndex = line.Start; charIndex < line.End; charIndex++)
            {
                if (indents.All(i => this.view.TextSnapshot[charIndex] != i))
                {
                    break;
                }

                SnapshotSpan span = new SnapshotSpan(this.view.TextSnapshot, Span.FromBounds(charIndex, charIndex + 1));
                Geometry geometry = textViewLines.GetMarkerGeometry(span);
                if (geometry != null)
                {
                    var brush = isError ? this.errorBrush : this.brush[(charIndex - line.Start) / indentSize % this.brush.Count()];
                    var drawing = new GeometryDrawing(brush, this.pen, geometry);
                    drawing.Freeze();

                    var drawingImage = new DrawingImage(drawing);
                    drawingImage.Freeze();

                    var image = new Image
                    {
                        Source = drawingImage,
                    };

                    // Align the image with the top of the bounds of the text geometry
                    Canvas.SetLeft(image, geometry.Bounds.Left);
                    Canvas.SetTop(image, geometry.Bounds.Top);

                    this.layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
                }
            }
        }

        private void MakeBrushes()
        {
            var colors = Option.Instance.Colors;

            this.brush = new Brush[colors.Count()];
            for (int i = 0; i < this.brush.Count(); i++)
            {
                this.brush[i] = new SolidColorBrush(this.ConvertColor(colors[i]));
                this.brush[i].Freeze();
            }

            this.errorBrush = new SolidColorBrush(this.ConvertColor(Option.Instance.ErrorColor));
            this.errorBrush.Freeze();
        }

        private Color ConvertColor(System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
