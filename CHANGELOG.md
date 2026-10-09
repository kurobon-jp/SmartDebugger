# Changelog

All notable changes to SmartDebugger are documented in this file.

## [1.0.32] - 2026-09-14

### Changed

- Added guards for zero-sized screen dimensions.
- Updated `SafeAreaContainer` to detect screen-size and safe-area changes using the actual display dimensions.

### Fixed

- Fixed `NaN` values being assigned to `SafeAreaContainer` content anchors when the screen dimensions were not initialized yet.
- Fixed safe-area mesh calculations during zero-sized screen initialization.

## [1.0.33] - 2026-10-09

### Changed

- InstantReplay packages to version v1.6.11