using Gtk;

namespace Sirkadirov.Overtest.Apps.TDE.Windows;

internal class AboutWindow : IAppWindow
{
    private readonly Adw.AboutWindow _aboutWindow;

    public AboutWindow(Window parent)
    {
        _aboutWindow = new Adw.AboutWindow();
        _aboutWindow.SetParent(parent);
        _aboutWindow.SetTransientFor(parent);
        BuildUi();
    }

    private void BuildUi()
    {
        _aboutWindow.ApplicationIcon = "utilities-terminal";
        _aboutWindow.ApplicationName = "Overtest TDE";
        _aboutWindow.Comments = "Using Overtest Tasks Development Environment you " +
                                "can create tasks for teaching your students and " +
                                "holding programming competitions with ease.";
        _aboutWindow.LicenseType = License.Gpl30;
        _aboutWindow.Version = "v0.1.0-alpha";
        _aboutWindow.Website = "https://github.com/overtest/overtest-tde";
        _aboutWindow.SupportUrl = "https://sirkadirov.com/contact";
        _aboutWindow.IssueUrl = "https://github.com/overtest/overtest-tde/issues";
        _aboutWindow.DeveloperName = "Developed by Yurii Kadirov (aka Sirkadirov)";
        _aboutWindow.Copyright = "Copyright © 2023 Yurii Kadirov (aka Sirkadirov). All rights reserved.";
        _aboutWindow.Developers = new[] {"Yurii Kadirov (aka Sirkadirov) <contact@sirkadirov.com>"};
        _aboutWindow.TranslatorCredits = "NGO SUPREME ORDER https://supremeorder.rocks/";
        _aboutWindow.AddLink("GitHub repository", "https://github.com/overtest");
    }

    public void Show() => _aboutWindow.Show();
}