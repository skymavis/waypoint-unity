# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.4.1] - 2024-11-25

### :wrench: Fixed

- Fixed non-iOS build with IL2CPP backend.

## [0.4.0] - 2024-11-19

### :rotating_light: BREAKING CHANGES

- Converted Waypoint SDK to the Unity Package Manager (UPM) format, allowing easier installation and version management. Users can now [install the SDK directly via a git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html) with [extended syntax](https://docs.unity3d.com/Manual/upm-git.html#syntax):
  ```
  https://github.com/skymavis/waypoint-unity.git#v0.4.0
  ```
- Introduced new API methods to replace older, deprecated ones. Deprecated methods remain available for backward compatibility but will be removed in the next major release.
  - `Waypoint.BindOnResponse(callback)` and `Waypoint.UnBindOnResponse(callback)` are deprecated. The `Waypoint.RespondReceived` C# event is the new recommended replacement.
  - Additional deprecated methods and their replacements:
    - `Waypoint.Init(sessionID, port)` ➔ `Waypoint.SetUp(WaypointSettings)`
    - `Waypoint.Init(clientID, keepLinkSchema, isTestNet)` ➔ `Waypoint.SetUp(WaypointSettings)`
    - `Waypoint.OnAuthorize()` ➔ `Waypoint.Authorize()`
    - `Waypoint.OnGetIDToken()` ➔ `Waypoint.Authorize()`
    - `Waypoint.OnPersonalSign(message, from)` ➔ `Waypoint.PersonalSign(message, from)`
    - `Waypoint.OnSignTypeData(typedData, from)` ➔ `Waypoint.SignTypedData(typedData, from)`
    - `Waypoint.SendTransaction(receiverAddress, value, from)` ➔ `Waypoint.SendNativeToken(receiverAddress, value, from)`
    - `Waypoint.OnCallContract(contractAddress, data, value, from)` ➔ `Waypoint.WriteContract(contractAddress, humanReadableAbi, functionParameters, value, from)`
  - **Note**: Deprecated methods will be fully removed in version 0.5.0.

### :sparkles: Added

- Introduced `Waypoint.CleanUp()` to release managed resources when the SDK is no longer in use. This method is now required for proper cleanup of the SDK’s resources.

## [0.3.0] - 2024-10-23

## [0.2.1] - 2024-09-13

## [0.2.0] - 2024-07-31

## [0.1.0] - 2024-05-30

[Unreleased]: https://github.com/skymavis/waypoint-unity/compare/v0.4.1...HEAD
[0.4.1]: https://github.com/skymavis/waypoint-unity/compare/v0.4.0...v0.4.1
[0.4.0]: https://github.com/skymavis/waypoint-unity/compare/waypoint-unity%2F0.3.0...v0.4.0
[0.3.0]: https://github.com/skymavis/waypoint-unity/compare/waypoint-unity%2F0.2.1...waypoint-unity%2F0.3.0
[0.2.1]: https://github.com/skymavis/waypoint-unity/compare/mavis-id-unity%2F0.2.0...waypoint-unity%2F0.2.1
[0.2.0]: https://github.com/skymavis/waypoint-unity/compare/mavis-id-unity%2F0.1.0...mavis-id-unity%2F0.2.0
[0.1.0]: https://github.com/skymavis/waypoint-unity/releases/tag/mavis-id-unity%2F0.1.0
