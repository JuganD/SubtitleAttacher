# Subtitle Attacher
Subtitle Attacher is a simple program to rename *presumably* a subtitle file to match a video file name. Shell script included to execute it with right click from Windows Explorer! It also works with the new Windows 11 right click menu! Tested on Win11 21H2 22000.71 Dev.

## How to
1. Either compile it from source or download already compiled binaries. Source project is currently targeting .NET Core 3.1, tho I assume it will work with no problems with other .NET Core versions.
2. Add the program to your right click menu by moving the file `InstallAttacher.bat` right next to `SubtitleAttacher.exe` (in the same folder!) and execute the batch file. This will add the command `Attach Subtitle` to your right click menu! You can always remove it by executing the other batch file - `UninstallAttacher.bat`.
3. That's it! Select both files (video file and subtitle file), right click and click on "Attach Subtitle" menu entry. This will change the subtitle file's name to match the video file name.

## Notes
This application started as a mutex playground and turned into something I actually use quite often. Call me old fashioned for downloading subtitle files, but I watch tons of old movies that streaming platforms do not provide and this application turned out to be very useful. Enjoy!