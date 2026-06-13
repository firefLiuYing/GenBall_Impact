using System;
using System.Collections.Generic;
using System.Text;
using GenBall.Framework.Config;
using GenBall.Map;
using GenBall.Procedure.Execute;
using GenBall.UI;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Main;
using Yueyn.UI;

namespace GenBall.GM
{
    public class GMCommandSystemDefault : IGMCommandSystem, IFrameUpdate
    {
        private readonly Dictionary<string, (Action<string[]> handler, string description)> _commands = new();
        private string _lastCommandOutput;
        private bool _isDevMode;
        private bool _isConsoleOpen;
        private int _consoleLogicId;

        public bool IsDevMode => _isDevMode;
        public SystemScope FrameUpdateScope => SystemScope.Game;

        public void Init()
        {
            _isDevMode = false;

            var configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
            if (configProvider != null)
            {
                var config = configProvider.GetConfig<AppSettingsConfig>();
                if (config != null)
                {
                    _isDevMode = config.devMode;
                }
            }

            RegisterBuiltInCommands();
        }

        public void UnInit()
        {
            if (_isConsoleOpen)
            {
                BusinessLogicManager.Instance.DestroyLogic(_consoleLogicId);
                _isConsoleOpen = false;
            }

            _commands.Clear();
        }

        private void RegisterBuiltInCommands()
        {
            RegisterCommand("help", args =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("Available commands:");
                foreach (var kvp in _commands)
                {
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value.description}");
                }
                _lastCommandOutput = sb.ToString().TrimEnd();
            }, "Show all available commands");

            RegisterCommand("load_scene", args =>
            {
                if (args.Length == 0)
                {
                    _lastCommandOutput = "Usage: load_scene <sceneName> [savePointIndex]";
                    return;
                }

                var sceneName = args[0];

                var sceneLoadSystem = SystemRepository.Instance.GetSystem<ISceneLoadSystem>();
                if (sceneLoadSystem == null)
                {
                    _lastCommandOutput = "Error: ISceneLoadSystem not registered.";
                    return;
                }

                sceneLoadSystem.LoadScene(sceneName);
                _lastCommandOutput = $"Loading {sceneName}...";
            }, "Load a scene by name. Usage: load_scene <sceneName> [savePointIndex]");

            RegisterCommand("skip_loading", args =>
            {
                var launchSystem = SystemRepository.Instance.GetSystem<ILaunchSystem>();
                if (launchSystem == null)
                {
                    _lastCommandOutput = "Error: ILaunchSystem not registered.";
                    return;
                }

                launchSystem.SkipStartupLoading();
                _lastCommandOutput = "Startup loading skipped.";
            }, "Skip the startup loading screen");

            RegisterCommand("list_scenes", args =>
            {
                _lastCommandOutput = "Scenes: Prologue, Episode1, SunnyStrikeScene, SSTest2";
            }, "List all available scenes");
        }

        public void RegisterCommand(string name, Action<string[]> handler, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogWarning("[GMCommandSystem] Cannot register command with empty name.");
                return;
            }

            var lowerName = name.ToLowerInvariant();
            _commands[lowerName] = (handler, description);
        }

        public string ExecuteCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            var trimmed = input.Trim();
            var spaceIndex = trimmed.IndexOf(' ');
            string commandName;
            string[] args;

            if (spaceIndex < 0)
            {
                commandName = trimmed;
                args = Array.Empty<string>();
            }
            else
            {
                commandName = trimmed.Substring(0, spaceIndex);
                var argsString = trimmed.Substring(spaceIndex + 1);
                args = argsString.Split(' ');
            }

            var lowerName = commandName.ToLowerInvariant();
            _lastCommandOutput = "";

            if (_commands.TryGetValue(lowerName, out var entry))
            {
                try
                {
                    entry.handler(args);
                }
                catch (Exception e)
                {
                    _lastCommandOutput = $"Command error: {e.Message}";
                    Debug.LogError($"[GMCommandSystem] Error executing command '{commandName}': {e}");
                }

                return _lastCommandOutput;
                
            }

            return $"Unknown command: {commandName}";
        }

        public void ToggleConsole()
        {
            if (_isConsoleOpen)
            {
                // Set false before destroying to prevent OnFormUnbound from
                // re-entering ToggleConsole and creating a new console
                _isConsoleOpen = false;
                BusinessLogicManager.Instance.DestroyLogic(_consoleLogicId);
            }
            else
            {
                var logic = BusinessLogicManager.Instance.CreateLogic<GMConsoleFormLogic>(
                    l => l.InitWithSystem(this));
                _consoleLogicId = logic.LogicId;
                _isConsoleOpen = true;
            }
        }

        public void NotifyConsoleClosed()
        {
            _isConsoleOpen = false;
        }

        public IEnumerable<(string name, string description)> GetCommands()
        {
            foreach (var kvp in _commands)
            {
                yield return (kvp.Key, kvp.Value.description);
            }
        }

        public void FrameUpdate(float deltaTime)
        {
            if (!_isDevMode)
                return;

            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                ToggleConsole();
            }
        }
    }
}
