using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using MappingDesktopUI.ViewModels;
using Microsoft.Win32;

namespace MappingDesktopUI.Views
{
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        public MapView()
        {

            InitializeComponent();
            Initialize();
            DataContext = this;

            string esriApi = ConfigurationManager.AppSettings["esriApi"];

            try
            {
                ArcGISRuntimeEnvironment.ApiKey = esriApi;
                ArcGISRuntimeEnvironment.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ArcGIS Runtime initialization failed.");
            }

            MyMapView.Map = new Map(BasemapStyle.ArcGISImagery);

            _serviceFeatureTable = new ServiceFeatureTable(new Uri(FeatureServiceUrl));
            _serviceLayer = new FeatureLayer(_serviceFeatureTable);

            MyMapView.Map.OperationalLayers.Add(_serviceLayer);
        }

        private ServiceFeatureTable _serviceFeatureTable;

        private FeatureLayer _serviceLayer;

        private ArcGISFeature _selectedFeature;

        private const string FeatureServiceUrl = "https://services7.arcgis.com/OrD0y9T7jEt4KcV9/arcgis/rest/services/properties_in_need_of_repairs/FeatureServer/0";

        private bool _isAddToggled = false;

        private bool _isEditToggled = false;

        private bool _isViewToggled = false;

        private bool _isDeleteToggled = false;

        private MapPoint _mapPoint;

        private string _viewId = "";
        private string _editDate = "";
        private string _viewPropertyName = "";
        private string _viewEvaluatorName = "";
        private string _viewDescription = "";




        public bool IsViewToggled
        {
            get { return _isViewToggled; }
            set
            {
                if (_isViewToggled != value)
                {
                    _isViewToggled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsAddToggled
        {
            get { return _isAddToggled; }
            set
            {
                if (_isAddToggled != value)
                {
                    _isAddToggled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEditToggled
        {
            get { return _isEditToggled; }
            set
            {
                if (_isEditToggled != value)
                {
                    _isEditToggled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDeleteToggled
        {
            get { return _isDeleteToggled; }
            set
            {
                if (_isDeleteToggled != value)
                {
                    _isDeleteToggled = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ViewId
        {
            get { return _viewId; }
            set
            {
                if (_viewId != value)
                {
                    _viewId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string EditDate
        {
            get { return _editDate; }
            set
            {
                if (value != _editDate)
                {
                    _editDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ViewPropertyName
        {
            get { return _viewPropertyName; }
            set
            {
                if (value != _viewPropertyName)
                {
                    _viewPropertyName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ViewEvaluatorName
        {
            get { return _viewEvaluatorName; }
            set
            {
                if (value != _viewEvaluatorName)
                {
                    _viewEvaluatorName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ViewDescription
        {
            get { return _viewDescription; }
            set
            {
                if (value != _viewDescription)
                {
                    _viewDescription = value;
                    OnPropertyChanged();
                }
            }
        }

        //Intializes the basemap and pulls in the feature layer from my ArcGIS account
        private void Initialize()
        {
            MyMapView.Map = new Map(BasemapStyle.ArcGISImagery);

            _serviceFeatureTable = new ServiceFeatureTable(new Uri(FeatureServiceUrl));
            _serviceLayer = new FeatureLayer(_serviceFeatureTable);

            MyMapView.Map.OperationalLayers.Add(_serviceLayer);

            MyMapView.LocationDisplay.IsEnabled = true;
            MyMapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Recenter;
        }

        // Allows the user to add a new feature by tapping on the desried location on the screen
        private void AddFeature_Click(object sender1, RoutedEventArgs d)
        {
            if (!IsAddToggled)
            {
                IsAddToggled = true;
                MyMapView.GeoViewTapped += MapView_Tapped;

                void MapView_Tapped(object sender, GeoViewInputEventArgs e)
                {
                    try
                    {
                        MapPoint tappedPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(e.Location);

                        AddBorder.Visibility = Visibility.Visible;
                        AddBorder.IsEnabled = true;

                        _mapPoint = tappedPoint;
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error adding feature");
                    }
                }
            }
        }

        // Allows the user to select a feature and make edits to it's attributes and attachments
        private void EditFeature_Click(object sender1, RoutedEventArgs d)
        {
            if (!IsEditToggled)
            {
                IsEditToggled = true;
                MyMapView.GeoViewTapped += MapView_Tapped;

                async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
                {
                    _serviceLayer.ClearSelection();
                    _selectedFeature = null;

                    EditBorder.Visibility = Visibility.Visible;
                    EditBorder.IsEnabled = true;
                    AttachmentsListBox.IsEnabled = false;
                    AttachmentsListBox.ItemsSource = null;
                    AddAttachmentButton.IsEnabled = false;

                    try
                    {
                        IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_serviceLayer, e.Position, 2, false);

                        if (!identifyResult.GeoElements.Any())
                        {
                            return;
                        }

                        GeoElement tappedElement = identifyResult.GeoElements.First();
                        ArcGISFeature tappedFeature = (ArcGISFeature)tappedElement;

                        _serviceLayer.SelectFeature(tappedFeature);
                        _selectedFeature = tappedFeature;

                        await tappedFeature.LoadAsync();

                        IReadOnlyList<Attachment> attachments = await tappedFeature.GetAttachmentsAsync();

                        AttachmentsListBox.ItemsSource = attachments.Where(attachment => attachment.ContentType == "image/jpeg");
                        AttachmentsListBox.IsEnabled = true;
                        AddAttachmentButton.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error loading feature");
                    }
                }
            }
            else
            {
                return;
            }

        }

        // Allows the user to select a feature and view it's attributes and attachments
        private void ViewFeature_Click(object sender1, RoutedEventArgs d)
        {
            if (!IsViewToggled)
            {
                IsViewToggled = true;
                MyMapView.GeoViewTapped += MapView_Tapped;

                async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
                {
                    _serviceLayer.ClearSelection();
                    _selectedFeature = null;

                    try
                    {
                        IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_serviceLayer, e.Position, 2, false);

                        if (!identifyResult.GeoElements.Any())
                        {
                            return;
                        }

                        ViewId = identifyResult.GeoElements.First().Attributes["objectid"].ToString();
                        EditDate = identifyResult.GeoElements.First().Attributes["EditDate"].ToString();
                        ViewPropertyName = (string)identifyResult.GeoElements.First().Attributes["property_name"];
                        ViewEvaluatorName = (string)identifyResult.GeoElements.First().Attributes["evaluator_name"];
                        ViewDescription = (string)identifyResult.GeoElements.First().Attributes["description"];

                        ViewBorder.Visibility = Visibility.Visible;
                        ViewBorder.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error loading feature");
                    }
                }
            }

        }

        // Allows the user to delete features
        private void DeleteFeature_Click(object sender1, RoutedEventArgs d)
        {
            if (!IsDeleteToggled)
            {
                IsDeleteToggled = true;
                MyMapView.GeoViewTapped += MapView_Tapped;

                async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
                {
                    _serviceLayer.ClearSelection();

                    MyMapView.DismissCallout();

                    try
                    {
                        IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_serviceLayer, e.Position, 2, false);

                        if (!identifyResult.GeoElements.Any())
                        {
                            return;
                        }

                        long featureId = (long)identifyResult.GeoElements.First().Attributes["objectid"];

                        QueryParameters qp = new QueryParameters();
                        qp.ObjectIds.Add(featureId);
                        FeatureQueryResult queryResult = await _serviceLayer.FeatureTable.QueryFeaturesAsync(qp);
                        Feature tappedFeature = queryResult.First();

                        _serviceLayer.SelectFeature(tappedFeature);

                        ShowDeletionCallout(tappedFeature);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "There was a problem");
                    }
                }
            }

        }

        // Shows another button to confirm deletion
        private void ShowDeletionCallout(Feature tappedFeature)
        {
            Button deleteButton = new Button();
            deleteButton.Content = "Delete Feature";
            deleteButton.Padding = new Thickness(5);
            deleteButton.Tag = tappedFeature;

            deleteButton.Click += DeleteButton_Click;

            MyMapView.ShowCalloutAt((MapPoint)tappedFeature.Geometry, deleteButton);
        }

        // Second delete button which actually removes the feature from ArcGIS FeatureTable
        private async void DeleteButton_Click(object sender, EventArgs e)
        {
            MyMapView.DismissCallout();

            try
            {
                Button deleteButton = (Button)sender;
                Feature featureToDelete = (Feature)deleteButton.Tag;

                await _serviceLayer.FeatureTable.DeleteFeatureAsync(featureToDelete);

                await _serviceFeatureTable.ApplyEditsAsync();

                MessageBox.Show("Deleted Feature with ID " + featureToDelete.Attributes["objectid"], "Success!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Couldn't delete feature");
            }
        }

        // Opens your local files to allow you to attach an image to the feature
        // Also starts an activity indicator bar
        private async void AddAttachment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFeature == null)
            {
                return;
            }

            AddAttachmentButton.IsEnabled = false;
            ActivityIndicator.Visibility = Visibility.Visible;

            try
            {
                OpenFileDialog dlg = new OpenFileDialog
                {
                    DefaultExt = ".jpg",
                    Filter = "Image Files (*.JPG;*.JPEG) |*.JPG;*.JPEG",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                };

                bool? result = dlg.ShowDialog();

                if (result != true)
                {
                    return;
                }

                string filename = System.IO.Path.GetFileName(dlg.FileName);

                FileStream fs = new FileStream(dlg.FileName,
                    FileMode.Open,
                    FileAccess.Read);

                BinaryReader br = new BinaryReader(fs);

                long numBytes = new FileInfo(dlg.FileName).Length;
                byte[] attachmentData = br.ReadBytes((int)numBytes);

                fs.Close();

                await _selectedFeature.AddAttachmentAsync(filename, "image/jpeg", attachmentData);

                _serviceFeatureTable = (ServiceFeatureTable)_selectedFeature.FeatureTable;

                await _serviceFeatureTable.ApplyEditsAsync();

                _selectedFeature.Refresh();
                AttachmentsListBox.ItemsSource = await _selectedFeature.GetAttachmentsAsync();

                MessageBox.Show("Successfully Added Attachment", "Success!");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error adding attachment");
            }
            finally
            {
                AddAttachmentButton.IsEnabled = true;
                ActivityIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // Deletes an existing attachment from the ArcGIS FeatureTable
        private async void DeleteAttachment_Click(object sender, RoutedEventArgs e)
        {
            ActivityIndicator.Visibility = Visibility.Visible;

            try
            {
                Button sendingButton = (Button)sender;
                Attachment selectedAttachment = (Attachment)sendingButton.DataContext;

                await _selectedFeature.DeleteAttachmentAsync(selectedAttachment);

                _serviceFeatureTable = (ServiceFeatureTable)_selectedFeature.FeatureTable;

                await _serviceFeatureTable.ApplyEditsAsync();

                _selectedFeature.Refresh();
                AttachmentsListBox.ItemsSource = await _selectedFeature.GetAttachmentsAsync();

                MessageBox.Show("Successfully Deleted Attachment", "Success!");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error deleting attachment");
            }
            finally
            {
                ActivityIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // Downloads an attachment from the ArcGIS FeatureTable to your local decvice
        private async void DownloadAttachment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button sendingButton = (Button)sender;
                Attachment selectedAttachment = (Attachment)sendingButton.DataContext;

                SaveFileDialog dlg = new SaveFileDialog
                {
                    FileName = selectedAttachment.Name,
                    Filter = selectedAttachment.ContentType + "|*.jpg",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                };

                bool? result = dlg.ShowDialog();

                if (result != true)
                {
                    return;
                }

                Stream attachmentDataStream = await selectedAttachment.GetDataAsync();
                byte[] attachmentData = new byte[attachmentDataStream.Length];
                attachmentDataStream.Read(attachmentData, 0, attachmentData.Length);

                FileStream fs = new FileStream(dlg.FileName,
                    FileMode.OpenOrCreate,
                    FileAccess.Write);
                fs.Write(attachmentData, 0, (int)attachmentData.Length);
                fs.Close();

                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error reading attachment");
            }
        }

        // Cancel Button on Edit PopUp Form
        private void EditCancel_Click(object sender, RoutedEventArgs e)
        {
            MyMapView.IsEnabled = true;
            EditBorder.Visibility = Visibility.Collapsed;
            EditBorder.IsEnabled = false;
        }

        // Ok button to confirm edits being made through the Edit Feature button
        private async void EditOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _selectedFeature.Attributes["property_name"] = PropertyNameBox.Text;
                _selectedFeature.Attributes["evaluator_name"] = EvaluatorNameBox.Text;
                _selectedFeature.Attributes["description"] = DescriptionBox.Text;
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                MessageBox.Show("Successfully Edited Feature", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                MyMapView.IsEnabled = true;
                EditBorder.Visibility = Visibility.Collapsed;
                EditBorder.IsEnabled = false;
            }
        }

        // Cancel Button on New Attributes PopUp Form
        private void AddCancel_Click(object sender, RoutedEventArgs e)
        {
            MyMapView.IsEnabled = true;
            AddBorder.Visibility = Visibility.Collapsed;
            AddBorder.IsEnabled = false;
        }

        // Ok button to confirm new attributes being added
        private async void AddOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ArcGISFeature feature = (ArcGISFeature)_serviceFeatureTable.CreateFeature();

                feature.Geometry = _mapPoint;

                feature.Attributes["property_name"] = NewPropertyNameBox.Text;
                feature.Attributes["evaluator_name"] = NewEvaluatorNameBox.Text;
                feature.Attributes["description"] = NewDescriptionBox.Text;
                await _serviceFeatureTable.AddFeatureAsync(feature);

                await _serviceFeatureTable.ApplyEditsAsync();

                feature.Refresh();

                MessageBox.Show("Created feature " + feature.Attributes["objectid"], "Success!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error adding feature");
            }
            finally
            {
                MyMapView.IsEnabled = true;
                AddBorder.Visibility = Visibility.Collapsed;
                AddBorder.IsEnabled = false;
            }
        }

        private void ViewOk_Click(object sender, RoutedEventArgs e)
        {
            MyMapView.IsEnabled = true;
            ViewBorder.Visibility = Visibility.Collapsed;
            ViewBorder.IsEnabled = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
