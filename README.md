# BackupBox
---
This is a simple backup tool that can be used to backup data from programs like a HiPOS, HiMarket, and Unipro.

It works on Avalonia, so it should work on Windows, Linux, and MacOS.

So, if you want to backup your data, you can use this tool.

---
## How to build
1. Clone the repository
2. Open folder in terminal
   ```bash
    cd BackupBox
    ```
3. Run the following command for Windows
    ```bash
    dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true
    ```
    Run the following command for Linux
    ```bash
    dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true
    ```
    Run the following command for MacOS
    ```bash
    dotnet publish -r osx-x64 -c Release /p:PublishSingleFile=true
    ```
4. The executable will be in the `bin/Release/net{runtime-version}/{platform}/publish` folder
