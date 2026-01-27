using System.Drawing;

using PT200_Parser;

namespace PT200_Rendering
{
    public class RenderCore
    {
        private RenderSnapshot[,] _lastFrame;
        private bool _initialized;

        public readonly struct RenderSnapshot : IEquatable<RenderSnapshot>
        {
            public readonly char RawChar;
            public readonly char OutChar;
            public readonly Color Fg;
            public readonly Color Bg;

            public RenderSnapshot(char rawChar, char outChar, Color fg, Color bg)
            {
                RawChar = rawChar;
                OutChar = outChar;
                Fg = fg;
                Bg = bg;
            }

            public bool Equals(RenderSnapshot other) =>
                RawChar == other.RawChar &&
                OutChar == other.OutChar &&
                Fg == other.Fg &&
                Bg == other.Bg;

            public override bool Equals(object obj) =>
                obj is RenderSnapshot other && Equals(other);

            public override int GetHashCode() =>
                HashCode.Combine(RawChar, OutChar, Fg, Bg);

            public static bool operator ==(RenderSnapshot left, RenderSnapshot right) => left.Equals(right);
            public static bool operator !=(RenderSnapshot left, RenderSnapshot right) => !left.Equals(right);
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

            // Förallokera en buffer per rad (återanvänds varje iteration)
            char[] runBuffer = new char[buffer.Cols];

            for (int row = 0; row < buffer.Rows; row++)
            {
                int col = 0;
                while (col < buffer.Cols)
                {
                    var cell = buffer.GetCell(row, col);
                    var zone = buffer.ZoneAttributes[row, col];

                    // Beräkna attribut en gång
                    var fg = zone?.Foreground ?? cell.Style.Foreground;
                    var bg = zone?.Background ?? cell.Style.Background;
                    bool reverse = zone?.ReverseVideo ?? cell.Style.ReverseVideo;
                    bool lowintensity = zone?.LowIntensity ?? cell.Style.LowIntensity;

                    if (reverse) (fg, bg) = (bg, fg);
                    if (lowintensity)
                    {
                        if (reverse) bg = bg.MakeDim();
                        else fg = fg.MakeDim();
                    }

                    char rawChar = cell.Char;
                    char outCh = rawChar == '\0' ? ' ' : rawChar;

                    TranslateColor.TryGetValue(fg, out var cfg);
                    TranslateColor.TryGetValue(bg, out var cbg);

                    var snap = new RenderSnapshot(rawChar, outCh, cfg, cbg);

                    if (!_initialized || !_lastFrame[row, col].Equals(snap))
                    {
                        int runStart = col;
                        Color runFg = snap.Fg;
                        Color runBg = snap.Bg;
                        int runLength = 0;

                        // Bygg run direkt i char[] buffer
                        while (col < buffer.Cols)
                        {
                            var nextCell = buffer.GetCell(row, col);
                            var nextZone = buffer.ZoneAttributes[row, col];

                            var nf = nextZone?.Foreground ?? nextCell.Style.Foreground;
                            var nb = nextZone?.Background ?? nextCell.Style.Background;
                            bool nr = nextZone?.ReverseVideo ?? nextCell.Style.ReverseVideo;
                            bool nl = nextZone?.LowIntensity ?? nextCell.Style.LowIntensity;

                            if (nr) (nf, nb) = (nb, nf);
                            if (nl)
                            {
                                if (nr) nb = nb.MakeDim();
                                else nf = nf.MakeDim();
                            }

                            char nch = nextCell.Char == '\0' ? ' ' : nextCell.Char;
                            TranslateColor.TryGetValue(nf, out var ncf);
                            TranslateColor.TryGetValue(nb, out var ncb);

                            var s = new RenderSnapshot(nextCell.Char, nch, ncf, ncb);

                            if (!_initialized && col == runStart) { }
                            else if (!s.Equals(snap)) break;

                            runBuffer[runLength++] = nch;
                            _lastFrame[row, col] = s;
                            col++;
                        }

                        // Skicka run som Span istället för ToArray()
                        target.DrawRun(new RenderRun(row, runStart, runFg, runBg, runBuffer.AsMemory(0, runLength)));
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

        public static readonly Dictionary<StyleInfo.Color, Color> TranslateColor = new()
        {
            { StyleInfo.Color.Black, Color.Black },
            { StyleInfo.Color.Green, Color.FromArgb(0, 255, 0) },
            { StyleInfo.Color.DarkGreen, Color.FromArgb(10, 15, 10) },
            { StyleInfo.Color.DarkCyan, Color.DarkCyan },
            { StyleInfo.Color.DarkRed, Color.DarkRed },
            { StyleInfo.Color.DarkMagenta, Color.DarkMagenta },
            { StyleInfo.Color.Blue, Color.FromArgb(234, 242, 255) },  // ljus blå text
            { StyleInfo.Color.DarkBlue, Color.FromArgb(12, 12, 30) },   // mörk blå bakgrund
            { StyleInfo.Color.Cyan, Color.Cyan },
            { StyleInfo.Color.Red, Color.Red },
            { StyleInfo.Color.Magenta, Color.Magenta },
            { StyleInfo.Color.DarkYellow, Color.FromArgb(26, 18, 8) }, // bakgrundston
            { StyleInfo.Color.Yellow, Color.FromArgb(255, 191, 0) },   // text amber
            { StyleInfo.Color.Gray, Color.FromArgb(24, 24, 24) },      // bakgrund
            { StyleInfo.Color.White, Color.White },                    // text
            { StyleInfo.Color.Default, Color.Wheat }
        };
    }
}