using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Manages persistence of user settings.
/// </summary>
public class SettingsService
{
    private readonly DataPersistenceService dataPersistenceService;
    private const string SettingsFileName = "settings.json";

    public SettingsService(DataPersistenceService dataPersistenceService)
    {
        this.dataPersistenceService = dataPersistenceService;
    }

    /// <summary>
    /// Saves settings to JSON file.
    /// </summary>
    public async Task SaveSettingsAsync(Settings settings)
    {
        await dataPersistenceService.SaveAsync(settings, SettingsFileName);
    }

    /// <summary>
    /// Loads settings from JSON file. Returns default settings if no file exists.
    /// </summary>
    public async Task<Settings> LoadSettingsAsync()
    {
        var settings = await dataPersistenceService.LoadAsync<Settings>(SettingsFileName);
        return settings ?? new Settings();
    }
}
