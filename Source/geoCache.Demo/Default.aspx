﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="GeoCache.Demo._Default" %>

<!doctype html>

<html lang="en">
<head runat="server">
    <meta charset="utf-8">
    <title>TrueNorth Geospatial Tile Server</title>
    <meta name="description" content="TrueNorth Geospatial Tile Server">
    <meta name="author" content="BlueToque Software">
    <link rel="stylesheet" href="style.css" type="text/css"/>
    <style type="text/css">
            html, body, #map {
                margin: 0;
                width: 100%;
                height: 100%;
            }

            #text {
                position: absolute;
                bottom: 1em;
                left: 1em;
                width: 512px;
                z-index: 20000;
                background-color: white;
                padding: 0 0.5em 0.5em 0.5em;
            }
        </style>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/openlayers/2.13.1/OpenLayers.js" type="text/javascript"></script>
    <!--[if lt IE 9]>
        <script src="http://html5shiv.googlecode.com/svn/trunk/html5.js"></script>
    <![endif]-->
</head>
<body>

    <div id="map"></div> 
    <div id="text">
        <h1 id="title">TrueNorth Geospatial Tile Server</h1>
        <div id="docs">
            <p>This server caches data from GeoBC's WMS server. It contains the TRIM data set as well a roads, water, landmarks, and contour data</p>
        </div>
    </div>

    <script defer="defer" type="text/javascript">

        var map = new OpenLayers.Map("map",
            {
                maxExtent: new OpenLayers.Bounds(-20037508.34, -20037508.34, 20037508.34, 20037508.34),
                numZoomLevels: 18,
                maxResolution: 156543.0339,
                allOverlays: true,
                units: 'm',
                projection: "EPSG:900913",
                displayProjection: new OpenLayers.Projection("EPSG:4326"),
                controls: [
                    new OpenLayers.Control.Navigation(),
                    new OpenLayers.Control.KeyboardDefaults(),
                    new OpenLayers.Control.PanZoomBar(),
                    //new OpenLayers.Control.Zoom(),
                    new OpenLayers.Control.Scale(),
                    new OpenLayers.Control.LayerSwitcher(),
                    new OpenLayers.Control.Attribution(),
                    new OpenLayers.Control.TouchNavigation()]
        });

        // create sphericalmercator layers
        var osmLayer = new OpenLayers.Layer.OSM("OpenStreetMap");

        var options = {
            attribution: {
                title: "Provided by GeoBC",
                href: "http://www.osgeo.org/"
            }
        };
        var geoBC = new OpenLayers.Layer.WMS("GeoBC",
                        "/WMS?",
                        {
                            layers: 'geobc',
                            transparent: true,
                            format: 'image/png',
                            srs: "EPSG:900913"
                        },
                        {
                            attribution: "&copy; <a href='http://geobc.gov.bc.ca//'>GeoBC</a>",
                            isBaseLayer: false
                        }
            );
        map.addLayers([osmLayer,geoBC]);

        var centerLL = new OpenLayers.LonLat(-123, 49.3);
        var centerM = centerLL.transform(new OpenLayers.Projection("EPSG:4326"), map.getProjectionObject());
        map.setCenter(centerM, 12);

        //map.zoomToMaxExtent();
        //if (!map.getCenter()) map.zoomToMaxExtent();
    </script>
</body>
</html>
