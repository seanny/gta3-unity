# How to contribute to GTA3-Unity #

The basic contribution process is simple:
- Fork the repository.
- Make your changes.
- Open a pull request to merge your changes into the project.

However, there are a few additional considerations:

Our primary focus is completing GTA3’s gameplay and fixing bugs caused by engine limitations. Pull requests outside this scope will generally be rejected unless they include a clear and compelling explanation of why the proposed changes should be accepted.

## Pull request guidelines
Keep each pull request focused on a single feature, fix, or closely related set of changes. Pull requests that attempt to implement many unrelated changes may be rejected because they are too broad.

For example, a focused pull request that implements a third-person camera consistent with GTA3’s gameplay and style is more likely to be reviewed and accepted.

Once you know your pull request number, update CHANGELOG.md with a description of your changes and include the pull request number. Example:
```
    Bugfix #1234: Fixed X bug that caused Y to happen
    Feature #1234: Implmented X feature
```

## Releases
New versions are released when the project maintainers believe enough improvements have accumulated to justify a release. Depending on development activity, this may take anywhere from a week to a year.

## AI Usage Policy
Any code generated or substantially written with the assistance of AI must include a clear in-source disclaimer acknowledging that AI was used.

Contributors may use AI to review, explain, or provide feedback on code without adding an in-source disclaimer, provided the AI does not directly generate or substantially rewrite the submitted code. Contributors should still use common-sense precautions, verify any suggestions, and thoroughly test their changes before submitting them.

The contributor who submits AI-assisted code remains fully responsible for reviewing, understanding, testing, and maintaining it. If the code introduces bugs, regressions, security issues, or other problems, responsibility rests with the contributor who implemented and submitted the code, not with the AI tool used to produce or review it.

In-source disclaimers for AI-assisted code should use the following format: 

`AI-ASSISTED: [Company] [Tool Name] - [Month Name] [Day], [Year]`
Example:`AI-ASSISTED: OpenAI Codex - July 11, 2026`