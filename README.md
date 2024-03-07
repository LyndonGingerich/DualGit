# The original idea: Parallel refactor and feature workflow
Two Git branches: 
- For cleanup and refactoring for ease of reading, debugging, and testing
- For the actual feature and tests

Basic commands:
- `commit refactor`: Check out the refactor branch, commit, return to previous branch
- `commit feature`: Check out the feature branch, commit
- `update`: Check out the feature branch, merge refactor branch into feature branch
In the end, the refactor branch must be merged before the feature branch.
Though building a tool to disentangle the two if possible also sounds fun...
# Config
While active, `dualgit` uses the YAML config file `.dualgit` in the working directory from which it is called.
`.dualgit` fields:
- `feature` and `refactor`: Names of Git branches. Hereafter, feature refers to the branch name stored under "feature" in .dualgit, and refactor refers to the branch name stored under refactor in .dualgit.
- `base`: An ancestor of both `feature` and `refactor`.
- `split_commits`: A list of commits being cherry-picked by the `split` command, for recovery in case of failure (such as due to conflicts).
# Common behavior
If a command other than `init` is called when `.dualgit` does not exist, the command should fail, warning the user to call `dualgit init` first.
If at any point a command attempts to use either `feature` or `refactor` when it is not defined in `.dualgit`, that command should fail, warning the user to define the required branch first.
Writes of `feature` and `refactor` to `.dualgit` should overwrite previous values.
# Commands
`init`: Create the file `.dualgit`; if it already exists, fail, warning the user that a `.dualgit` workflow is already in progress.
`set`: Take two arguments. The first must be either "feature" or "refactor"; the second is a Git branch name. Write the second argument to `.dualgit` using the first argument as the key.
`commit`: Take any number of arguments. The first must be either `feature` or `refactor`. Check out the branch stored under that key in `.dualgit` (or fail if it is not defined in `.dualgit`), then call `git commit` using the remaining arguments as arguments.
`update`: Check out `feature`, then merge `refactor` into `feature`.
`reset`: Delete `.dualgit`.
# Future
## Implement `split`
If no flags, create a new branch on existing branch "develop" with the name of `feature` with "-split" appended, hereafter referred to as `split` (or fail if that branch already exists); store the list of hashes of commits  hashes under "split_commits" in `.dualgit`; for each hash in `split_commits`, cherry-pick that hash onto `split`; and clear `split_commits`.
Also implement `--abort` and `--continue`.
## Implement `switch`
Take zero or one arguments. If no arguments, check out `feature` if `refactor` is checked out, or `refactor` if `feature` is checked out; otherwise, fail. If one argument, it should be either "refactor" or "feature" to indicate which branch to check out.
## Other
- Ensure that `feature` and `refactor` are branches
- Add tests
