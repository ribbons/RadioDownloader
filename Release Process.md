Radio Downloader Release Process
================================

This is the process to prepare and package up a release of Radio Downloader and
make it available via https://nerdoftheherd.com/tools/radiodld/.

 * Create annotated tag for release: `git tag -a -m "x.x [beta] release" x.x`.
 * Run _build/publish-win32.bat_ to build & sign 32-bit application, providers and installer.
 * Run _build/publish-win64.bat_ to build & sign 64-bit application, providers and installer.
 * Test Windows installers.
 * Draft release notes on GitHub and upload installers.
 * Push master branch and relevant tags to GitHub: `git push --follow-tags`.
 * Publish release on GitHub.
 * Close GitHub issues milestone.
 * Update nerdoftheherd.com website with latest release, news and changelog.
 * Prevent errors from being submitted against previous version in error reporting.
