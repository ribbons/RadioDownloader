Radio Downloader Release Process
================================

This is the process to prepare and package up a release of Radio Downloader and
make it available via https://nerdoftheherd.com/tools/radiodld/.

 * Create annotated tag for release: `git tag -a -m "x.x [beta] release" x.x`.
 * Push master branch and relevant tags to GitHub: `git push --follow-tags`.
 * Wait for AppVeyor build to complete.
 * Write release notes and publish release on GitHub.
 * Close GitHub issues milestone.
 * Update nerdoftheherd.com to generate release info, news and changelog.
 * Prevent errors from being submitted against previous version in error reporting.
