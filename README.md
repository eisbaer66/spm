
# SourcePawnManager (spm)

## SourcePawnManager CLI

This CLI tool is used to install, restore, update and remove includes/dependecies

### Global options

These options can be used with any command

|Option                                    |Shorthand  |Type    |Default     |Description
|------------------------------------------|-----------|--------|------------|------------------------------------
|`--working-directory <working-directory>` |`-w`       |string  |.           |directory containing the spm.json
|`--dry-run`                               |`-d`       |bool    |            |executes a dry-run without actually installing/restoring/updating/removing
|`--github-token <github-token>`           |`-t`       |string  |            |GitHub token used to authenticate API calls
|                                          |`-v`       |bool    |            |logs: shows warnings
|                                          |`-vv`      |bool    |            |logs: shows information
|                                          |`-vvv`     |bool    |            |logs: shows debug
|                                          |`-vvvv`    |bool    |            |logs: shows verbose
|`--version`                               |           |bool    |            |Show version information
|`--help`                                  |`-?`, `-h` |bool    |            |Show help and usage information

#### working directory

spm will look for spm.json in the current working directory. If spm.json is located in an other directory, use `--working-directory <working-directory>` to specify this directory.

#### dry run

spm will not change/create/delete any files, if `--dry-run` is specified.  
Can be used to test to see what a certain command would do, if  executed without `--dry-run`.

#### GitHub token

will be used to authenticate API calls, increasing the rate limit. Unauthorized calles are limited by GitHub to 60 requests per hour.  
If [git-credential-manager](https://github.com/GitCredentialManager/git-credential-manager/) is installed, the token will be saved there after the first successful API call. So the token does not have to be provided for further spm executions.

#### Logging levels

By default only errors and the result of the executed command will be output. To output additional logging messages use the `-v` flags. Specifying more v's will increase the level at which log messages will be output (`-vvvv` being the most verbose).

### `list` command
aliases: `ls`, `l`

lists all already installed includes. Reads the spm.json and outputs all installed includes with the specified Versionrange.

The includes are numbered in the output. These numbers can be used with `spm remove` to easily remove already installed includes.

Usage: `spm list`

### `install` command
aliases: `i`

installs a include file. Adding it to the spm.json and the spm.lock.json.


Following sub-commands are available when `install`ing:

#### `install github-tag-file` command
aliases: `i ghtf`

installs include-file from github identified by a git-tag.

Usage: `spm install github-tag-file <owner> <repository> <versionRange> <assetName> [options]`  

Example: `spm install github-tag-file nosoop tf2attributes 1.* tf2attributes.inc`  
installs the file tf2attributes.inc from the latest 1.* version of https://github.com/nosoop/tf2attributes/tags

|Argument         |Description
|-----------------|------------------------------------
|`<owner>`        |owner of the github-repository
|`<repository>`   |repository-name on github
|`<versionRange>` |versionRange used when updating this include
|`<assetName>`    |name of the include-asset in the GitHub-tag

|Options                             |Shorthand |Default                                          |Description
|------------------------------------|----------|-------------------------------------------------|------------------------------------
|`--download-path <download-path>`   |`-d`      |`/include/<assetName>`                           |path where the include-file will be stored after download
|`--version-reg-ex <version-reg-ex>` |`-r`      |`(\d+(?:\.\d+)?(?:\.\d+)?(?:\.\d+)?)[-+]?(\S*)?` |Regular-Expression used to parse the version from the git-tag

#### `install github-tag-zip` command
aliases: `i ghtz`

installs include-file from a zip on github identified by a git-tag.

Usage: `spm install github-tag-zip <owner> <repository> <versionRange> <assetName> <fileInZip> [options]`  

Example: `spm install github-tag-zip nosoop SM-TFCustAttr 8.* package.zip scripting/include/tf_custom_attributes.inc`  
installs the zip-file package.zip and extracts the file scripting/include/tf_custom_attributes.inc from the latest 8.* version of https://github.com/nosoop/SM-TFCustAttr/tags

|Argument         |Description
|-----------------|------------------------------------
|`<owner>`        |owner of the github-repository
|`<repository>`   |repository-name on github
|`<versionRange>` |versionRange used when updating this include
|`<assetName>`    |name of the include-asset in the GitHub-tag
|`<fileInZip>`    |path of the file within the downloaded zip

|Options                             |Shorthand |Default                                          |Description
|------------------------------------|----------|-------------------------------------------------|------------------------------------
|`--download-path <download-path>`   |`-d`      |`/include/<assetName>`                           |path where the include-file will be stored after download and extraction
|`--version-reg-ex <version-reg-ex>` |`-r`      |`(\d+(?:\.\d+)?(?:\.\d+)?(?:\.\d+)?)[-+]?(\S*)?` |Regular-Expression used to parse the version from the git-tag

#### `restore` command
aliases: `r`

restores installed includes according to spm.json and spm.lock.json.  
Will not update versions while the include-version is locked.

Used to bring the includes to the same version as your team/contributers.

Usage: `spm restore`

#### `update` command
aliases: `u`

updates installed includes to the latest version according to spm.json.  
Will ignore locks and update to the highest available version within the specified VersionRange for each include.

Usage: `spm update`

#### `remove` command
aliases: `rm`

removes a already installed include from disk, spm.json and spm.lock.json.

Usage: `spm update <Id>`

The `<Id>` can be either the Id or the Index of the include-file (both are output by `spm list`).

#### `show-license` command
aliases: `sl`

displays copyright/license information.

Usage: `spm show-license`


### spm.lock.json
`spm install` and `spm update` will lock the installed/updated version of the include-file, by writing them into a file called spm.lock.json.  
This file is used to prevent unintended updates of the installed include-file version.  
When executing `spm restore` include-files will be installed with the locked version, to make sure all contributers use the same version.  
`spm update` is only used to update the used version of installed include-files (according to the VersionRange specified for the include-file) and write the new version to spm.lock.json.

Both spm.json and spm.lock.json should be added to your versioncontrol, to make sure other contributers will work with the same versions of include-files.

### VersionRanges
When installing a include you can specify a [range](https://docs.microsoft.com/en-us/nuget/concepts/package-versioning#version-ranges) of allowed versions.

|Notation  |Applied rule  |Description
|----------|--------------|-----------
|1.0       |x ≥ 1.0       |Minimum version, inclusive
|(1.0,)    |x > 1.0       |Minimum version, exclusive
|[1.0]     |x == 1.0      |Exact version match
|(,1.0]    |x ≤ 1.0       |Maximum version, inclusive
|(,1.0)    |x < 1.0       |Maximum version, exclusive
|[1.0,2.0] |1.0 ≤ x ≤ 2.0 |Exact range, inclusive
|(1.0,2.0) |1.0 < x < 2.0 |Exact range, exclusive
|[1.0,2.0) |1.0 ≤ x < 2.0 |Mixed inclusive minimum and exclusive maximum version
|(1.0)     |invalid       |invalid


## GitHub Docker Action

This action restores include files defined in spm.json.

### Example usage
To use the GitHub Action, you'll need to add it as a step in your [Workflow file](https://help.github.com/en/actions/automating-your-workflow-with-github-actions).   
`restore` is the default action for the GitHub Action, so no further configuration is needed:
```yaml
on: push

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v1
      - uses: eisbaer66/spm@v1
```


### Input Parameters
You can set any or all of the following input parameters:

|Name                |Type    |Required? |Default                     |Description
|--------------------|--------|----------|----------------------------|------------------------------------
|`action`            |string  |no        |restore                     |The action to execute. Can be either "restore" or "update".
|`verbosity`         |int     |no        |0                           |The verbosity level used when logging. Examples, "0" through "4".
|`working-directory` |string  |no        |.                           |The directory containing the spm.json. Examples, "." or "./src".
|`github-token`      |string  |no        |                            |GitHub token used to authenticate API calls.

```yaml
on: push

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v1
      - uses: eisbaer66/spm@v1
        with:
          action: update
          working-directory: ./src
          verbosity: 4
          github-token: ${{ secrets.GITHUB_TOKEN }}
```

### Output Variables
spm exposes some output variables, which you can use in later steps of your workflow. To access the output variables, you'll need to set an `id` for the spm step.

```yaml
steps:
  - id: spm
    uses: eisbaer66/spm@v1
    
  - if: steps.spm.outputs.dependency-count != 0
    run: |
      echo "${{ steps.spm.outputs.message }}"
  
  - if: steps.spm.outputs.exit-code != 0
    run: |
      echo "restore failed with exit-code ${{ steps.spm.outputs.exit-code }}: ${{ steps.spm.outputs.message }}"
```


|Variable           |Type    |Description
|-------------------|--------|------------------------------------
|`exit-code`        |int     |The exit code of the action.
|`message`          |string  |A message detailing the executed actions.
|`dependency-count` |int     |The count of restored/updated dependencies.


## License
Copyright (C) 2022 icebear <icebear@icebear.rocks>

SourcePawnManager (spm) is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

SourcePawnManager (spm) is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with SourcePawnManager (spm). If not, see <https://www.gnu.org/licenses/>. 