# Main Options Dialog

Radio Downloader has a number of options available for users. These can be
accessed by opening the Options menu on the left of the toolbar, and then
selecting Main Options.

## Run Radio Downloader on computer startup

Un-check this option to prevent Radio Downloader running on start-up.

## Minimise to notification area instead of taskbar button on close

Select this option to have Radio Downloader minimise to the notification area
instead of the taskbar when the main window is closed (under Windows 7 and
above).

## Parallel downloads

The maximum number of downloads to process at once. This defaults to the number
of CPU cores in your system minus 1, and has a maximum of double the number of
cores.

## Save downloaded programmes in

Press Change to browse for a different folder to save downloaded programmes in.

## Downloaded programme file name format

With this option, you can specify the format string that Radio Downloader
will use to generate the filenames for downloaded episodes.

The following substitutions are available to be used in this format string:

### %progname%

The name of the programme which this is an episode of.

### %epname%

The name of this episode.

### %hour%

The hour of the episode.

### %minute%

The minute of the hour of the episode.

### %day%

The day of the month of the episode.

### %month%

The number of the month of the episode.

### %shortmonthname%

The short name for the month of the episode.

### %monthname%

The name for the month of the episode.

### %year%

The two digit year of the episode.

### %longyear%

The four digit year of the episode.

You can also use slashes in the format string to denote subfolders (which will
be created automatically). For instance, specifying the format string
`%progname%\%epname% %day%-%month%-%year%` will cause downloads to be organised
into folders per programme.

## Run command after download

If a program or command is specified here, Radio Downloader will run this after
an episode of a programme has been downloaded.

The following substitutions are available to be used with this option:

### %file%

The full path to the downloaded file.
