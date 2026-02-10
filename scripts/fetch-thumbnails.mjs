#!/usr/bin/env node
// One-time script to download README images for all existing tools
// Usage: node scripts/fetch-thumbnails.mjs

import fs from 'fs';
import path from 'path';

const TOOLS_DIR = 'src/content/tools';
const THUMBS_DIR = 'public/thumbnails';

const badgeHosts = ['img.shields.io', 'badge.fury.io', 'badgen.net', 'badges.', 'coveralls.io', 'codecov.io', 'travis-ci.', 'ci.appveyor.com', 'github.com/workflows'];
const isBadge = (url) => badgeHosts.some(h => url.includes(h)) || url.includes('/badge');

async function findReadmeImage(owner, repo) {
  for (const branch of ['main', 'master']) {
    const rawUrl = `https://raw.githubusercontent.com/${owner}/${repo}/${branch}/README.md`;
    const res = await fetch(rawUrl);
    if (!res.ok) continue;

    const readme = await res.text();
    const mdMatches = [...readme.matchAll(/!\[[^\]]*\]\(([^)]+)\)/g)];
    const firstRealMd = mdMatches.find(m => !isBadge(m[1]));
    const htmlMatches = [...readme.matchAll(/<img[^>]+src=["']([^"']+)["']/gi)];
    const firstRealHtml = htmlMatches.find(m => !isBadge(m[1]));

    const imgPath = firstRealMd?.[1] || firstRealHtml?.[1] || null;
    if (!imgPath) continue;

    let absoluteUrl = imgPath;
    if (!imgPath.startsWith('http')) {
      const cleanPath = imgPath.replace(/^\.\//, '');
      absoluteUrl = `https://raw.githubusercontent.com/${owner}/${repo}/${branch}/${cleanPath}`;
    }
    return { url: absoluteUrl, branch };
  }
  return null;
}

async function downloadImage(url, destPath) {
  const res = await fetch(url);
  if (!res.ok) throw new Error(`HTTP ${res.status} for ${url}`);
  const buffer = Buffer.from(await res.arrayBuffer());
  fs.writeFileSync(destPath, buffer);
  return buffer.length;
}

function parseFrontmatter(content) {
  const match = content.match(/^---\r?\n([\s\S]*?)\r?\n---/);
  if (!match) return {};
  const fm = {};
  for (const line of match[1].split('\n')) {
    const m = line.match(/^([\w_]+):\s*"?([^"]*)"?\s*$/);
    if (m) fm[m[1]] = m[2];
  }
  return fm;
}

async function main() {
  const files = fs.readdirSync(TOOLS_DIR).filter(f => f.endsWith('.md'));
  console.log(`Found ${files.length} tools\n`);

  for (const file of files) {
    const slug = file.replace(/\.md$/, '');
    const content = fs.readFileSync(path.join(TOOLS_DIR, file), 'utf-8');
    const fm = parseFrontmatter(content);
    const githubUrl = fm.github_url;
    
    if (!githubUrl) {
      console.log(`⏭️  ${slug}: no github_url, skipping`);
      continue;
    }

    const repoMatch = githubUrl.match(/github\.com\/([^/]+)\/([^/]+)/);
    if (!repoMatch) {
      console.log(`⏭️  ${slug}: can't parse github_url`);
      continue;
    }

    const [, owner, repo] = repoMatch;
    console.log(`🔍 ${slug}: checking ${owner}/${repo}...`);

    try {
      const result = await findReadmeImage(owner, repo);
      if (!result) {
        console.log(`   ❌ No image found in README`);
        continue;
      }

      const urlPath = new URL(result.url).pathname;
      const ext = path.extname(urlPath).split('?')[0] || '.png';
      const thumbFile = `${slug}${ext}`;
      const destPath = path.join(THUMBS_DIR, thumbFile);

      const size = await downloadImage(result.url, destPath);
      console.log(`   ✅ Downloaded ${thumbFile} (${(size / 1024).toFixed(1)} KB)`);

      // Update the .md file to add thumbnail field
      const thumbPath = `/thumbnails/${thumbFile}`;
      if (content.includes('thumbnail:')) {
        const updated = content.replace(/thumbnail:.*/, `thumbnail: "${thumbPath}"`);
        fs.writeFileSync(path.join(TOOLS_DIR, file), updated);
      } else {
        const updated = content.replace(
          /(github_url:.*\n)/,
          `$1thumbnail: "${thumbPath}"\n`
        );
        fs.writeFileSync(path.join(TOOLS_DIR, file), updated);
      }
      console.log(`   📝 Updated frontmatter`);
    } catch (e) {
      console.log(`   ❌ Error: ${e.message}`);
    }
  }
  
  console.log('\nDone!');
}

main();
