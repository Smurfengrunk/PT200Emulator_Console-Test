using System.Drawing;

namespace PT200_Rendering
{
    public record RenderRun(int Row, int StartCol, Color Fg, Color Bg, ReadOnlyMemory<char> Chars);

    public interface IRenderTarget
    {
        public enum CursorStyle
        {
            Block,
            HorizontalBar,
            VerticalBar
        }

        void Clear();
        void DrawRun(RenderRun run);
        void SetCaret(int row, int col);
    }
}