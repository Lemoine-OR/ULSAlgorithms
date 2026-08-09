# ULSAlgorithms Graphviz CI fix

Cause:
Chocolatey returned HTTP 504 while resolving Graphviz. This is an external
package-feed failure, not a ULSAlgorithms build failure.

This patch removes the Chocolatey dependency for Graphviz from both:
- `.github/workflows/documentation.yml`
- `.github/workflows/release.yml`

It adds:
- `tools/Install-Graphviz.ps1`

The installer:
1. pins Graphviz 15.1.1;
2. downloads the official upstream Windows x64 ZIP from Graphviz GitLab;
3. retries transient downloads up to four times;
4. verifies the official SHA-256:
   e8256ef077e601d9f284378d96cd17faa7910832cf6bb85c43005e66ec2f255e
5. extracts the portable archive;
6. locates `dot.exe`;
7. adds its directory to `GITHUB_PATH`;
8. validates `dot -V`.

Apply to the repository root, rebuild locally if desired, commit and push.
The documentation workflow should then rerun without using Chocolatey.
