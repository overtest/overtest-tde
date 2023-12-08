using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Adw;
using Sirkadirov.Overtest.Common.TaskModels;

namespace Sirkadirov.Overtest.Apps.TDE.Panels;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class GeneralPreferencesPage
{
    public PreferencesPage Control { get; }

    private string _projectPath;
    private string _editedFilePath;
    private string _descriptionFilePath;

    private EntryRow _projectPathEntryRow; //readonly

    private EntryRow _projectIdEntryRow;
    private EntryRow _projectTitleEntryRow;
    private EntryRow _projectAuthorEntryRow;
    private SpinRow _projectDifficultySpinRow;

    private ProgrammingTaskModel _programmingTaskModel;

    public GeneralPreferencesPage()
    {
        Control = new PreferencesPage();
        BuildUi();
    }

    public void SetCurrentProject(string path)
    {
        _projectPath = path;
        _projectPathEntryRow.Text_ = path;
        _editedFilePath = Path.Combine(_projectPath, DefaultOvertestTaskStructure.ProjectFileName);
        _descriptionFilePath = Path.Combine(_projectPath, DefaultOvertestTaskStructure.TaskDescriptionFileName);

        var projectJson = File.ReadAllText(_editedFilePath, Encoding.UTF8);
        _programmingTaskModel = JsonSerializer.Deserialize<ProgrammingTaskModel>(projectJson);

        _projectIdEntryRow.SetText(_programmingTaskModel.Id.ToString());
        _projectTitleEntryRow.SetText(_programmingTaskModel.Title ?? "");
        _projectAuthorEntryRow.SetText(_programmingTaskModel.Author ?? "");
        _projectDifficultySpinRow.Adjustment!.SetValue(_programmingTaskModel.Difficulty);
    }

    public void SaveCurrentProject()
    {
        _programmingTaskModel.Id = new Guid(_projectIdEntryRow.GetText());
        _programmingTaskModel.Title = _projectTitleEntryRow.GetText();
        _programmingTaskModel.Author = _projectAuthorEntryRow.GetText();
        _programmingTaskModel.Difficulty = (int)_projectDifficultySpinRow.Adjustment!.Value;

        var serializerOptions = new JsonSerializerOptions
        { WriteIndented = true, NumberHandling = JsonNumberHandling.Strict };
        var projectJson = JsonSerializer.Serialize(_programmingTaskModel, serializerOptions);
        File.WriteAllText(_editedFilePath, projectJson, Encoding.UTF8);
    }

    private void BuildUi()
    {
        var regenerateGuidButton = new Gtk.Button
        { Child = new ButtonContent {
            IconName = "view-refresh",
            Label = "New GUID"
        }};
        regenerateGuidButton.OnClicked += (_, _) => _projectIdEntryRow.SetText(Guid.NewGuid().ToString());

        var projectInfoPreferencesGroup = new PreferencesGroup
        {
            Title = "Project info",
            Description = "General information about the project",
            HeaderSuffix = regenerateGuidButton
        };
        projectInfoPreferencesGroup.Add(_projectIdEntryRow = new EntryRow { Title = "Unique task identifier" });
        projectInfoPreferencesGroup.Add(_projectPathEntryRow = new EntryRow
            { Title = "Project path", Editable = false, Selectable = false });
        projectInfoPreferencesGroup.Add(new EntryRow
            { Title = "Schema version", Editable = false, Text_ = "overtest-1.0", Selectable = false });
        Control.Add(projectInfoPreferencesGroup);

        var taskInfoPreferencesGroup = new PreferencesGroup
        {
            Title = "Task details",
            Description = "Information about task, shown to all users"
        };
        taskInfoPreferencesGroup.Add(_projectTitleEntryRow = new EntryRow { Title = "Task title" });
        taskInfoPreferencesGroup.Add(_projectAuthorEntryRow = new EntryRow { Title = "Task author" });
        taskInfoPreferencesGroup.Add(_projectDifficultySpinRow = new SpinRow
        { 
            Title = "Task difficulty", Subtitle = "Can be a number between 0 and 100",
            Adjustment = new Gtk.Adjustment { Lower = 0, Upper = 100, StepIncrement = 1, Value = 1 }
        });
        var taskDescriptionEditAction = new ActionRow
        {
            Title = "Task description",
            Subtitle = "Description of the task in Markdown format",
            Activatable = true, IconName = "document-edit"
        };
        taskDescriptionEditAction.OnActivated += (_, _) => { };
        taskInfoPreferencesGroup.Add(taskDescriptionEditAction);

        Control.Add(taskInfoPreferencesGroup);
    }
}