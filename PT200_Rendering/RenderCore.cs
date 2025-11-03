using PT200_Parser;
using PT200_Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT200_Rendering
{
    public class RenderCore
    {
        private RenderSnapshot[,] _lastFrame;
        private bool _initialized;

        private record struct RenderSnapshot(char Char, ConsoleColor Fg, ConsoleColor Bg);

        public void ForceFullRender()
        {
            _initialized = false;
            _lastFrame = null;
        }

        public void Render(ScreenBuffer buffer, IRenderTarget target)
        {
            IRenderTarget.CursorStyle style = IRenderTarget.CursorStyle.Block;
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
                    var cell = buffer.GetCell(row, col);
                    var zone = buffer.ZoneAttributes[row, col];

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

                    var outCh = cell.Char == '\0' ? ' ' : cell.Char;
                    var snap = new RenderSnapshot(outCh, MapToConsoleColor(fg), MapToConsoleColor(bg));

                    if (!_initialized || !_lastFrame[row, col].Equals(snap))
                    {
                        int runStart = col;
                        var runFg = snap.Fg;
                        var runBg = snap.Bg;
                        var runChars = new List<char>();

                        while (col < buffer.Cols)
                        {
                            var c = buffer.GetCell(row, col);
                            var z = buffer.ZoneAttributes[row, col];
                            var f = z?.Foreground ?? c.Style.Foreground;
                            var b = z?.Background ?? c.Style.Background;
                            var r = z?.ReverseVideo ?? c.Style.ReverseVideo;
                            var l = z?.LowIntensity ?? c.Style.LowIntensity;

                            if (r) (f, b) = (b, f);
                            if (l)
                            {
                                if (r) b = b.MakeDim();
                                else f = f.MakeDim();
                            }

                            var ch = c.Char == '\0' ? ' ' : c.Char;
                            var s = new RenderSnapshot(ch, MapToConsoleColor(f), MapToConsoleColor(b));

                            if (!_initialized && col == runStart) { }
                            else if (!s.Equals(snap)) break;

                            runChars.Add(ch);
                            _lastFrame[row, col] = s;
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

            target.SetCaret(buffer.CursorRow, buffer.CursorCol, true, style);
        }

        private static ConsoleColor MapToConsoleColor(StyleInfo.Color color)
        {
            if (color.Equals(StyleInfo.Color.Black) || color.Equals(StyleInfo.Color.Black_Low)) return ConsoleColor.Black;
            if (color.Equals(StyleInfo.Color.White)) return ConsoleColor.White;
            if (color.Equals(StyleInfo.Color.White_Low)) return ConsoleColor.Gray;
            if (color.Equals(StyleInfo.Color.Green)) return ConsoleColor.Green;
            if (color.Equals(StyleInfo.Color.Green_Low)) return ConsoleColor.DarkGreen;
            if (color.Equals(StyleInfo.Color.DarkYellow)) return ConsoleColor.DarkYellow;
            if (color.Equals(StyleInfo.Color.DarkYellow_Low)) return ConsoleColor.Yellow;
            if (color.Equals(StyleInfo.Color.Blue)) return ConsoleColor.Blue;
            if (color.Equals(StyleInfo.Color.Blue_Low)) return ConsoleColor.DarkBlue;
            return ConsoleColor.White;
        }
    }
}