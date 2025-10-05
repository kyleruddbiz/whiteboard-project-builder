using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Services;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly PrintService printService;

    public ObservableCollection<ProjectItemViewModel> Projects { get; }
    public ProjectItemViewModel SampleProject { get; }
    public GoalItemViewModel SampleGoal { get; }
    public InspirationItemViewModel SampleInspiration { get; }

    public MainPageViewModel(PrintService printService)
    {
        this.printService = printService;

        SampleProject = new ProjectItemViewModel
        {
            Title = "Projects App",
            Subtitle = "Whiteboard Templates",
            Image = "Assets/Backgrounds/Examples/landscape-1.jpg",
            Size = ProjectSize.Large,
            Value = ProjectValue.Grand,
            DueDate = DateTime.Now.Date // = new DateTime(2025, 10, 23)
        };

        Projects =
        [
            SampleProject,
            // Add more test items
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

             new ProjectItemViewModel
            {
                Title = "222 Draw Forms",
                Subtitle = "Practice Figure Drawing",
                Image = "Assets/Backgrounds/Examples/portrait-3.jpg",
                Size = ProjectSize.Medium,
                Value = ProjectValue.Good,
                DueDate = null
            },
        ];

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
}