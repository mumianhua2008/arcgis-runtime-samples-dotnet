﻿using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates performing a geocode by submitting values for multiple address fields to a local locator.
    /// </summary>
    /// <title>Geocoding</title>
    /// <category>Offline</category>
    public partial class OfflineGeocoding : UserControl
    {
        private const string LOCATOR_PATH = @"..\..\..\..\..\samples-data\locators\san-diego\san-diego-locator.loc";

        private LocalLocatorTask _locatorTask;

        /// <summary>Construct Offline Geocoding sample control</summary>
        public OfflineGeocoding()
        {
            InitializeComponent();

			mapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-13044000, 3855000, -13040000, 3858000, SpatialReferences.WebMercator));

            SetupRendererSymbols();
        }

        // Setup marker symbol and renderer
        private async void SetupRendererSymbols()
        {
            var markerSymbol = new PictureMarkerSymbol() { Width = 48, Height = 48, YOffset = 24 };
			try
			{
				await markerSymbol.SetSourceAsync(
					new Uri("pack://application:,,,/ArcGISRuntimeSDKDotNet_DesktopSamples;component/Assets/RedStickpin.png"));
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
            graphicsLayer.Renderer = new SimpleRenderer() { Symbol = markerSymbol, };
        }

        // Geocode input address and add result graphics to the map
        private async void FindButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;
                listResults.Visibility = Visibility.Collapsed;
                graphicsLayer.GraphicsSource = null;

                // Street, City, State, ZIP
                Dictionary<string, string> address = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(InputAddress.Text))
                    address.Add("Street", InputAddress.Text);
                if (!string.IsNullOrEmpty(City.Text))
                    address.Add("City", City.Text);
                if (!string.IsNullOrEmpty(State.Text))
                    address.Add("State", State.Text);
                if (!string.IsNullOrEmpty(Zip.Text))
                    address.Add("ZIP", Zip.Text);

                if (_locatorTask == null)
                    _locatorTask = await Task.Run<LocalLocatorTask>(() => new LocalLocatorTask(LOCATOR_PATH));

                var candidateResults = await _locatorTask.GeocodeAsync(
                    address, new List<string> { "Match_addr" }, mapView.SpatialReference, CancellationToken.None);

                graphicsLayer.GraphicsSource = candidateResults
                    .Select(result => new Graphic(result.Location, new Dictionary<string, object> { { "Locator", result } }));

                await mapView.SetViewAsync(ExtentFromGraphics().Expand(2));
            }
            catch (AggregateException ex)
            {
                var innermostExceptions = ex.Flatten().InnerExceptions;
                if (innermostExceptions != null && innermostExceptions.Count > 0)
                    MessageBox.Show(string.Join(" > ", innermostExceptions.Select(i => i.Message).ToArray()));
                else
                    MessageBox.Show(ex.Message);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
                if (graphicsLayer.GraphicsSource != null)
                    listResults.Visibility = Visibility.Visible;
            }
        }

        // Helper method to retrieve an extent from graphics in the graphics layer
        private Envelope ExtentFromGraphics()
        {
			var graphics = graphicsLayer.GraphicsSource;
			if (graphics == null || graphics.Count() == 0)
				return mapView.Extent;

			var extent = graphics.First().Geometry.Extent;
			foreach (var graphic in graphics)
			{
				if (graphic == null || graphic.Geometry == null)
					continue;
				extent = extent.Union(graphic.Geometry.Extent);
				MapPoint point = graphic.Geometry as MapPoint;
			}

			return extent;
        }
    }
}
