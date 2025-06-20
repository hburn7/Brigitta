# Brigitta

Brigitta is an IRC chat client designed specifically for referees of the rhythm game 
[osu!](https://osu.ppy.sh/home). *Unfortunately, I do not have the bandwidth to maintain this like I used to. But, plenty of people still choose Brigitta as their client of choice even today. Hope you enjoy!*

# Run The Application
**Prerequisites:**
- [.NET 9.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) *Look for ".NET Runtime 9.x.x" on this page*

**Run:** 
- Visit the latest [Release](https://github.com/hburn7/Brigitta/releases) and download the .zip file for your platform.

| Platform | Instructions                                                                                      | Special Notes                                                                                                                                                    |
|----------|---------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Windows  | Extract the downloaded folder and<br/>run `BrigittaBlazor.exe`.                                         | *None*                                                                                                                                                           |
| MacOS    | Right click on the "BrigittaBlazor"<br/>executable and click Open.                                      | If you get an error stating that <br>the executable cannot be run,<br/>please click "Show Finder" and then <br/>`Right Click` -> `Run` any executables<br/> you see. This will need to be done each update. |
| Linux    | cd into the downloaded folder<br/>and run `./BrigittaBlazor`. Ensure<br/>it has executable permissions. | *None*                                                                                                                                                           |
- All platforms will need to navigate to `localhost:5000` in a browser in order to use the application.
- Checkout my [Discord Server](https://discord.gg/TjH3uZ8VgP) and get the `Brigitta` role in `#roles` to gain special access to developer announcements and know exactly when new releases happen!

## Contributing

Please follow the instructions below when considering making a contribution to the project:

- First and foremost, this is my senior capstone project for my undergraduate degree. I do not expect PRs or any direct code contributions of any kind.
- Please check for any existing issues if you wish to make a PR. I likely will not accept PRs that add additional features without my previous approval.
- Ensure any PRs made have a very detailed description as to what the code does. Ensure all test cases pass.

## Building From Source

**Prerequisites:**
- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

Clone the repo (pick one):
- `git clone https://github.com/hburn7/Brigitta.git`
- `gh repo clone hburn7/Brigitta`

Target and build:
- `cd Brigitta`
- `cd BrigittaBlazor`
- `dotnet run`

<!-- Run test cases:
- `cd BrigittaTests` (run from repo's root directory)
- `dotnet test`
- You should see a result like this. The number of tests is likely to change, but there should never be any failed tests.

![Example test result where all tests have passed](https://user-images.githubusercontent.com/38370573/192799897-02f5c0a3-f5ab-4bb7-bd53-ac3fd589a91d.jpeg) -->

## Supplementary Note
If you have concerns, please make an issue on this project and I will respond to it very quickly. I hope you enjoy using Brigitta :)
