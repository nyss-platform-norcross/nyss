# RX.Nyss

Welcome to the repository for the new CBS solution, called *NYSS*! 

Nyss is a norwegian word and means to "get the wind of something". And the first norwegian computer was called [Nusse](https://no.wikipedia.org/wiki/Nusse).

## Getting started
dotnet run

## How to contribute

### Code review checklist
* PR branch is on top of master
* Build and all unit tests are green (and business logic is covered by unit tests)
* The code fulfills the acceptance criterias
* The code is easy to understand or has substantial documentation on how to understand
* "How to test" steps should be applied and understandable for both tester and reviewer
* Test that it is working in dev once the PR is completed, merged to master and deployed
* The code is formatted according to code conventions
* If datamodel changes, ER diagram needs to be updated

### Documentation
* How to run locally
  * Tools and frameworks needed
  * Local configuration
  * Useful commands
* High level architecture diagram
* Datamodel ER diagram
* Code should be self-explanatory, but when not it should be possible for the developer looking at to get some help by comments.
* Swagger: xmldoc should be used for specififying endpoints in api controllers.

### Code conventions
* C# code style should be specified in the [.editorconfig](./.editorconfig) file in root. Examples:
  * line length (and wrap)
  * public members on top
  * object initializers
  * usings
* Keep it simple
* One developer creates initial code convention specification and the rest follows

### Git commit message style
* [Guidelines to look at](https://github.com/trein/dev-best-practices/wiki/Git-Commit-Best-Practices)
* Commit often and push if you feel for it. Squash them if a lot of them doesn't add value
* Branch name format: `feature/ or bugfix/`-`workitem number`-`workitem name`
* Commit messages: Imperative style: Write what the code now does and not what the developer has done. Example:
```
# not
Added some code
# correct
Add validation rules for creating supervisor

- Add mapper
- Add something
```