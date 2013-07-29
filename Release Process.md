Radio Downloader Release Process
================================

This is the process that is followed to prepare and package up a release of Radio Downloader and make it available via http://nerdoftheherd.com/tools/radiodld/.

Beta & Full Releases
--------------------

1. Update version number in application & providers
2. Commit version changes.
3. Run _build/publish-win32.bat_ to build & sign 32-bit application, providers and installer
4. Run _build/publish-win64.bat_ to build & sign 64-bit application, providers and installer
5. Test installers - major upgrade
6. Test installers - clean install
7. Create annotated tag for release: `git tag -a -m "x.x release" x.x`
8. Push branch and relevant tags to GitHub: `git push --follow-tags`

Beta Release
------------

9. Move installers to local nerdoftheherd _downloads/beta_ folder.
10. Add new version number into the Radio Downloader product in Bugzilla.
11. Upload to website.
12. Post message to beta mailing list about release.

Full Release
------------

9. Move installers to local nerdoftheherd _downloads_ and sourceforge folders.
10. Update nerdoftheherd.com properties file with latest version number.
11. Add new version number into the Radio Downloader product in Bugzilla.
12. Upload installers to SourceForge
13. Write news item.
14. Mark win32 installer as Windows platform default on SourceForge
15. Upload files and news to website.
16. Prevent errors from being submitted against previous version in error reporting.
