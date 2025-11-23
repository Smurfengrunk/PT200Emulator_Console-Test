using System.Text;

using PT200_Logging;

namespace PT200_Parser
{

    public class DcsSequenceHandler
    {
        //public event Action<string, IReadOnlyList<TerminalAction>> ActionsReady;
        public event Action<byte[]> OnDcsResponse;
        public event Action<string> OnStatusUpdate;

        private readonly TerminalState state;
        private readonly string jsonPath;

        public DcsSequenceHandler(TerminalState state, string jsonPath)
        {
            this.state = state;
            this.jsonPath = jsonPath;
        }

        public void Handle(byte[] payload)
        {
            RaiseStatus("🟡 Väntar på DCS");
            if (payload.Length == 0)
            {
                RaiseStatus("🟡 Väntar på DCS");
                string dcs = state.BuildDcs(this.jsonPath);
                SendDcsResponse(dcs);
                return;
            }

            string content = Encoding.ASCII.GetString(payload);

            state.ReadDcs(this.jsonPath, content);
            IReadOnlyList<TerminalAction> actions = DcsSequenceHandler.Build(content);
            state.DisplayDCS();
        }

        private void SendDcsResponse(string dcs)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(dcs);
            this.LogTrace($"[DCS] Using handler hash={this.GetHashCode()}");
            OnDcsResponse?.Invoke(bytes);
        }
        public static IReadOnlyList<TerminalAction> Build(string content)
        {
            List<TerminalAction> actions = new List<TerminalAction>();

            if (content.Contains("BLOCK", StringComparison.OrdinalIgnoreCase))
                actions.Add(new SetModeAction("BLOCK"));
            else if (content.Contains("LINE", StringComparison.OrdinalIgnoreCase))
                actions.Add(new SetModeAction("LINE"));

            actions.Add(new DcsAction(content));
            return actions;
        }
        private void RaiseStatus(string message)
        {
            OnStatusUpdate?.Invoke(message);
        }
    }
}
