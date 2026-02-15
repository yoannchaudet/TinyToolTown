/**
 * Extract "owner/repo" from a GitHub URL.
 * Returns null if the URL doesn't point to a valid GitHub repository.
 */
export function getGitHubRepo(url?: string): string | null {
  if (!url) return null;
  try {
    const parsed = new URL(url);
    if (parsed.hostname !== 'github.com' && parsed.hostname !== 'www.github.com') {
      return null;
    }
    const segments = parsed.pathname.split('/').filter(Boolean);
    if (segments.length < 2) return null;
    const repo = segments[1].replace(/\.git$/, '');
    return `${segments[0]}/${repo}`;
  } catch {
    return null;
  }
}
