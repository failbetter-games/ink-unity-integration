# Changelog

All notable changes to this project will be documented in this file.

## 1.0.211 - 2024-05-30

### Fixed
- Calling Ink custom functions.

## 1.0.210 - 2024-05-10

### Added
- Support for InkLists and its Native Functions in the EvaluateAtRuntime function.

### Modified
- Moved custom functions to a partial class of Runtime.Story.cs.
- Substituted literals for NativeFunctionCall constants where possible.


## 1.0.208 - 2022-09-09

### Modified
- Default and Global variable names are now accesible through VariablesState.

## 1.0.207 - 2022-02-15

### Added
- EvaluateExternalFunction method at Ink.Runtime.Story, which allows to test external calls from Unity. 

### Fixed
- EvaluateAtRuntime was not able to evaluate external functions. 


## 1.0.206 - 2021-11-04

### Modified
- onChoiceCreated now triggers even if Choice is null, allowing Unity to adjust accordingly for payloads.


## 1.0.205 - 2021-07-02

### Modified
- EvaluateAtRuntime now takes UnaryExpressions into account.

### Fixed
- EvaluateAtRuntime: Ink's transforms true into True, which results to null. This is now taken into account.


## 1.0.204 - 2021-07-02

### Fixed
- ResultOfBinaryOperation: Equal operation was not using .Equals(...).


## 1.0.203 - 2021-06-29

### Added
- Tags to Choices at Runtime. Accesible through choice.tags.


## 1.0.202 - 2021-06-25

### Modified
- InkParser's Expression() method is now public.

### Added
- EvaluateAtRuntime function and ResultOfBinaryOperation to evaluate Expressions at Runtime.


## 1.0.201 - 2021-06-18

### Modified
- Locked choices are no longer hidden for Unity. Instead, an attribute is set on the Choice object (choice.isTrue), and checked before selecting the choice.

### Added
- Condition attribute to Choice and ChoicePoint. Stores the condition string for this Choice.
- IsTrue attribute to Choice and ChoicePoint. Stores whether the condition for this Choice passed.
- onChoiceCreated event.
- Added payload attribute to Choice.