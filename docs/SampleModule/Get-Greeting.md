---
document type: cmdlet
external help file: SampleModule-Help.xml
HelpUri: https://github.com/krymtkts/FsPowerShellTemplate/blob/main/docs/FsPowerShellTemplate/SampleModule.md
Locale: en-US
Module Name: SampleModule
ms.date: 02-20-2026
PlatyPS schema version: 2024-05-01
title: Get-Greeting
---

# Get-Greeting

## SYNOPSIS

Gets the names stored in the greeting store.

## SYNTAX

### __AllParameterSets

```powershell
Get-Greeting [<CommonParameters>]
```

## ALIASES

None.

## DESCRIPTION

Gets a snapshot of names added with `Add-Greeting`.

The cmdlet writes each stored name to the pipeline as a string.

## EXAMPLES

### Example 1

Gets all stored names.

```powershell
Get-Greeting
```

## PARAMETERS

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutBuffer, -OutVariable, -PipelineVariable,
-ProgressAction, -Verbose, -WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### System.String

Each stored name.

## NOTES

The greeting store is in-memory and is not persisted across sessions.

## RELATED LINKS

- [Add-Greeting](https://github.com/krymtkts/FsPowerShellTemplate/blob/main/docs/FsPowerShellTemplate/SampleModule/Add-Greeting.md)
