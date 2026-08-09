# ULSAlgorithms 0.1.0 — C# code bootstrap

This bundle adds only the C# technical skeleton. It does **not** contain any ULS
algorithm, mathematical model, heuristic, formulation, or separation routine.

## Files added

- `ULSAlgorithms.sln`
- `src/ULSAlgorithms/`
- `tests/ULSAlgorithms.Tests/`
- `benchmarks/ULSAlgorithms.Benchmarks/`

## Target

- .NET 10 (`net10.0`)
- xUnit.net v3 test project, executed through VSTest by the existing CI
- BenchmarkDotNet benchmark harness
- Existing repository-wide Nerdbank.GitVersioning configuration is inherited
  from `Directory.Build.props` and `version.json`

## Installation

Copy the content of this bundle directly into the repository root:

`D:\Dev\UlsAlgorithm\ULSAlgorithms`

Do not create an extra enclosing directory.

Then open `ULSAlgorithms.sln` in Visual Studio 2026.

## Local validation

1. Build the solution in Release configuration.
2. Run all tests in Test Explorer.
3. Do not run the benchmark as part of normal unit tests.
4. Do not create a release until the GitHub `Build and Test` and
   `Build Documentation` workflows are green.

## Intended v0.1.0 release

The 0.1.0 release is an infrastructure release. Its public DLL intentionally
contains only `ULSAlgorithmsInfo`, which validates assembly/version metadata.

The first ULS domain abstractions and algorithm implementations must be added
only after this bootstrap release is validated end-to-end.
