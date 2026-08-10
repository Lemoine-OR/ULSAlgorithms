# Apply the ULSAlgorithms documentation redesign

This overlay is designed for the repository state after public release **v0.14.0**.

It **does not modify `version.json`** and **does not modify any algorithm implementation**.

## 1. Extract

Extract the ZIP directly into:

```text
D:\Dev\UlsAlgorithm\ULSAlgorithms
```

and replace the documentation/README files when asked.

## 2. Clean the repository root

The repository still contains temporary overlay manifests and INSTALL/APPLY notes from incremental development.

Run:

```powershell
pwsh .\tools\Clean-LegacyRootArtifacts.ps1
```

This deletes only the explicit legacy file list embedded in the script.

## 3. Validate code first

In Visual Studio:

```text
Release → Rebuild Solution
Run All Tests
```

The redesign must not change solver behavior.

## 4. Build the documentation locally

```powershell
.\tools\Install-Graphviz.ps1
.\tools\Install-Doxygen.ps1
.\docs\build-documentation.ps1
```

Then open:

```text
Documentation\site\index.html
```

The build now validates local documentation links.

## 5. Review the identity

Main assets:

```text
docs\assets\algorithms-icon.svg
docs\assets\algorithms-logo.svg
docs\assets\ulsalgorithms-logo.svg
docs\brand\github-social-preview.png
docs\brand\BRAND-GUIDE.md
```

`algorithms-icon.*` and `algorithms-logo.svg` are the **shared Lemoine-OR Algorithms identity** intended for future pure-algorithm projects.

## 6. GitHub social preview

After the redesign is committed, upload:

```text
docs\brand\github-social-preview.png
```

in:

```text
GitHub repository → Settings → General → Social preview
```

This is a GitHub repository setting and cannot be changed by a normal source commit.

## 7. Do not bump the release version yet

Keep the existing version while validating the redesign. Decide the next release number only after the portal and README have been reviewed.

## Suggested commit message after validation

```text
Redesign documentation portal and algorithm identity
```
