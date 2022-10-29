# Brigitta

Brigitta is an IRC chat client designed specifically for referees of the rhythm game 
[osu!](https://osu.ppy.sh/home).

# Run The Application
**Prerequisites:**
- [.NET 6.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) *Look for ".NET Runtime 6.x.x" on this page*

**Run:** 
- Visit the latest [Release](https://github.com/hburn7/BanchoSharp/releases) and download the .zip folder for your platform.

| Platform | Instructions                                                                                      | Special Notes                                                                                                                                                    |
|----------|---------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Windows  | Extract the downloaded folder and<br/>run `Brigitta.exe`.                                         | *None*                                                                                                                                                           |
| MacOS    | Right click on the "Brigitta"<br/>executable and click Open.                                      | If you get an error stating that <br>the executable cannot be run,<br/>please click "Show Finder" and then <br/>Right Click -> Run any executables<br/> you see. |
| Linux    | cd into the downloaded folder<br/>and run `./Brigitta`. Ensure<br/>it has executable permissions. | *None*                                                                                                                                                           |

## Contributing

Please follow the instructions below when considering making a contribution to the project:

- First and foremost, this is my senior capstone project for my undergraduate degree. I do not expect PRs or any direct code contributions of any kind.
- Please check for any existing issues if you wish to make a PR. I likely will not accept PRs that add additional features without my previous approval.
- Ensure any PRs made have a very detailed description as to what the code does. Ensure all test cases pass.

## Building From Source

**Prerequisites:**
- [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

Clone the repo (pick one):
- `git clone https://github.com/hburn7/Brigitta.git`
- `gh repo clone hburn7/Brigitta`

Target and build:
- `cd Brigitta` (cd into the newly downloaded "Brigitta" folder)
- `cd Brigitta` (cd again, now you are in the primary project folder)
- `dotnet run`

Run test cases:
- `cd BrigittaTests` (run from repo's root directory)
- `dotnet test`
- You should see a result like this. The number of tests is likely to change, but there should never be any failed tests.

![Example test result where all tests have passed](https://user-images.githubusercontent.com/38370573/192799897-02f5c0a3-f5ab-4bb7-bd53-ac3fd589a91d.jpeg)

Some things to note:
- While the app is in development, the primary window on launch may change. I will not keep this consistent until the app is fully ready for publishing. **To change the main window on application startup**, go to `App.axaml.cs` and change the following code:
```cs
// Launches a new "PrimaryDisplay" window on launch
desktop.MainWindow = new PrimaryDisplay
{
    DataContext = new PrimaryDisplayViewModel()
};

// Change the below to any window you'd like. Make sure
// to assign the correct DataContext (it's always the corresponding ViewModel)

// desktop.MainWindow = new Login
// {
// 	DataContext = new LoginViewModel()
// };
```

## Supplementary Note
If you have concerns, please make an issue on this project and I will respond to it very quickly. Please do not unnecessarily create issues. I hope you enjoy using Brigitta :)