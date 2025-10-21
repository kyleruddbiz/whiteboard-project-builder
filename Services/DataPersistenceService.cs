using System.Text.Json;
using System.Text.Json.Serialization;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Models.Serialization;
using WhiteboardProjectBuilder.ViewModels;
using Windows.Storage;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Manages persistence of data to JSON files in ApplicationData.LocalFolder.
/// </summary>
public class DataPersistenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private const string DataFileName = "whiteboard-items.json";

    /// <summary>
    /// Saves data to JSON file in LocalFolder.
    /// </summary>
    public async Task SaveAsync<T>(T data, string fileName)
    {
        await Task.Run(async () =>
        {
            try
            {
                string json = JsonSerializer.Serialize(data, JsonOptions);
                string filePath = Path.Combine(
                    ApplicationData.Current.LocalFolder.Path,
                    fileName
                );

                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save to {fileName}: {ex.Message}", ex);
            }
        });
    }

    /// <summary>
    /// Loads data from JSON file. Returns default if no file exists.
    /// </summary>
    public async Task<T?> LoadAsync<T>(string fileName)
    {
        return await Task.Run(async () =>
        {
            try
            {
                string filePath = Path.Combine(
                    ApplicationData.Current.LocalFolder.Path,
                    fileName
                );

                if (!File.Exists(filePath))
                {
                    return default;
                }

                string json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load from {fileName}: {ex.Message}", ex);
            }
        });
    }

    /// <summary>
    /// Saves whiteboard items to JSON file in LocalFolder.
    /// </summary>
    public async Task SaveWhiteboardItemsAsync(IEnumerable<WhiteboardItemViewModelBase> items)
    {
        var data = new WhiteboardData
        {
            Version = 1,
            Items = items.Select(vm => vm.ToModel()).ToList()
        };

        await SaveAsync(data, DataFileName);
    }

    /// <summary>
    /// Loads whiteboard items from JSON file. Returns empty collection if no file exists.
    /// </summary>
    public async Task<IEnumerable<WhiteboardItemViewModelBase>> LoadWhiteboardItemsAsync()
    {
        var data = await LoadAsync<WhiteboardData>(DataFileName);

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
                SomedayMaybePair pair => SomedayMaybePairViewModel.FromModel(pair),
                _ => throw new NotSupportedException($"Unsupported whiteboard item type: {item.GetType().Name}")
            };

            viewModels.Add(viewModel);
        }

        return viewModels;
    }
}
