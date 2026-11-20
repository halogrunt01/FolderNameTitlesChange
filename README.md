Folder Name Titles

A Jellyfin plugin that automatically uses movie and TV show folder names as their displayed titles.

Features
Automatically renames movie titles using their containing folder names.
Automatically renames TV series titles using their containing folder names.
Updates Jellyfin's library metadata.
Locks the updated title to help prevent metadata providers from overwriting it.
Runs automatically after a library scan.
Preserves the original media files and folder structure.
Example

Instead of displaying the metadata-provider title:

Wicked

Jellyfin displays the folder name:

WICKED (2024) 1080p

Installation
Install the plugin through a compatible Jellyfin plugin repository once a release is available.
Restart Jellyfin.
Run a library scan.
The plugin will update movie and TV series titles based on their folder names.
Requirements
Jellyfin Server 10.11.x
.NET 9 compatible Jellyfin environment
Important Notes

This plugin changes titles stored in Jellyfin's library database. It does not rename files or folders on your storage drives.

Back up your Jellyfin configuration and database before installing.

Building from Source
powershell
dotnet restore
dotnet build

License

MIT License (see LICENSE file).