using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Adw;
using Sirkadirov.Overtest.Apps.TDE.Panels;

namespace Sirkadirov.Overtest.Apps.TDE.Windows;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal class MainWindow : ApplicationWindow
{
    private bool _isProjectOpened;
    private Gtk.Button _createProjectButton;
    private Gtk.Button _openProjectButton;
    private Gtk.Button _saveProjectButton;
    private Gtk.Button _openProjectFolderButton;
    private Gtk.Button _exportProjectButton;
    private Gtk.Button _closeProjectButton;

    private WindowTitle _windowTitle;
    private ToolbarView _mainToolbarView;
    private StatusPage _noProjectOpenedPage;
    private ToastOverlay _toastOverlay;

    public MainWindow(Application application)
    {
        Application = application;
        BuildUi();
    }

    private void BuildUi()
    {
        ProjectEditor.Initialize();
        Title = "Overtest TDE";
        SetDefaultSize(800, 600);
        SetSizeRequest(800, 600);

        _mainToolbarView = GetToolbarView();
        SetContent(_mainToolbarView);

        ToggleProjectOpenedState(setToDefaultState: true);
        return;

        ToolbarView GetToolbarView()
        {
            var toolbarView = new ToolbarView();
            toolbarView.AddTopBar(GetHeaderBar());

            var contentBox = new Gtk.Box();
            contentBox.Homogeneous = true;
            contentBox.Append(_noProjectOpenedPage = new StatusPage
            {
                Title = "No project opened",
                Description = "Create a new one or open an existing project to start working",
                IconName = "folder-open"
            });
            contentBox.Append(ProjectEditor.ViewStackControl);

            _toastOverlay = new ToastOverlay();
            _toastOverlay.SetChild(contentBox);
            toolbarView.SetContent(_toastOverlay);

            return toolbarView;
        }

        HeaderBar GetHeaderBar()
        {
            var headerBar = new HeaderBar();

            var titleBox = new Gtk.Box();
            titleBox.Append(_windowTitle = new WindowTitle
            {
                Title = "Overtest Tasks Development Environment",
                Subtitle = "Create tasks for teaching your students with ease"
            });
            titleBox.Append(ProjectEditor.ViewSwitcherControl);
            headerBar.SetTitleWidget(titleBox);

            var aboutButton = new Gtk.Button { IconName = "help-about", TooltipText = "About program & support" };
            aboutButton.OnClicked += (_, _) => new AboutWindow(this).Show();
            headerBar.PackEnd(aboutButton);

            headerBar.PackStart(_createProjectButton = new Gtk.Button { Child = new ButtonContent
            {
                IconName = "document-new",
                Label = "New project",
                TooltipText = "Create a new project"
            }});

            headerBar.PackStart(_openProjectButton = new Gtk.Button { Child = new ButtonContent
            {
                IconName = "folder-open",
                Label = "Open",
                TooltipText = "Open existing project"
            }});
            _openProjectButton.OnClicked += async (_, _) => await OnOpenProjectActionRequested();

            headerBar.PackStart(_saveProjectButton = new Gtk.Button { Child = new ButtonContent
            {
                IconName = "document-save",
                Label = "Save",
                TooltipText = "Save project"
            }});
            _saveProjectButton.OnClicked += (_, _) => OnSaveProjectActionRequested();

            headerBar.PackStart(_openProjectFolderButton = new Gtk.Button
            {
                IconName = "folder-open",
                TooltipText = "Open project's folder"
            });
            _openProjectFolderButton.OnClicked += (_, _) =>
            {
                if (ProjectEditor.ProjectPath is null)
                    return;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Process.Start("explorer.exe", ProjectEditor.ProjectPath);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    throw new NotImplementedException();
            };

            headerBar.PackStart(_exportProjectButton = new Gtk.Button
            {
                IconName = "document-send",
                TooltipText = "Export project"
            });

            headerBar.PackStart(_closeProjectButton = new Gtk.Button
            {
                IconName = "application-exit",
                TooltipText = "Close current project"
            });
            _closeProjectButton.OnClicked += (_, _) => OnCloseProjectActionRequested();

            return headerBar;
        }
    }

    private void ToggleProjectOpenedState(string projectFullPath = null, bool setToDefaultState = false)
    {
        _isProjectOpened = !setToDefaultState && !_isProjectOpened;

        _createProjectButton.Visible = _openProjectButton.Visible = !_isProjectOpened;
        _saveProjectButton.Visible = _openProjectFolderButton.Visible = _exportProjectButton.Visible
            = _closeProjectButton.Visible = _isProjectOpened;

        _windowTitle.SetVisible(!_isProjectOpened);
        _noProjectOpenedPage.SetVisible(!_isProjectOpened);

        if (!_isProjectOpened)
        {
            if (projectFullPath is not null)
                throw new ArgumentOutOfRangeException();
            ProjectEditor.CloseProject();
        }
        else
        {
            ArgumentNullException.ThrowIfNull(projectFullPath);
            ProjectEditor.OpenProject(projectFullPath);
        }
    }

    private async Task OnOpenProjectActionRequested()
    {
        using var openFolderDialog = Gtk.FileDialog.New();
        openFolderDialog.SetTitle("Open project folder");
        openFolderDialog.SetModal(true);
        Gio.File folder;
        try
        {
            folder = await openFolderDialog.SelectFolderAsync(this);
        }
        // Catch dismiss exception
        catch (Exception) { return; }
        if (folder is null) return;
        ToggleProjectOpenedState(folder.GetPath());
    }

    private void OnCloseProjectActionRequested()
    {
        var confirmationDialog = new MessageDialog();
        confirmationDialog.SetTransientFor(this);
        confirmationDialog.SetHeading("Close project?");
        confirmationDialog.SetBody("Are you sure you want to close current project? All unsaved changes will be lost.");
        confirmationDialog.AddResponse("cancel", "Cancel");
        confirmationDialog.AddResponse("close", "Close project");
        confirmationDialog.SetResponseAppearance("close", ResponseAppearance.Destructive);
        confirmationDialog.SetDefaultResponse("cancel");
        confirmationDialog.OnResponse += (_, response) =>
        {
            if (response.Response == "close")
                ToggleProjectOpenedState();
        };
        confirmationDialog.Show();
    }

    private void OnSaveProjectActionRequested()
    {
        ProjectEditor.SaveProject();
        _toastOverlay.AddToast(Toast.New("All changes have been saved successfully!"));
    }
}