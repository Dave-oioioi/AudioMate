using System.Text.Json;

namespace AudioMate.Core.Narration;

/// <summary>
/// File-backed queue used by the Codex Aural Skill and AudioMate tray process.
/// </summary>
public sealed class FileNarrationQueue(string queueDirectoryPath) : INarrationQueue
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public void Enqueue(NarrationRequest request)
    {
        Directory.CreateDirectory(queueDirectoryPath);

        var fileName = $"{request.CreatedAt:yyyyMMddHHmmssfff}-{request.Id:N}.json";
        var finalPath = Path.Combine(queueDirectoryPath, fileName);
        var tempPath = $"{finalPath}.tmp";

        var json = JsonSerializer.Serialize(request, JsonOptions);
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, finalPath, overwrite: false);
    }

    public IReadOnlyList<NarrationRequest> ReadPending()
    {
        if (!Directory.Exists(queueDirectoryPath))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(queueDirectoryPath, "*.json")
            .Order(StringComparer.OrdinalIgnoreCase)
            .Select(ReadRequest)
            .OfType<NarrationRequest>()
            .ToArray();
    }

    public NarrationQueueItem? TryClaimNext()
    {
        if (!Directory.Exists(queueDirectoryPath))
        {
            return null;
        }

        foreach (var path in Directory.EnumerateFiles(queueDirectoryPath, "*.json").Order(StringComparer.OrdinalIgnoreCase))
        {
            var claimedPath = $"{path}.processing";
            try
            {
                File.Move(path, claimedPath, overwrite: false);
            }
            catch (IOException)
            {
                continue;
            }

            var request = ReadRequest(claimedPath);
            if (request is not null)
            {
                return new NarrationQueueItem(request, claimedPath);
            }

            File.Delete(claimedPath);
        }

        return null;
    }

    public void Complete(NarrationQueueItem item)
    {
        if (File.Exists(item.ClaimedFilePath))
        {
            File.Delete(item.ClaimedFilePath);
        }
    }

    private static NarrationRequest? ReadRequest(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<NarrationRequest>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
