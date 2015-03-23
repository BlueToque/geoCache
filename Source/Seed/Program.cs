﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GeoCache.Core;
using GeoCache.Extensions.Base;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Seed
{
    class Program
    {
        static OsmLayer osmLayer;

        /// <summary>
        /// Seed the tile cache
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=tntilecache;AccountKey=vskhSwFKN1z4hZ+LOowQbtruPLeaUZoRMoYfPYJstty+mnNbwZwEbi5T97jRUnWRFufPVE3GZPHsZaRFzUgooQ==";

            osmLayer = new OsmLayer(20)
            {
                Name = "geobc",
                BBox = new BBox("-20037508.3428,-20037508.3428,20037508.3428,20037508.3428"),
                Srs = "EPSG:3857",
                Size = new Size(256, 256),
                ExtentType = ExtentType.Strict,
                MapBBox = new BBox("-15691595.4222,5802989.4975,-12160095.8963,8880086.82382"),
                Url = new Uri("http://tncache.azurewebsites.net/Tiles")
            };

            osmLayer.Resolutions = osmLayer.BBox.GetResolutions(20, osmLayer.Size);

            int zoomStart = 1;
            int zoomEnd = 18;
            int xStart = 0;
            bool reverse = true;

            if (reverse)
            {
                for (int zoom = zoomEnd; zoom >= zoomStart; zoom--)
                    DoZoom(xStart, zoom);
            }
            else
            {
                for (int zoom = zoomStart; zoom <= zoomEnd; zoom++)
                    DoZoom(xStart, zoom);
            }
            Console.WriteLine("Completed");
        }

        private static void DoZoom(int xStart, int zoom)
        {
            int numTiles = (int)Math.Pow(2, zoom) - 1;
            Console.WriteLine("Zoom {0}, {1} tiles", zoom, numTiles);

            for (int x = xStart; x < numTiles; x++)
            {
                Parallel.For(0, numTiles, y =>
                {
                    DoWork(new Tile(osmLayer, x, y, zoom));
                });

            }
        }

        public static void DoWork(ITile tile)
        {
            try
            {
                if (!osmLayer.MapBBox.Contains(tile.Bounds.MinX, tile.Bounds.MinY))
                    return;
                if (Exists(tile))
                    return;

                Console.WriteLine("Retrieve {0}", tile.ToString());
                Uri uri = new Uri(string.Format("{0}/{1}/{2}/{3}/{4}.png", osmLayer.Url, osmLayer.Name, tile.Z, tile.X, tile.Y));
                int retryCount = 3;
                while (retryCount-- > 0)
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                            wc.DownloadData(uri);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Error downloading {0} on try {1}\r\n{2}", uri, 3 - retryCount, ex);
                    }
                }
                Console.WriteLine("Done {0}", tile.ToString());
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error on {0}:\r\n{1}", tile, ex);
                Console.WriteLine("Error on {0}:\r\n{1}", tile, ex);
            }
        }

        static string ConnectionString { get; set; }

        static CloudStorageAccount StorageAccount { get; set; }

        static CloudBlobClient Client { get; set; }

        static CloudBlobContainer Container { get; set; }

        static CloudBlobContainer GetContainer(string containerName)
        {
            if (StorageAccount == null)
                StorageAccount = CloudStorageAccount.Parse(ConnectionString);

            if (Client == null)
                Client = StorageAccount.CreateCloudBlobClient();

            if (Container == null)
                Container = Client.GetContainerReference(containerName);

            return Container;
        }

        static bool Exists(ITile tile)
        {
            CloudBlobContainer container = GetContainer(tile.Layer.Name);
            string name = GetFileName(tile);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(name);
            return blockBlob.Exists();
        }

        static string GetFileName(ITile tile)
        {
            return string.Join("/", new[]
        	{
        		tile.Layer.Name,
        		string.Format("{0:00}", tile.Z),
        		string.Format("{0:000}", Convert.ToInt32(tile.X / 1000000)),
        		string.Format("{0:000}", (Convert.ToInt32(tile.X) / 1000) % 1000),
        		string.Format("{0:000}", (Convert.ToInt32(tile.X) % 1000)),
        		string.Format("{0:000}", Convert.ToInt32(tile.Y / 1000000)),
        		string.Format("{0:000}", (Convert.ToInt32(tile.Y / 1000) % 1000)),
        		string.Format("{0:000}.{1}", (Convert.ToInt32(tile.Y) % 1000), tile.Layer.Extension)
        	});
        }

        static void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
                Console.WriteLine("Error:\r\n{0}", e.Error);
            if (e.UserState == null)
                return;
            Tile t = e.UserState as Tile;
            if (t == null)
                return;
            Console.WriteLine("Done {0}", t.ToString());
        }

        static void Test()
        {
            Tile testTile = new Tile(osmLayer, 5182, 21567, 15);
            byte[] b = osmLayer.RenderTile(testTile);

            using (var ms = new MemoryStream(b))
            {
                Image testImage = Image.FromStream(ms);
                testImage.Save("c:\\Temp\\x.png");
            }
            if (!osmLayer.MapBBox.Contains(testTile.Bounds.MinX, testTile.Bounds.MinY))
                return;

        }
    }
}
