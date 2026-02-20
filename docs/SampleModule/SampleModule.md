---
document type: module
Help Version: 1.0.0.0
HelpInfoUri:
Locale: en-US
Module Guid: 5f5b4b6a-8e9b-46f0-9451-629afba26f0a
Module Name: SampleModule
ms.date: 02-20-2026
PlatyPS schema version: 2024-05-01
title: SampleModule Module
---

# SampleModule Module

## Description

A sample PowerShell module. This module requires PowerShell 7.4 or higher.

This module also registers the following PowerShell subsystems when you import it:

- Command-line predictor: suggests `Hello <name>, PowerShell from F#!`.
  It uses names added with `Add-Greeting`.
  Accepting a suggestion removes the name from the store.
- Feedback provider: on successful commands, shows feedback when the store updates.
  It reports the greeting count.

## SampleModule

### [Add-Greeting](https://github.com/krymtkts/FsPowerShellTemplate/blob/main/docs/FsPowerShellTemplate/SampleModule/Add-Greeting.md)

Adds a name to the in-memory greeting store.

### [Get-Greeting](https://github.com/krymtkts/FsPowerShellTemplate/blob/main/docs/FsPowerShellTemplate/SampleModule/Get-Greeting.md)

Gets the names stored in the greeting store.
