# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
### Changed
### Removed


## 1.1.0 - 2021-06-23

### Added

- Enabled fixtures configuration on the assembly and class levels.
  - It is not a breaking change.

## 1.0.0 - 2021-06-07

### Added

- Initial projects
  - A basic builder source generator
    - the source generator creates the "with" methods basing on constructor parameters and settable properties.
      - if there is duplication in naming, a constructor parameter has higher priority than the property.
  - Sample projects
  - IntegrationTests
- a .gitignore file