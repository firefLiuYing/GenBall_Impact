using System;
using System.Collections.Generic;
using Yueyn.Main;

namespace GenBall.GM
{
    public interface IGMCommandSystem : ISystem
    {
        bool IsDevMode { get; }
        void RegisterCommand(string name, Action<string[]> handler, string description);
        string ExecuteCommand(string input);
        void ToggleConsole();
        void NotifyConsoleClosed();
        IEnumerable<(string name, string description)> GetCommands();
    }
}
