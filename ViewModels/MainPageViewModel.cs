using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Services;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly PrintService printService;

    public ObservableCollection<ProjectItemViewModel> Projects { get; }
    public ObservableCollection<GridItemWrapper> GridItems { get; }
    public ProjectItemViewModel SampleProject { get; }
    public GoalItemViewModel SampleGoal { get; }
    public InspirationItemViewModel SampleInspiration { get; }

    public MainPageViewModel(PrintService printService)
    {
        this.printService = printService;

        Projects = [];
        GridItems = [];

        SampleProject = new ProjectItemViewModel
        {
            Title = "Projects App",
            Subtitle = "Whiteboard Templates",
            Image = "Assets/Backgrounds/Examples/landscape-1.jpg",
            Size = ProjectSize.Large,
            Value = ProjectValue.Grand,
            DueDate = DateTime.Now.Date // = new DateTime(2025, 10, 23)
        };

        var initialProjects = new[]
        {
            SampleProject,
            new ProjectItemViewModel
            {
                Title = "Singing Bowl",
                Subtitle = "Record The Sounds",
                Image = "Assets/Backgrounds/Examples/portrait-1.jpg",
                Size = ProjectSize.Small,
                Value = ProjectValue.Good,
                DueDate = new DateTime(2025, 10, 7)
            },
            new ProjectItemViewModel
            {
                Title = "Draw Forms",
                Subtitle = "Practice Figure Drawing",
                Image = "Assets/Backgrounds/Examples/portrait-2.jpg",
                Size = ProjectSize.Medium,
                Value = ProjectValue.Great,
                DueDate = null
            },
            new ProjectItemViewModel
            {
                Title = "2026 Mazda3",
                Subtitle = "2.5 Turbo Premium Plus",
                Image = "Assets/Backgrounds/Examples/landscape-2.jpg",
                Size = ProjectSize.Huge,
                Value = ProjectValue.Grand,
                DueDate = null
            },
        };

        foreach (var project in initialProjects)
        {
            Projects.Add(project);
        }

        Projects.CollectionChanged += Projects_CollectionChanged;
        RebuildGridItems();

        SampleGoal = new GoalItemViewModel
        {
            Title = "Draw Forms",
            Subtitle = "Practice Figure Drawing",
            Image = "Assets/Backgrounds/Examples/portrait-2.jpg"
        };

        SampleInspiration = new InspirationItemViewModel
        {
            Text = "Some text that really inspires the people and relates in a clever way to puzzles\n\n– That Famous Person",
            Image = "Assets/Backgrounds/Examples/landscape-3.jpg"
        };
    }

    [RelayCommand]
    private async Task PrintProjectsAsync()
    {
        await printService.ShowPrintUIAsync(Projects);
    }

    private void Projects_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildGridItems();
    }

    private void RebuildGridItems()
    {
        GridItems.Clear();

        foreach (var project in Projects)
        {
            GridItems.Add(new GridItemWrapper { ProjectItem = project });
        }

        GridItems.Add(new GridItemWrapper { IsAddButton = true });
    }

    [RelayCommand]
    private void AddProject()
    {
        Projects.Add(new ProjectItemViewModel
        {
            Title = "New Project",
            Subtitle = "Add Details",
            Image = "Assets/Backgrounds/Examples/landscape-1.jpg",
            Size = ProjectSize.Medium,
            Value = ProjectValue.Good,
            DueDate = null
        });
    }
}