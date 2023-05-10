[![NuGet](https://img.shields.io/nuget/v/SDSlite.svg?style=flat)](https://www.nuget.org/packages/SDSlite/)
![build-test](https://github.com/predictionmachines/SDSlite/workflows/build-test/badge.svg)Scientific DataSet Lite
=======================

This is a cross platform library for manipulating netCDF, CSV and TSV files.
This is a subset of **Scientific DataSet** [https://www.microsoft.com/en-us/research/project/scientific-dataset/](https://www.microsoft.com/en-us/research/project/scientific-dataset/).

External Libraries
------------------

SDSLite requires a platform dependent library available from [Unidata](https://www.unidata.ucar.edu/software/netcdf/).

### Windows

For Windows go to https://docs.unidata.ucar.edu/netcdf-c/current/winbin.html and download the version of netCDF4 (without DAP) corresponding to your machine, either 32 or 64 bit.
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


### MacOS

Use homebrew [http://brew.sh/](http://brew.sh/) to install netcdf:

`brew install netcdf`

### LICENCE

You can find license details in Licence.txt file provided with this project or online at [https://github.com/predictionmachines/SDSlite/blob/master/Licence.txt](https://github.com/predictionmachines/SDSlite/blob/master/Licence.txt).
