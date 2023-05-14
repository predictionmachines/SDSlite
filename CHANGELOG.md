# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.5] - 2023-05-14

### Added
- Tests for CSV dataset.
- This `CHANGELOG.md`

### Changed
- `netCDF` cross-platform P/Invoke is now done by .NET. The dependency on `DynamicInterop` has been removed.
- `CsvDataset` handles absence of `MissingValue` attribute. This closes #38.
- `README.md` now contains sample code. It is now packaged in `.nuget`.