## The right tool for the right job. | Part 1: Log classifier in C# ## 

This repo contains the full code of my [blog entry](https://code.darkroku12.ovh/6-log-classifier-part-1/).

## Install instructions: ##

1. Clone this repository.
2. Get the `Android.log` file.
3. Run your app with Visual Studio or with dotnet command line.

Getting the `Android.log` file:
1. You can get the `Android.log` file at this [link](https://zenodo.org/record/3227177#.YRqa--VjSUk).
2. Download the file `Android.tar.gz (md5:1a1bac1cf0ea95bc88e296f689f0258f)`.
3. Decompress the log file and add it to the launching path of the application.

__Note:__ If you're using Visual Studio to build & run the project, the launch path is configured to be the root folder, which is the folder containing the `.sln` file.

C# .NET Core is available for Linux and MAC too, take a look at this [link](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial/run).
You can also run the project within your command line (without the need of Visual Studio), please, [see this tutorial](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial/run) provided by Microsoft.

## Running samples: ##

| Method       | Run #1 - Time | Run #2 - Time |
|--------------|---------------|---------------|
| Sequential   | 6313ms        | 6332ms        |
| Parallel.For | 2333ms        | 1737ms        |
| AsParallel   | 1762ms        | 1708ms        |

These results are based on `results.txt` file, located at the root folder of this repository.

## Author:
#### Enmanuel Reynoso | DarkRoku12 | enmarey2012@hotmail.com
