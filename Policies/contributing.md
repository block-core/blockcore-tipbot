Contribution Guidelines
=======================

We try to follow the [.Net core](https://github.com/dotnet/coreclr/blob/master/Documentation/project-docs/contributing.md) guidelines.

## Adding Features

First create an issue or start discussion on existing one. 

Adding Unit Tests
-----------------
Test contributions are more than welcome!
Pick a class that does not have any tests and start testing it.
Have a look at our testing guidelines.
Use other tests for reference.

Typos and small changes
-----------------------
Typos are embarrassing! We will accept most PRs that fix typos. In order to make it easier to review your PR, please focus on a given component with your fixes or on one type of typo across the entire repository. If it's going to take >30 mins to review your PR, then we will probably ask you to chunk it up.

Commit Messages
---------------

Please format commit messages as follows (based on this [excellent post](http://tbaggery.com/2008/04/19/a-note-about-git-commit-messages.html)):

```
Summarize change in 50 characters or less

Provide more detail after the first line. Leave one blank line below the
summary and wrap all lines at 72 characters or less.

If the change fixes an issue, leave another blank line after the final
paragraph and indicate which issue is fixed in the specific format
below.

Fix #42
```

Also do your best to factor commits appropriately, i.e not too large with unrelated
things in the same commit, and not too small with the same small change applied N
times in N different commits. If there was some accidental reformatting or whitespace
changes during the course of your commits, please rebase them away before submitting
the PR.

DOs and DON'Ts
--------------

* **DO** follow our coding style.
* **DO** follow our code commenting policy and guidelines.
* **DO** give priority to the current style of the project or file you're changing even if it diverges from the general guidelines.
* **DO** include tests when adding new features. When fixing bugs, start with
  adding a test that highlights how the current behavior is broken.
* **DO** keep the discussions focused. When a new or related topic comes up
  it's often better to create new issue than to side track the discussion.
* **DO** blog and tweet (or whatever) about your contributions, frequently!
* **DON'T** send PRs for style changes. 
* **DON'T** surprise us with big pull requests. Instead, file an issue and start
  a discussion so we can agree on a direction before you invest a large amount
  of time.
* **DON'T** commit code that you didn't write. If you find code that you think is a good fit to add, file an issue and start a discussion before proceeding.
