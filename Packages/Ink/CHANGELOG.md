# Changelog

All notable changes to this project will be documented in this file.

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