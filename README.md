[![NuGet](https://img.shields.io/nuget/v/SDSlite.svg?style=flat)](https://www.nuget.org/packages/SDSlite/)
![build-test](https://github.com/predictionmachines/SDSlite/workflows/build-test/badge.svg)

Scientific DataSet Lite
=======================

This is a cross platform [.NET](https://dotnet.microsoft.com) library for manipulating netCDF, CSV and TSV files.
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

Sample
------

C# example:

```csharp
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

// download a netCDF file from unidata web site
var client = new System.Net.Http.HttpClient();
var response = await client.GetAsync("https://www.unidata.ucar.edu/software/netcdf/examples/sresa1b_ncar_ccsm3-example.nc");
using (var stream = System.IO.File.OpenWrite("sresa1b_ncar_ccsm3-example.nc")){
    await response.Content.CopyToAsync(stream);
}

// open the file and print some info
using (DataSet ds = DataSet.Open("msds:nc?file=sresa1b_ncar_ccsm3-example.nc&openMode=readOnly")){
    Console.WriteLine(ds);
    Console.WriteLine(ds.Metadata["comment"]);
    var lat = ds.GetData<float[]>("lat");
    Console.WriteLine($"latitude: len={lat.Length}, min={lat.Min()}, max={lat.Max()}");
}
```

Output:

```text
msds:nc?openMode=readOnly&file=c:\Users\***\sresa1b_ncar_ccsm3-example.nc
[1]
DSID: ca6c1f06-f743-4190-9b94-20cf0cbc18f7
[12] ua of type Single (time:1) (plev:17) (lat:128) (lon:256)
[11] time_bnds of type Double (time:1) (bnds:2)
[10] time of type Double (time:1)
[9] tas of type Single (time:1) (lat:128) (lon:256)
[8] pr of type Single (time:1) (lat:128) (lon:256)
[7] plev of type Double (plev:17)
[6] msk_rgn of type Int32 (lat:128) (lon:256)
[5] lon_bnds of type Double (lon:256) (bnds:2)
[4] lon of type Single (lon:256)
[3] lat_bnds of type Double (lat:128) (bnds:2)
[2] lat of type Single (lat:128)
[1] area of type Single (lat:128) (lon:256)

This simulation was initiated from year 2000 of 
 CCSM3 model run b30.030a and executed on 
 hardware cheetah.ccs.ornl.gov. The input external forcings are
ozone forcing    : A1B.ozone.128x64_L18_1991-2100_c040528.nc
aerosol optics   : AerosolOptics_c040105.nc
aerosol MMR      : AerosolMass_V_128x256_clim_c031022.nc
carbon scaling   : carbonscaling_A1B_1990-2100_c040609.nc
solar forcing    : Fixed at 1366.5 W m-2
GHGs             : ghg_ipcc_A1B_1870-2100_c040521.nc
GHG loss rates   : noaamisc.r8.nc
volcanic forcing : none
DMS emissions    : DMS_emissions_128x256_clim_c040122.nc
oxidants         : oxid_128x256_L26_clim_c040112.nc
SOx emissions    : SOx_emissions_A1B_128x256_L2_1990-2100_c040608.nc
 Physical constants used for derived data:
 Lv (latent heat of evaporation): 2.501e6 J kg-1
 Lf (latent heat of fusion     ): 3.337e5 J kg-1
 r[h2o] (density of water      ): 1000 kg m-3
 g2kg   (grams to kilograms    ): 1000 g kg-1
 
 Integrations were performed by NCAR and CRIEPI with support
 and facilities provided by NSF, DOE, MEXT and ESC/JAMSTEC.
 latitude: len=128, min=-88.927734, max=88.927734
```
LICENCE
-------

This project is licensed under MIT.