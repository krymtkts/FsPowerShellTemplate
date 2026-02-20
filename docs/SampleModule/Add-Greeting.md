---
document type: cmdlet
external help file: SampleModule-Help.xml
HelpUri: https://github.com/krymtkts/FsPowerShellTemplate/blob/main/docs/FsPowerShellTemplate/SampleModule/Add-Greeting.md
Locale: en-US
Module Name: SampleModule
ms.date: 02-20-2026
PlatyPS schema version: 2024-05-01
title: Add-Greeting
---

# Add-Greeting

## SYNOPSIS

Adds a name to the in-memory greeting store.

## SYNTAX

### __AllParameterSets

```powershell
Add-Greeting [-Name] <string> [<CommonParameters>]
```

## ALIASES

None.

## DESCRIPTION

Adds the specified name to a module-wide in-memory store.

Other components in this module can read the same store.
For example: predictor/feedback provider.
The store exists for the lifetime of the current PowerShell session.

## EXAMPLES

### Example 1

```powershell
Add-Greeting -Name sample
```

## PARAMETERS

### -Name

Who to greet.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 0
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutBuffer, -OutVariable, -PipelineVariable,
-ProgressAction, -Verbose, -WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

You can pipe a string that represents the name to add.

## OUTPUTS

This cmdlet does not return anything.

## NOTES

The greeting store is in-memory and is not persisted across sessions.

## RELATED LINKS

- [Get-Greeting](https://github.com/krymtkts/FsPowerShellTemplate/blob/main/docs/FsPowerShellTemplate/SampleModule/Get-Greeting.md)
