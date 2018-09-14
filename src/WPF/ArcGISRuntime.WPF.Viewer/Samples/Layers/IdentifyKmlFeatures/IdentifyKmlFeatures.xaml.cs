// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.IdentifyKmlFeatures
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Identify KML features",
        "Layers",
        "Identify KML features and show popups.",
        "")]
    public partial class IdentifyKmlFeatures
    {
        // Hold a reference to the KML layer for use in identify operations.
        private KmlLayer _forecastLayer;

        public IdentifyKmlFeatures()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Set up the basemap.
            MyMapView.Map = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Create the dataset.
            KmlDataset dataset = new KmlDataset(new Uri("https://www.wpc.ncep.noaa.gov/kml/noaa_chart/WPC_Day1_SigWx.kml"));

            // Create the layer from the dataset.
            _forecastLayer = new KmlLayer(dataset);

            // Add the layer to the map.
            MyMapView.Map.OperationalLayers.Add(_forecastLayer);

            // Zoom to the extent of the layer.
            await _forecastLayer.LoadAsync();
            await MyMapView.SetViewpointGeometryAsync(_forecastLayer.FullExtent, 10);

            // Listen for taps to identify features.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing popups.
            MyMapView.DismissCallout();

            // Perform identify on the KML layer and get the results.
            IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_forecastLayer, e.Position, 15, false);

            // Return if there are no results that are KML placemarks.
            if (!identifyResult.GeoElements.OfType<KmlPlacemark>().Any())
            {
                return;
            }

            // Get the first identified feature that is a KML placemark
            KmlPlacemark firstIdentifiedPlacemark = identifyResult.GeoElements.OfType<KmlPlacemark>().First();

            // Create a browser to show the feature popup HTML.
            WebBrowser browser = new WebBrowser
            {
                Width = 400,
                Height = 100
            };
            browser.NavigateToString(firstIdentifiedPlacemark.BalloonContent);

            // Create and show the callout.
            MyMapView.ShowCalloutAt(e.Location, browser);
        }
    }
}