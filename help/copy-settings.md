# Copy Settings to a New Machine

You can migrate your settings, subscriptions and list of downloads to a new
computer by copying the Radio Downloader database:

* Make sure that Radio Downloader is installed but not running on both machines.
* Enter `%appdata%\nerdoftheherd.com\Radio Downloader` into the start menu /
  start screen search and press enter.
* Copy `store.db` from the folder that appears across to a removable drive.
* Connect the removable drive to the destination machine.
* On the destination machine: Enter `%appdata%\nerdoftheherd.com\Radio Downloader`
  into the start menu / start screen search and press enter.
* Copy `store.db` into the folder that appears from the removable drive.
* Start Radio Downloader on the destination machine.

Radio Downloader on the destination machine should now have your settings,
subscriptions and list of downloads. You will also need to copy across the
downloaded audio files from the source machine to the same path on the
destination machine if you want to be able to access them from within Radio
Downloader.

There is [an issue
raised](https://github.com/ribbons/RadioDownloader/issues/72) to add an export
and import facility for data and settings to make this process easier.
