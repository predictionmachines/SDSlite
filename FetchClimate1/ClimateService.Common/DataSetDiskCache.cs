// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Research.Science.Data.Factory;
using System.Security.Cryptography;
using Microsoft.Research.Science.Data.Climate.Conventions;

namespace Microsoft.Research.Science.Data.Climate
{
    /// <summary>
    /// Stores DataSets copies as csv files with hash names.
    /// </summary>
    public sealed class DataSetDiskCache
    {
        private static TraceSwitch Tracer = new TraceSwitch("DataSetCache", "Filters trace messages related to the DataSet core classes.", "Error");
        private const long maxCacheSize = 1024L * 1024L * 1024L * 2L; // 2 Gb
        private readonly string cacheFolder;
        private const long thresholdActivateCleanUp = 100 * 1024 * 1024; // 100 Mb
        private long lastAccumulatedSize = thresholdActivateCleanUp;
        private bool isCleaningUp = false;
        private bool firstClean = true;

        public DataSetDiskCache(string cacheFolder)
        {
            if (cacheFolder == null)
                throw new ArgumentNullException("cacheFolder");
            this.cacheFolder = Path.GetFullPath(cacheFolder);
            if (!Directory.Exists(this.cacheFolder))
                Directory.CreateDirectory(this.cacheFolder);
        }

        public void Add(DataSet ds, bool isAsync = false)
        {
            if (isAsync)
            {
                WaitCallback wc = new WaitCallback(new Action<object>(
                     dataSet =>
                     {
                         AddToCache((DataSet)dataSet);
                     }
                    ));
                ThreadPool.QueueUserWorkItem(wc, ds);
            }
            else
            {
                AddToCache(ds);
            }
        }

        private void AddToCache(DataSet ds)
        {
            string tempPath = Path.GetTempFileName();
            long sizeOfDS = 0;
            try
            {
                string targetUri = String.Format("msds:csv?file={0}&openMode=create&appendMetadata=true", tempPath);
                ds.Clone(targetUri);

                string hash = null;

                if (ds.Metadata.ContainsKey(Namings.metadataNameHash))
                {
                    hash = (string)ds.Metadata[Namings.metadataNameHash];
                }
                else
                {
                    hash = DataSetDiskCache.ComputeHash(ds);
                }
                string targetPath = Path.Combine(cacheFolder, String.Format("{0}.csv", hash));

                if (File.Exists(targetPath))
                    File.Delete(targetPath);
                File.Move(tempPath, targetPath);
                tempPath = null;
                //File.SetAttributes(targetPath, FileAttributes.ReadOnly);
                sizeOfDS = (new FileInfo(targetPath)).Length;

            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(Tracer.TraceError, "Failed to save cache. " + ex.ToString());
                return;
            }
            finally
            {
                if (tempPath != null)
                    File.Delete(tempPath);
            }

            ScheduleCacheClearCheck(sizeOfDS);
        }

        private void ScheduleCacheClearCheck(long cacheSizeIncrement)
        {
            /****** Scheduling cache clear ********/
            lock (this)
            {
                if (!firstClean && cacheSizeIncrement == 0)
                    return;
                firstClean = false;

                lastAccumulatedSize += cacheSizeIncrement;
                if (!isCleaningUp)
                {
                    if (lastAccumulatedSize >= thresholdActivateCleanUp)
                    {
                        lastAccumulatedSize = 0;
                        isCleaningUp = true;
                        ThreadPool.QueueUserWorkItem(FreeCacheToPermitedSize);
                    }
                }
            }
        }

        public DataSet Get(string hash)
        {
            try
            {
                string targetPath = Path.Combine(cacheFolder, String.Format("{0}.csv", hash));
                if (File.Exists(targetPath))
                {
                    return DataSet.Open(String.Format("msds:csv?file={0}&openMode=readOnly", targetPath));
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(Tracer.TraceError, "Failed to load cache. " + ex.ToString());
            }
            ScheduleCacheClearCheck(0);
            return null;
        }

        public void FullCacheClear()
        {
            try
            {
                foreach (var file in Directory.GetFiles(cacheFolder))
                {
                    FileInfo info = new FileInfo(file);
                    if (info.Attributes != FileAttributes.Normal)
                    {
                        info.Attributes = FileAttributes.Normal;
                    }
                    File.Delete(file);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        private void FreeCacheToPermitedSize(object state)
        {
            try
            {
                long totalSize = 0;
                if (!Directory.Exists(cacheFolder))
                {
                    Directory.Delete(cacheFolder);
                    return;
                }
                foreach (var file in Directory.GetFiles(cacheFolder))
                {
                    FileInfo fi = new FileInfo(file);
                    totalSize += fi.Length;
                }
                if (totalSize > maxCacheSize)
                {
                    Trace.WriteLineIf(Tracer.TraceVerbose, "Cache limit exceed; starting clear.");
                    FileInfo[] files = (new DirectoryInfo(cacheFolder)).GetFiles();
                    Array.Sort(files, (f1, f2) => DateTime.Compare(f1.LastAccessTime, f2.LastAccessTime));

                    int n = 0;
                    for (int i = 0; i < files.Length; i++)
                    {
                        try
                        {
                            long sz = files[i].Length;
                            files[i].Attributes = FileAttributes.Normal;
                            files[i].Delete();
                            n++;
                            totalSize -= sz;
                            if (totalSize <= (maxCacheSize * 0.8))
                                break; // clearing down to 80% of max cache size
                        }
                        catch (FileNotFoundException)
                        {
                            Trace.WriteLineIf(Tracer.TraceInfo, "Cannot remove file " + files[i].FullName + " for it is already deleted");
                        }
                        catch (IOException)
                        {
                            Trace.WriteLineIf(Tracer.TraceInfo, "Cannot remove file " + files[i].FullName + " for it is already opened");
                        }
                    }
                    Trace.WriteLineIf(Tracer.TraceVerbose, n + " cached files deleted");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(Tracer.TraceError, "Cache clear got an exception: " + ex);
            }
            finally
            {
                lock (this)
                {
                    isCleaningUp = false;
                }
            }
        }

        public static string ComputeHash(DataSet ds)
        {
            return ShaHash.CalculateShaHash(ds);
        }
    }
}

