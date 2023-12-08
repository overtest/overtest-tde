using System.Diagnostics.CodeAnalysis;
using Adw;

namespace Sirkadirov.Overtest.Apps.TDE.Panels;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal static class ProjectEditor
{
    public static ViewSwitcher ViewSwitcherControl { get; private set; }
    public static ViewStack ViewStackControl { get; private set; }
    public static string ProjectPath { get; private set; }

    private static GeneralPreferencesPage _generalPreferencesPage;

    public static void Initialize()
    {
        ViewSwitcherControl = new ViewSwitcher();
        ViewStackControl = new ViewStack();
        ViewSwitcherControl.SetStack(ViewStackControl);
        ViewSwitcherControl.SetPolicy(ViewSwitcherPolicy.Wide);
        ProjectPath = null;
        BuildUi();
    }

    public static void OpenProject(string path)
    {
        ProjectPath = path;
        ViewSwitcherControl.SetVisible(true);
        ViewStackControl.SetVisible(true);
        _generalPreferencesPage.SetCurrentProject(path);
    }

    public static void CloseProject()
    {
        ProjectPath = null;
        ViewSwitcherControl.SetVisible(false);
        ViewStackControl.SetVisible(false);
    }

    public static void SaveProject()
    {
        _generalPreferencesPage.SaveCurrentProject();
    }

    private static void BuildUi()
    {
        _generalPreferencesPage = new GeneralPreferencesPage();
        ViewStackControl.AddTitledWithIcon(_generalPreferencesPage.Control, "general", "General info", "dialog-information");
        ViewStackControl.AddTitledWithIcon(new PreferencesPage(), "processing", "Processing config", "preferences-system");
    }
}