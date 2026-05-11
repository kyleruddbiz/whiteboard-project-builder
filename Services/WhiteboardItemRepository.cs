using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Models.Serialization;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Manages persistence and retrieval of whiteboard items.
/// </summary>
public class WhiteboardItemRepository(DataPersistenceService dataPersistenceService, WhiteboardItemWorkspaceViewModel workspace)
{
    private const string DataFileName = "whiteboard-items.json";

    public async Task SaveWhiteboardItemsAsync(IEnumerable<WhiteboardItemViewModelBase> items)
    {
        var data = new WhiteboardData
        {
            Version = 1,
            Items = items.Select(vm => vm.ToModel()).ToList()
        };

        await dataPersistenceService.SaveAsync(data, DataFileName);
    }

    public async Task<IEnumerable<WhiteboardItemViewModelBase>> LoadWhiteboardItemsAsync()
    {
        var data = await dataPersistenceService.LoadAsync<WhiteboardData>(DataFileName);

        if (data?.Items == null)
        {
            return [];
        }

        var viewModels = new List<WhiteboardItemViewModelBase>();

        foreach (var item in data.Items)
        {
            WhiteboardItemViewModelBase viewModel = item switch
            {
                ProjectItem project => ProjectItemViewModel.FromModel(project, workspace),
                TaskItem task => TaskItemViewModel.FromModel(task, workspace),
                FullImageItem fullImage => FullImageItemViewModel.FromModel(fullImage, workspace),
                _ => throw new NotSupportedException($"Unsupported whiteboard item type: {item.GetType().Name}")
            };

            viewModels.Add(viewModel);
        }

        return viewModels;
    }
}
