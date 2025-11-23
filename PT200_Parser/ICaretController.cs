namespace PT200_Parser
{
    public interface ICaretController
    {
        void SetCaretPosition(int row, int col);
        void MoveCaret(int dRow, int dCol);
        void Show();
        void Hide();
    }
}
