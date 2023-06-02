# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [3.0.1] - 2023-06-02

### Added
- Exposed `nc_inq_libvers` function as `string NetCDFInterop.NetCDF.nc_inq_libvers()`.
## [3.0.0] - 2023-06-01

### Added
- Tests for `.NETFramework,Version=v4.8`.

### Removed
- Obsolete `CoordinateSystem` class and all related functionality.
- Obsolete `AddVariableByValue` methods.

### Changed
- `CsvDataset` now writes floating point values using the `R` format specifier.
  This ensures that the value doesn't change when it is read back.

## [2.0.4] - 2023-05-14

### Added
- Tests for CSV dataset.
- This `CHANGELOG.md`

### Changed
- `netCDF` cross-platform P/Invoke is now done by .NET. The dependency on `DynamicInterop` has been removed.
- `CsvDataset` handles absence of `MissingValue` attribute. This closes #38.
- `README.md` now contains sample code. It is now packaged in `.nuget`.