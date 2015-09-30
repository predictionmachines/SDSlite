Scientific DataSet Lite
=======================

This is a cross platform library for manipulating netCDF, CSV and TSV files.
This is a subset of **Scientific DataSet** [http://research.microsoft.com/en-us/projects/sds/](http://research.microsoft.com/en-us/projects/sds/) and [https://sds.codeplex.com/](https://sds.codeplex.com/).

External Libraries
------------------

SDSLite requires a platform dependent library available from [http://www.unidata.ucar.edu/software/netcdf/docs/getting_and_building_netcdf.html](http://www.unidata.ucar.edu/software/netcdf/docs/getting_and_building_netcdf.html).

### Windows

For Windows go to [http://www.unidata.ucar.edu/software/netcdf/docs/winbin.html](http://www.unidata.ucar.edu/software/netcdf/docs/winbin.html) and download the version of netCDF4 (without DAP) corresponding to your machine, either 32 or 64 bit. These are
currently: [http://www.unidata.ucar.edu/downloads/netcdf/ftp/netCDF4.3.3.1-NC4-32.exe](http://www.unidata.ucar.edu/downloads/netcdf/ftp/netCDF4.3.3.1-NC4-32.exe) or [http://www.unidata.ucar.edu/downloads/netcdf/ftp/netCDF4.3.3.1-NC4-64.exe](http://www.unidata.ucar.edu/downloads/netcdf/ftp/netCDF4.3.3.1-NC4-64.exe)
When you install this library select the option to add its location to your system PATH, so that SDSLite can find it.

### Linux

For Linux install pre-built netCDF-C libraries. For example on Ubuntu:

`sudo apt-get install libnetcdf-dev`

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
