namespace Asteroids
{
    internal class SizableControl<T>(PointF position, SizeF size, float fontSize) where T : Control , new()
    {
        // private attributes
        private readonly T control = new();
        private PointF position    = position;
        private SizeF size         = size;
        private float scale        = 0f;
        private float fontSize     = fontSize;

        // public attribute lambdas
        public PointF Position      { get { return position; }  set { position = value; } }
        public SizeF Size           { get { return size; }      set { size = value; } }
        public float Scale          { get { return scale; }     set { scale = value; } }
        public float FontSize       { get { return fontSize; }  set { fontSize = value; } }
        public RectangleF Rectangle { get { return new(position, size); } }
        public T Control            { get { return control; } }

        public void Update(PointF AdditionalOffset, float scale = 1)
        {
            this.scale = scale;
            control.Location = new Point((int)(position.X * this.scale + AdditionalOffset.X), (int)(position.Y  * this.scale + AdditionalOffset.Y));
            control.Size     = new Size ((int)(size.Width * this.scale), (int)(size.Height * this.scale));
            control.Font     = new Font(GameForm.PublicFonts?.Families[0]!, fontSize * scale, control.Font.Style);
        }
    }
}
