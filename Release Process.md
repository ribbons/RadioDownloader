Radio Downloader Release Process
================================

This is the process that is followed to prepare and package up a release of Radio Downloader and make it available via http://nerdoftheherd.com/tools/radiodld/.

1. Create annotated tag for release: `git tag -a -m "x.x [beta] release" x.x`.
2. Run _build/publish-win32.bat_ to build & sign 32-bit application, providers and installer.
3. Run _build/publish-win64.bat_ to build & sign 64-bit application, providers and installer.
4. Test installers - major upgrade.
5. Test installers - clean install.
6. Draft release notes on GitHub and upload installers.
7. Write news item and update version in site properties (full release only).
8. Push master branch and relevant tags to GitHub: `git push --follow-tags`.
9. Publish release on GitHub.
10. Post message to beta mailing list about release or upload files and news to website.
11. Prevent errors from being submitted against previous version in error reporting.
