﻿using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using System;
using System.Threading.Tasks;
using TailwindCSSIntellisense.Build;
using TailwindCSSIntellisense.Configuration;
using TailwindCSSIntellisense.Options;
using TailwindCSSIntellisense.Settings;

namespace TailwindCSSIntellisense
{
    [Command(PackageGuids.guidVSPackageCmdSetString, PackageIds.StopBuildProcessCmdId)]
    internal sealed class StopBuildProcess : BaseCommand<StopBuildProcess>
    {
        protected override async Task InitializeCompletedAsync()
        {
            BuildProcess = await VS.GetMefServiceAsync<TailwindBuildProcess>();
            ConfigFileScanner = await VS.GetMefServiceAsync<ConfigFileScanner>();
            SettingsProvider = await VS.GetMefServiceAsync<SettingsProvider>();
        }

        internal TailwindBuildProcess BuildProcess { get; set; }
        internal ConfigFileScanner ConfigFileScanner { get; set; }
        internal SettingsProvider SettingsProvider { get; set; }

        protected override void BeforeQueryStatus(EventArgs e)
        {
            var settings = ThreadHelper.JoinableTaskFactory.Run(SettingsProvider.GetSettingsAsync);
            Command.Visible = settings.EnableTailwindCss && BuildProcess.AreProcessesActive() && ConfigFileScanner.HasConfigurationFile && settings.BuildType != BuildProcessOptions.None;
            switch (settings.BuildType)
            {
                case BuildProcessOptions.Default:
                    Command.Text = "Stop Tailwind CSS JIT build process";
                    break;
                case BuildProcessOptions.OnBuild:
                    Command.Text = "Cancel Tailwind CSS build";
                    break;
                default:
                    Command.Text = "Stop Tailwind CSS build process";
                    break;
            }
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await BuildProcess.InitializeAsync();

            BuildProcess.EndProcess();
        }
    }
}
