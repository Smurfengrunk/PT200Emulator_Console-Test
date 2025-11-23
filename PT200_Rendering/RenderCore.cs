using System.Drawing;

using PT200_Parser;

namespace PT200_Rendering
{
    public class RenderCore
    {
        private RenderSnapshot[,] _lastFrame;
        private bool _initialized;

        private record struct RenderSnapshot(char RawChar, char OutChar, Color Fg, Color Bg)
        {
            public bool Equals(RenderSnapshot other)
            {
                return RawChar == other.RawChar &&
                       Fg == other.Fg &&
                       Bg == other.Bg;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(RawChar, Fg, Bg);
            }
        }

        public void ForceFullRender()
        {
            _initialized = false;
            _lastFrame = null;
        }

        public void Render(ScreenBuffer buffer, IRenderTarget target)
        {
            if (buffer.clearScreen)
            {
                _initialized = false;
                target.Clear();
                buffer.ScreenCleared();
            }

            if (_lastFrame == null || _lastFrame.GetLength(0) != buffer.Rows || _lastFrame.GetLength(1) != buffer.Cols)
            {
                _lastFrame = new RenderSnapshot[buffer.Rows, buffer.Cols];
                _initialized = false;
            }

            for (int row = 0; row < buffer.Rows; row++)
            {
                int col = 0;
                while (col < buffer.Cols)
                {
                    ScreenBuffer.ScreenCell cell = buffer.GetCell(row, col);
                    StyleInfo zone = buffer.ZoneAttributes[row, col];

                    StyleInfo.Color fg = zone?.Foreground ?? cell.Style.Foreground;
                    StyleInfo.Color bg = zone?.Background ?? cell.Style.Background;
                    bool reverse = zone?.ReverseVideo ?? cell.Style.ReverseVideo;
                    bool lowintensity = zone?.LowIntensity ?? cell.Style.LowIntensity;

                    if (reverse) (fg, bg) = (bg, fg);
                    if (lowintensity)
                    {
                        if (reverse) bg = bg.MakeDim();
                        else fg = fg.MakeDim();
                    }

                    char rawChar = cell.Char;
                    char outCh = cell.Char == '\0' ? ' ' : cell.Char;
                    RenderSnapshot snap = new RenderSnapshot(rawChar, outCh, TranslateColor(fg), TranslateColor(bg));

                    if (!_initialized || !_lastFrame[row, col].Equals(snap))
                    {
                        int runStart = col;
                        Color runFg = snap.Fg;
                        Color runBg = snap.Bg;
                        List<char> runChars = new List<char>();

                        while (col < buffer.Cols)
                        {
                            ScreenBuffer.ScreenCell c = buffer.GetCell(row, col);
                            StyleInfo z = buffer.ZoneAttributes[row, col];
                            StyleInfo.Color f = z?.Foreground ?? c.Style.Foreground;
                            StyleInfo.Color b = z?.Background ?? c.Style.Background;
                            bool r = z?.ReverseVideo ?? c.Style.ReverseVideo;
                            bool l = z?.LowIntensity ?? c.Style.LowIntensity;

                            if (r) (f, b) = (b, f);
                            if (l)
                            {
                                if (r) b = b.MakeDim();
                                else f = f.MakeDim();
                            }

                            char ch = c.Char == '\0' ? ' ' : c.Char;
                            RenderSnapshot s = new RenderSnapshot(rawChar, ch, TranslateColor(f), TranslateColor(b));

                            if (!_initialized && col == runStart) { }
                            else if (!s.Equals(snap)) break;

                            runChars.Add(ch);
                            if (_lastFrame != null) _lastFrame[row, col] = s;
                            col++;
                        }

                        target.DrawRun(new RenderRun(row, runStart, runFg, runBg, runChars.ToArray()));
                    }
                    else
                    {
                        col++;
                    }
                }
            }

            _initialized = true;
            buffer.forceRedraw = false;
            buffer.ClearDirty();
            target.SetCaret(buffer.CursorRow, buffer.CursorCol);
        }

        public static Color TranslateColor(StyleInfo.Color color) => color switch
        {
            StyleInfo.Color.Black => Color.Black,
            StyleInfo.Color.Green => Color.FromArgb(0, 255, 0),
            StyleInfo.Color.DarkGreen => Color.FromArgb(10, 15, 10),
            StyleInfo.Color.DarkCyan => Color.DarkCyan,
            StyleInfo.Color.DarkRed => Color.DarkRed,
            StyleInfo.Color.DarkMagenta => Color.DarkMagenta,
            StyleInfo.Color.Blue => Color.FromArgb(234, 242, 255),  // ljus blå text
            StyleInfo.Color.DarkBlue => Color.FromArgb(12, 12, 30),   // mörk blå bakgrund
            StyleInfo.Color.Cyan => Color.Cyan,
            StyleInfo.Color.Red => Color.Red,
            StyleInfo.Color.Magenta => Color.Magenta,
            StyleInfo.Color.DarkYellow => Color.FromArgb(26, 18, 8), // bakgrundston
            StyleInfo.Color.Yellow => Color.FromArgb(255, 191, 0),   // text amber
            StyleInfo.Color.Gray => Color.FromArgb(24, 24, 24),      // bakgrund
            StyleInfo.Color.White => Color.White,                    // text
            _ => Color.Wheat
        };
    }
}