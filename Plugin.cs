
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace FolderNameTitles;

public sealed class PluginConfiguration : BasePluginConfiguration
{
}

public sealed class Plugin : BasePlugin<PluginConfiguration>
{
    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
    }

    public override string Name => "Folder Name Titles";

    public override Guid Id =>
        Guid.Parse("d8f1e5c2-7a93-4b61-9e24-6c8f31a5b207");
}

public sealed class FolderNameTask : ILibraryPostScanTask
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<FolderNameTask> _logger;

    public FolderNameTask(
        ILibraryManager libraryManager,
        ILogger<FolderNameTask> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;

        _logger.LogInformation(
            "Folder Name Titles: Task constructed.");
    }

    public async Task Run(
        IProgress<double> progress,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Folder Name Titles: Starting title update.");

        var items = _libraryManager.GetItemList(
            new InternalItemsQuery
            {
                IncludeItemTypes =
                [
                    BaseItemKind.Movie,
                    BaseItemKind.Series
                ],
                Recursive = true
            });

        _logger.LogInformation(
            "Folder Name Titles: Found {Count} movies and TV shows.",
            items.Count);

        var changed = 0;
        var processed = 0;
        var total = Math.Max(items.Count, 1);

        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var path = item.Path;

            if (!string.IsNullOrWhiteSpace(path))
            {
                var directory = Directory.Exists(path)
                    ? path
                    : Path.GetDirectoryName(path);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    var folderName = new DirectoryInfo(directory).Name;

                    if (!string.IsNullOrWhiteSpace(folderName))
                    {
                        var lockedFields = item.LockedFields ?? [];

                        var nameLocked =
                            lockedFields.Contains(MetadataField.Name);

                        var titleChanged = !string.Equals(
                            item.Name,
                            folderName,
                            StringComparison.Ordinal);

                        var sortChanged = !string.Equals(
                            item.ForcedSortName,
                            folderName,
                            StringComparison.Ordinal);

                        if (titleChanged || sortChanged || !nameLocked)
                        {
                            var oldTitle = item.Name;

                            item.Name = folderName;
                            item.ForcedSortName = folderName;

                            if (!nameLocked)
                            {
                                item.LockedFields = lockedFields
                                    .Append(MetadataField.Name)
                                    .ToArray();
                            }

                            var parent = item.GetParent();

                            if (parent is not null)
                            {
                                await _libraryManager.UpdateItemAsync(
                                    item,
                                    parent,
                                    ItemUpdateType.MetadataEdit,
                                    cancellationToken);

                                _logger.LogInformation(
                                    "Updated {Type} title from '{OldTitle}' to '{NewTitle}'",
                                    item.GetType().Name,
                                    oldTitle,
                                    folderName);

                                changed++;
                            }
                        }
                    }
                }
            }

            processed++;
            progress.Report((double)processed / total * 100);
        }

        _logger.LogInformation(
            "Folder Name Titles: Completed. Updated {Changed} of {Total} items.",
            changed,
            items.Count);

        progress.Report(100);
    }
}
