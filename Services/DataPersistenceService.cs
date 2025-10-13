using System.Text.Json;
using System.Text.Json.Serialization;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Models.Serialization;
using WhiteboardProjectBuilder.ViewModels;
using Windows.Storage;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Manages persistence of whiteboard data to JSON files in ApplicationData.LocalFolder.
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

    private const string DataFileName = "project-items.json";

    /// <summary>
    /// Saves projects to JSON file in LocalFolder.
    /// </summary>
    public async Task SaveProjectsAsync(IEnumerable<ProjectItemViewModel> projects)
    {
        await Task.Run(async () =>
        {
            try
            {
                var data = new WhiteboardData
                {
                    Version = 1,
                    Projects = projects.Select(vm => vm.ToModel()).ToList()
                };

                var json = JsonSerializer.Serialize(data, JsonOptions);
                var filePath = Path.Combine(
                    ApplicationData.Current.LocalFolder.Path,
                    DataFileName
                );

                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                // Log error - for now, just rethrow to be caught by caller
                throw new InvalidOperationException($"Failed to save projects: {ex.Message}", ex);
            }
        });
    }

    /// <summary>
    /// Loads projects from JSON file. Returns empty collection if no file exists.
    /// </summary>
    public async Task<IEnumerable<ProjectItemViewModel>> LoadProjectsAsync()
    {
        return await Task.Run(async () =>
        {
            try
            {
                var filePath = Path.Combine(
                    ApplicationData.Current.LocalFolder.Path,
                    DataFileName
                );

                // If no save file exists, return empty collection
                if (!File.Exists(filePath))
                {
                    return [];
                }

                var json = await File.ReadAllTextAsync(filePath);
                var data = JsonSerializer.Deserialize<WhiteboardData>(json, JsonOptions);

                if (data?.Projects == null)
                {
                    return [];
                }

                return data.Projects.Select(m => ProjectItemViewModel.FromModel(m)).ToList();
            }
            catch (Exception ex)
            {
                // Log error and return empty collection to prevent app crash
                throw new InvalidOperationException($"Failed to load projects: {ex.Message}", ex);
            }
        });
    }
}
