# SsisXmlDiffHelper

Console application to extract `Data Flow Task` and `Execute SQL Task` information from SSIS `dtsx` files to a more user-friendly JSON format.
Run the application in the folder containing the `dtsx` files to generate the original state and commit it. Once you're done with the changes, run the app again and commit the files. This way you can easily compare the diffs between the generated JSON files and ensure SQL scripts and referred objects were not changed.

## Dependencies

Target Framework: .NET 6.0

## Usage

### First-time setup
1. Set up your publish profile to deploy to your local machine with the following options (for ease-of-use):
![image](https://user-images.githubusercontent.com/102298186/178027590-5882ebba-ed90-4550-884f-f54fa9ec9fcb.png)
1. Put the executable in the directory with the `dtsx` files and run it on the desired branch (this will probably be your `main` or `master`) to generate the starting state.
1. Commit the generated JSON files and log file to this branch.
1. Check out the branch with the changes or if you haven't started, do the required changes to the SSIS packages.
1. Run the `SsisXmlDiffHelper.exe` again and compare the diffs.
1. Commit or do more changes and include the generated files in your pull request.

### Regular usage
Just be sure to run the executable and include it in your pull request.

## Notes

The executable generates files in the format of `ssis_package.dtsx.json` and a log file called `SsisXmlDiffHelperLog.json` which holds useful info like generated time, version etc.

As the executable in self-contained form is around 14 MB, I'm not sure if it's a good idea to push it to the repo - just exclude it from the project.

TODO: make the app into a pre-commit hook which would automate the report generation
