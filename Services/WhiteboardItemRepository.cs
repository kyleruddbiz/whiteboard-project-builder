using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Models.Serialization;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Manages persistence and retrieval of whiteboard items.
/// </summary>
public class WhiteboardItemRepository
{
    private const string DataFileName = "whiteboard-items.json";
    private readonly DataPersistenceService dataPersistenceService;
    private readonly ImageStorageService imageStorageService;
    private readonly ImageTransformService imageTransformService;
    private readonly ImageDimensionService imageDimensionService;

    public WhiteboardItemRepository(
        DataPersistenceService dataPersistenceService,
        ImageStorageService imageStorageService,
        ImageTransformService imageTransformService,
        ImageDimensionService imageDimensionService)
    {
        this.dataPersistenceService = dataPersistenceService;
        this.imageStorageService = imageStorageService;
        this.imageTransformService = imageTransformService;
        this.imageDimensionService = imageDimensionService;
    }

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
                ProjectItem project => ProjectItemViewModel.FromModel(project),
                SomedayMaybePair pair => CreateSomedayMaybePairViewModel(pair),
                _ => throw new NotSupportedException($"Unsupported whiteboard item type: {item.GetType().Name}")
            };

            viewModels.Add(viewModel);
        }

        return viewModels;
    }

    private SomedayMaybePairViewModel CreateSomedayMaybePairViewModel(SomedayMaybePair model)
    {
        var viewModel = new SomedayMaybePairViewModel(imageStorageService, imageTransformService, imageDimensionService);
        viewModel.LoadFromModel(model);
        return viewModel;
    }
}
