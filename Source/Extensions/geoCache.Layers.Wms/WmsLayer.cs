﻿//
// File: WmsLayer.cs
//
// Author:
//   Steinar Herland <steinar.herland@gecko.no>
//
// Copyright (C) 2006-2007 MetaCarta, Inc.
// Copyright (C) 2008 Steinar Herland
// Copyright (C) 2008 Gecko Informasjonssystmer AS (http://www.gecko.no)
// Copyright (C) 2015 Blue Toque Software (http://www.BlueToque.ca)
//
// Licensed under the terms of the GNU Lesser General Public License
// (http://www.opensource.org/licenses/lgpl-license.php)

using System;
using System.Collections.Generic;
using GeoCache.Core;
using GeoCache.Extensions.Base;

namespace GeoCache.Layers.Wms
{
	public class WmsLayer : MetaLayer
	{
		public Uri Url { get; set; }

        public bool Transparent { get; set; }

		public override byte[] RenderTile(ITile tile)
		{
			var wms = new WmsClient(Url, new Dictionary<string, string>
			                             	{
			                             		{"bbox", tile.BBox},
			                             		{"width", tile.Size.Width.ToString()},
			                             		{"height", tile.Size.Height.ToString()},
			                             		{"srs", Srs},
			                             		{"format", Format},
			                             		{"layers", Layers},
                                                {"transparent", Transparent.ToString()}
			                             	});
			return wms.Fetch();
		}

	}
}