using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Enums;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    public ProjectItemViewModel SampleProject { get; }
    public GoalItemViewModel SampleGoal { get; }
    public InspirationItemViewModel SampleInspiration { get; }

    public MainPageViewModel()
    {
        SampleProject = new ProjectItemViewModel
        {
            Title = "Projects App",
            Subtitle = "Whiteboard Templates",
            Image = "Assets/Backgrounds/Examples/landscape-1.jpg",
            Size = ProjectSize.Large,
            Value = ProjectValue.Grand,
            DueDate = new DateTime(2025, 10, 23)
        };

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
}