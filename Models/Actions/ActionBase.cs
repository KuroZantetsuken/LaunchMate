﻿using Playnite.SDK;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LaunchMate.Models
{
    public abstract class ActionBase : ObservableObject, IAction
    {
        private readonly ILogger logger = LogManager.GetLogger();
        private string _target;
        private string _args;
        private bool _useWebView = true;
        private Screen _openScreen;

        public string Target { get => _target; set => SetValue(ref _target, value); }
        public string TargetArgs { get => _args; set => SetValue(ref _args, value); }
        public bool UseWebView { get => _useWebView; set => SetValue(ref _useWebView, value); }
        public Screen OpenScreen { get => _openScreen; set => SetValue(ref _openScreen, value); }

        /// <summary>
        /// Executes the action
        /// </summary>
        /// <param name="groupName">The name of the group executing the action</param>
        /// <returns>true if the execution was successful, false otherwise</returns>
        public abstract bool Execute(string groupName, Screen screen = null, Playnite.SDK.Events.OnGameStartingEventArgs args = null);

        /// <summary>
        /// Automatically closes something launched by <see cref="Execute(string)", if applicable/>
        /// </summary>
        /// <param name="groupName">The name of the group calling AutoClose</param>
        public virtual void AutoClose(string groupName)
        {
            logger.Warn($"{groupName} - AutoClose not supported for this action type");
        }

    }
}
