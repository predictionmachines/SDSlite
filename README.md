[![NuGet](https://img.shields.io/nuget/v/SDSlite.svg?style=flat)](https://www.nuget.org/packages/SDSlite/)
![build-test](https://github.com/predictionmachines/SDSlite/workflows/build-test/badge.svg)Scientific DataSet Lite
=======================

This is a cross platform library for manipulating netCDF, CSV and TSV files.
This is a subset of **Scientific DataSet** [https://www.microsoft.com/en-us/research/project/scientific-dataset/](https://www.microsoft.com/en-us/research/project/scientific-dataset/) and [https://sds.codeplex.com/](https://sds.codeplex.com/).

External Libraries
------------------

SDSLite requires a platform dependent library available from [http://www.unidata.ucar.edu/software/netcdf/docs/getting_and_building_netcdf.html](http://www.unidata.ucar.edu/software/netcdf/docs/getting_and_building_netcdf.html).

### Windows

For Windows go to [http://www.unidata.ucar.edu/software/netcdf/docs/winbin.html](http://www.unidata.ucar.edu/software/netcdf/docs/winbin.html) and download the version of netCDF4 (without DAP) corresponding to your machine, either 32 or 64 bit.
As of May 2023 these are: https://downloads.unidata.ucar.edu/netcdf-c/4.9.2/netCDF4.9.2-NC4-64.exe or https://downloads.unidata.ucar.edu/netcdf-c/4.9.2/netCDF4.9.2-NC4-64.exe.

The Scientific DataSet library looks for `netcdf.dll` file in the following locations:
- `LIBNETCDFPATH` environment variable if it contains full path of the `netcdf.dll` file;
- Current directory;
- In the same directory as the `ScientificDataSet.dll` assembly;
- PATH environment variable;
- Default installation directory of netCDF4.

### Linux

For Linux install pre-built netCDF-C libraries. For example on Ubuntu:

`sudo apt-get install libnetcdf-dev`

If `LIBNETCDFPATH` environment variable contains full path of the `.so*` file, this will be used.
Otherwise, standard Linux library search for `libnetcdf.so` will be used.

### MacOS

Use homebrew [http://brew.sh/](http://brew.sh/) to install netcdf:

`brew install netcdf`

Compilation
-----------

### Windows

Use Visual Studio to build the source files, the Community Edition of Visual Studio should be sufficient [https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx).
The model requires additional packages that are included in the packages directory, it is not necessary to use NuGet.

  * DynamicInterop.0.7.4 - to bind from C# to the netCDF library;
  * NUnit.2.6.4 - for running the unit tests.

### Linux

For Linux, MonoDevelop [http://www.monodevelop.com/](http://www.monodevelop.com/) is able to build the solution and it can run under Mono [http://www.mono-project.com/](http://www.mono-project.com/).
See the installation instructions at [http://www.mono-project.com/docs/getting-started/install/linux/](http://www.mono-project.com/docs/getting-started/install/linux/).

### MacOS

For MacOS, Mono is also available - but unfortunately only in 32bit mode as a package. See here: [http://www.mono-project.com/docs/compiling-mono/mac/](http://www.mono-project.com/docs/compiling-mono/mac/).

### LICENCE

You can find license details in Licence.txt file provided with this project or online at [https://github.com/predictionmachines/SDSlite/blob/master/Licence.txt](https://github.com/predictionmachines/SDSlite/blob/master/Licence.txt).
