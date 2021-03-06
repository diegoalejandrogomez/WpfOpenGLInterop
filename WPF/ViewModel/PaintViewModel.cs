﻿using Microsoft.Win32;
using SimpleEngineTileEditor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WPF.Infrastructure;
using WPF.Model;

namespace WPF.ViewModel
{
    public class PaintViewModel : INotifyPropertyChanged
    {
        #region Properties

        private System.Windows.Forms.UserControl openGLRenderControl;
        public System.Windows.Forms.UserControl OpenGLRenderControl
        {
            get
            {
                return openGLRenderControl;
            }
            set
            {
                openGLRenderControl = value;
                openGLRenderControl.Click += new EventHandler(this.OnClick);
                openGLRenderControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnDrag);
                (openGLRenderControl as SimpleEngineViewerControl).OnEngineInitialized += OnGameLogicCreated;
                openGLRenderControl.MouseWheel += OnMouseWheel;
                PropertyChanged(this, new PropertyChangedEventArgs("Layers"));
                openGLRenderControl.Resize += Resize;


            }
        }

        private String mousePosition;

        public String MousePosition
        {
            get
            {
                return mousePosition;
            }
            set
            {
                mousePosition = value;
                PropertyChanged(this, new PropertyChangedEventArgs("MousePosition"));
            }
        }

        private Int32 _zoomLevel;

        public Int32 ZoomLevel {

            get {
                return _zoomLevel;
            }
            set {
                SimpleEngineViewerControl view = openGLRenderControl as SimpleEngineViewerControl;
                _zoomLevel = value;
                _zoomLevel = Math.Max(Math.Min(100, (int)_zoomLevel), 0);
                view.SetZoom(_zoomLevel / 100.0f * MaxZoomLevel);
                PropertyChanged(this, new PropertyChangedEventArgs("ZoomLevel"));
            }
        }

        public Int32 MaxZoomLevel
        {
            get {
                SimpleEngineViewerControl view = openGLRenderControl as SimpleEngineViewerControl;
                return view.MaxZoom;
            }

        }

        public Object selected;

        public Object Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Selected"));
            }
        }

        public bool drawLine;

        public bool DrawLine
        {
            get { return drawLine; }
            set
            {
                drawLine = value;
                PropertyChanged(this, new PropertyChangedEventArgs("DrawLine"));
            }
        }

        private int quantityX;

        public int QuantityX
        {
            get { return quantityX; }
            set
            {
                quantityX = value;
                PropertyChanged(this, new PropertyChangedEventArgs("QuantityX"));
            }
        }

        private int quantityY;

        public int QuantityY
        {
            get { return quantityY; }
            set
            {
                quantityY = value;
                PropertyChanged(this, new PropertyChangedEventArgs("QuantityY"));
            }
        }

        public string color;

        public string Color
        {
            get { return color; }
            set
            {
                color = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Color"));
            }
        }

        public string fps;

        public string Fps
        {
            get { return fps; }
            set
            {
                fps = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Fps"));
            }
        }

        public bool DrawSquare;

        public bool Drag;

        public bool Paint;

        public bool Erase;

        public bool Pick;

        public string filePath;

        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                PropertyChanged(this, new PropertyChangedEventArgs("FilePath"));
            }
        }

        public List<ManagedSimpleLayer> Layers
        {
            get
            {
                return ((SimpleEngineViewerControl)OpenGLRenderControl).ManagedSimpleLayers;
            }

        }

        public ObservableCollection<TileViewModel> Tiles { get; set; }

        public ManagedSimpleLayer SelectedLayer { get; set; }

        public List<TileViewModel> SelectedTiles { get; set; }

        AnimationViewModel animationViewModel { get; set; }

        public ObservableCollection<AnimationViewModel> Animations { get; set; }

        private FontViewModel fontViewModel;

        TileMapControl _tileMap;
        float _panSpeed = 1.0f;
        float _zoomSpeed = 0.01f;
        float _prevX, _prevY;

        #endregion

        #region Methods

        private void GetAvailableTiles(Project project)
        {
            if(Tiles == null)
            {
                return;
            }

            //get all available tiles on bar
            foreach (var tile in Tiles)
            {
                var resource = new Resource();
                if (tile.Path.Contains(AppDomain.CurrentDomain.BaseDirectory))
                {
                    resource.Name = tile.Path.Substring(AppDomain.CurrentDomain.BaseDirectory.Length, tile.Path.Length - AppDomain.CurrentDomain.BaseDirectory.Length);
                }
                else
                {
                    resource.Name = tile.Path;
                }

                resource.Data = ConvertToBytes(tile.Image);
                resource.Id = tile.Id;
                var property = new ResourceProperty();
                property.Heigth = tile.heigth;
                property.Width = tile.width;
                property.X = tile.x;
                property.Y = tile.y;
                property.Splited = tile.Splited;
                resource.Properties = property;
                project.Resources.Add(resource);
            }
        }

        public void ClearParameters()
        {
            this.DrawSquare = false;
            this.DrawLine = false;
            this.Drag = false;
            this.Paint = false;
            this.Erase = false;
            this.Pick = false;

        }

        public void OnClick(Object sender, EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs args = (System.Windows.Forms.MouseEventArgs)e;
            
            if (Drag)
            {
                if (Selected != null)
                    Drag = false;
                Selected = null;
            }
            else
            {
                if (args.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    if (Pick) //We should filter based on selected tool (brush, move tile, etc)
                    {
                        if (Selected != null)
                            Drag = false;
                        int x = ((System.Windows.Forms.MouseEventArgs)e).X;
                        int y = ((System.Windows.Forms.MouseEventArgs)e).Y;
                        Selected = null;
                        Selected = ((SimpleEngineViewerControl)OpenGLRenderControl).SetItem(x, y);
                        if (Selected == null)
                            Selected = _tileMap;
                    }
                    else
                    {
                        _tileMap.Paint();

                    }
                }
            }
            PropertyChanged(this, new PropertyChangedEventArgs("Layers"));
            PropertyChanged(this, new PropertyChangedEventArgs("MaxZoomLevel"));
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                bitmapimage.Freeze();
                return bitmapimage;
            }
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return bitmap;
            }
        }


        public static byte[] ConvertToBytes(BitmapImage bitmapImage)
        {
            byte[] data;
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }

            return data;
        }

        private void GetSelectableAnimations(Project project)
        {
            this.Animations = new ObservableCollection<ViewModel.AnimationViewModel>();
            int i = this.Animations.Count + 1;
            string path = string.Empty;
            foreach (var animation in project.Animations)
            {
                var animationViewModel = new AnimationViewModel();
                animationViewModel.Name = animation.Name;
                animationViewModel.Frequency = animation.Frequency;
                var addedTiles = this.Tiles.Where(tile => animation.Resources.Contains(tile.Id)).ToList();
                if (animationViewModel.Tiles == null)
                    animationViewModel.Tiles = new List<ViewModel.TileViewModel>();

                animationViewModel.Tiles.AddRange(addedTiles);
                this.animationViewModel = animationViewModel;
                
                if (Animations == null)
                {
                    Animations = new ObservableCollection<AnimationViewModel>();
                }

                Animations.Add(animationViewModel);
                PropertyChanged(this, new PropertyChangedEventArgs("Animations"));
            }
        }

        private BitmapImage GetSelectableTiles(BitmapImage bitmapImage, Project project)
        {
            int i = this.Tiles.Count + 1;
            string path = string.Empty;
            foreach (var item in project.Resources)
            {
                byte[] image = item.Data;
                MemoryStream ms = new MemoryStream(image);
                System.Drawing.Image img = System.Drawing.Image.FromStream(ms);

                if (item.Properties != null && !item.Properties.Splited)
                {
                    path = ImportImageToTempFolder(i, img);
                    i++;
                }

                var newTile = new TileViewModel(item.Id);
                newTile.Path = path;
                if (item.Properties != null)
                {
                    newTile.x = item.Properties.X;
                    newTile.y = item.Properties.Y;
                    newTile.width = (int)item.Properties.Width;
                    newTile.heigth = (int)item.Properties.Heigth;
                }

                if (item.Properties != null && item.Properties.Splited)
                {
                    if (bitmapImage != null)
                    {
                        var bitmapImageConverted = new Bitmap(BitmapImage2Bitmap((BitmapImage)bitmapImage));
                        if (bitmapImageConverted.Height >= newTile.y + newTile.heigth && bitmapImageConverted.Width >= newTile.x + newTile.width)
                        {
                            var imageSplited = bitmapImageConverted.Clone(new Rectangle(newTile.x, newTile.y, newTile.width, newTile.heigth), System.Drawing.Imaging.PixelFormat.DontCare);
                            newTile.Image = BitmapToImageSource(imageSplited);
                            newTile.Splited = true;
                        }
                        else
                        {
                            var asd = 1;
                        }
                    }
                }
                else
                {
                    newTile.Image = new BitmapImage();
                    newTile.Image.BeginInit();
                    newTile.Image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    newTile.Image.CacheOption = BitmapCacheOption.OnLoad;
                    newTile.Image.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                    newTile.Image.UriSource = new Uri(path);
                    newTile.Image.EndInit();
                    newTile.Image.Freeze();
                    bitmapImage = newTile.Image;
                }

                if (newTile.Image != null)
                {
                    newTile.SpriteControl = new SpriteSheetControl();
                    if (Tiles == null)
                        Tiles = new ObservableCollection<TileViewModel>();
                    this.Tiles.Add(newTile);
                }

                img.Dispose();
                ms.Dispose();
            }

            PropertyChanged(this, new PropertyChangedEventArgs("Tiles"));

            return bitmapImage;
        }

        private static string ImportImageToTempFolder(int i, Image img)
        {
            string relativePath = @"/temp/image" + i + ".png";
            string path = AppDomain.CurrentDomain.BaseDirectory + relativePath;
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"/temp");

            img.Save(path);
            return path;
        }

        private void SaveAnimation(object sender, EventArgs e)
        {
            AddAnimation();
            if (Animations == null)
            {
                Animations = new ObservableCollection<AnimationViewModel>();
            }

            Animations.Add(animationViewModel);
            PropertyChanged(this, new PropertyChangedEventArgs("Animations"));
        }

        private void AddAnimation()
        {
            animationViewModel.Validate();

            animationViewModel.AnimatedControl.SetAnimation(animationViewModel.Name, (float)animationViewModel.Frequency * 1e-3f);
            foreach (TileViewModel t in animationViewModel.Tiles)
            {
                animationViewModel.AnimatedControl.AddFrame(t.Path, t.x, t.y, t.width, t.heigth);
            }

            animationViewModel.AnimatedControl.AddControl(animationViewModel.Name);
        }

        private void Clean()
        {
            ((SimpleEngineViewerControl)OpenGLRenderControl).Restart();
            this.Tiles = new ObservableCollection<TileViewModel>();
            PropertyChanged(this, new PropertyChangedEventArgs("Tiles"));
            this.Selected = null;

            string relativePath = @"/temp/";
            string path = AppDomain.CurrentDomain.BaseDirectory + relativePath;

            System.IO.DirectoryInfo di = new DirectoryInfo(path);

            if (di.Exists)
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }

        }

        #endregion

        #region Events
        private void Window_Closed(object sender, EventArgs e)
        {
            PropertyChanged(this, new PropertyChangedEventArgs("Layers"));
        }

        private void Resize(object sender, EventArgs e)
        {
            ZoomLevel = _zoomLevel;
        }

        private void OnMouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            SimpleEngineViewerControl view = openGLRenderControl as SimpleEngineViewerControl;
            //float maxZoomLevel = MaxZoomLevel * 100.0f; // Shouldn't this value be constant and always positive??
            //-> Yes, I forgot to fix this after we added the ui control for the zoom. Now is a value in [1,100]

            ZoomLevel = (Int32)(e.Delta * _zoomSpeed  + _zoomLevel);

        }

        private void OnGameLogicCreated(object sender, EventArgs e)
        {
            _tileMap = new TileMapControl();
            PropertyChanged(this, new PropertyChangedEventArgs("MaxZoomLevel"));
        }

        public void OnDrag(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            
            SimpleEngineViewerControl view = openGLRenderControl as SimpleEngineViewerControl;

            //Panning
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                view.MoveCamera(-(e.X - _prevX) * _panSpeed, (e.Y - _prevY) * _panSpeed);


            MousePosition = e.Location.X + ":" + e.Location.Y;
            if (this.Drag && Selected != null)
            {
                //Check if can be dragged ie ManagedSimpleObject
                ManagedSimpleObject sel = Selected as ManagedSimpleObject;
                if (sel != null)
                {
                    sel.positionX = e.Location.X;
                    sel.positionY = e.Location.Y;
                }
                //Selected = null;
                //Selected = ((SimpleEngineViewerControl)OpenGLRenderControl).SetItem(e.Location.X, e.Location.Y);
            }

            _prevX = e.X;
            _prevY = e.Y;

            //Update cursor position
            view.SetMousePosition(e.X, e.Y);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate (object sender, PropertyChangedEventArgs e)
        {
            var paintViewModel = sender as PaintViewModel;

            if (paintViewModel.OpenGLRenderControl != null)
            {
                //todo get this info using something better than reflection
                //paintViewModel.OpenGLRenderControl.SetProperty(e.PropertyName, true);
            }
        };

        #endregion

        #region Commands
        private ICommand setDrawLineCommand;

        private ICommand setDrawSquareCommand;

        private ICommand openFileCommand;

        private ICommand importImageCommand;

        private ICommand saveCommand;

        private ICommand addSelectedTile;

        private ICommand setDragCommand;

        private ICommand setPaintCommand;

        private ICommand setEraseCommand;

        private ICommand setPickCommand;

        private ICommand splitSelectedImage;

        private ICommand deleteSelectedTile;

        private ICommand deleteSelectedAnimation;

        private ICommand deleteSelectedLayer;

        private ICommand editSelectedLayer;

        private ICommand addLayer;

        private ICommand animateCommand;

        private ICommand addSelectedAnimation;

        private ICommand addTextCommand;
        
        public ICommand AddSelectedTile
        {
            get
            {
                if (addSelectedTile == null)
                {
                    addSelectedTile = new Command((tile) =>
                    {

                        var tileObject = (TileViewModel)tile;
                        tileObject.Idx = tileObject.SpriteControl.AddControl(tileObject.Path, tileObject.x, tileObject.y, tileObject.width, tileObject.heigth);
                    });
                }

                return addSelectedTile;
            }
        }

        public ICommand SplitSelectedImage
        {
            get
            {
                if (splitSelectedImage == null)
                {

                    splitSelectedImage = new Command((tile) =>
                    {
                        var tileObject = (TileViewModel)tile;
                        var image = tileObject.Image;
                        int width = (int)((BitmapImage)image).PixelWidth;
                        int heigth = (int)((BitmapImage)image).PixelHeight;
                        int x = 0;
                        int y = 0;


                        SegmentationWindow segmentWindow = new SegmentationWindow();
                        segmentWindow.ImageSource = image;
                        if (segmentWindow.ShowDialog() == false)
                            return;
                        QuantityX = segmentWindow.QuantityX;
                        QuantityY = segmentWindow.QuantityY;



                        if (QuantityX == 0)
                            QuantityX = 1;
                        if (QuantityY == 0)
                            QuantityY = 1;

                        int proportionalWidth = width / QuantityX;
                        int proportionalHeigth = heigth / QuantityY;


                        while (y + proportionalHeigth <= heigth)
                        {
                            while (x + proportionalWidth <= width)
                            {
                                var bitmapImage = new Bitmap(BitmapImage2Bitmap((BitmapImage)image));
                                var imageSplited = bitmapImage.Clone(new Rectangle(x, y, proportionalWidth, proportionalHeigth), System.Drawing.Imaging.PixelFormat.DontCare);
                                BitmapImage newImage = BitmapToImageSource(imageSplited);
                                var newTile = new TileViewModel();
                                newTile.Image = newImage;
                                newTile.Path = tileObject.Path;
                                newTile.x = x;
                                newTile.y = y;
                                newTile.width = proportionalWidth;
                                newTile.heigth = proportionalHeigth;
                                newTile.SpriteControl = new SpriteSheetControl();
                                newTile.Splited = true;
                                Tiles.Add(newTile);
                                PropertyChanged(this, new PropertyChangedEventArgs("Tiles"));
                                x += proportionalWidth;
                            }

                            x = 0;
                            y += proportionalHeigth;
                        }
                    });
                }

                return splitSelectedImage;
            }

            set { }
        }

        public ICommand DeleteSelectedTile
        {
            get
            {
                if (deleteSelectedTile == null)
                {
                    deleteSelectedTile = new Command((tile) =>
                    {
                        var tileObject = tile as TileViewModel;
                        if (tileObject != null)
                            Tiles.Remove(tileObject);
                    });
                }

                return deleteSelectedTile;
            }
        }

        public ICommand DeleteSelectedAnimation
        {
            get
            {
                if (deleteSelectedAnimation == null)
                {
                    deleteSelectedAnimation = new Command((animation) =>
                    {
                        var animationObject = animation as AnimationViewModel;
                        if (animationObject != null)
                            Animations.Remove(animationObject);
                    });
                }

                return deleteSelectedAnimation;
            }
        }

        public ICommand AddSelectedAnimation
        {
            get
            {
                if (addSelectedAnimation == null)
                {
                    addSelectedAnimation = new Command((animation) =>
                    {
                        animationViewModel = animation as AnimationViewModel;
                        AddAnimation();
                    });
                }

                return addSelectedAnimation;
            }
        }
        
        public ICommand DeleteSelectedLayer
        {
            get
            {
                if (deleteSelectedLayer == null)
                {
                    deleteSelectedLayer = new Command((layer) =>
                    {
                        var layerObject = layer as ManagedSimpleLayer;
                        if (layerObject != null)
                        {
                            layerObject.Remove();
                            Layers.Remove(layerObject);
                            PropertyChanged(this, new PropertyChangedEventArgs("Layers"));
                        }
                    });
                }

                return deleteSelectedLayer;
            }
        }

        public ICommand EditSelectedLayer
        {
            get
            {
                if (editSelectedLayer == null)
                {
                    editSelectedLayer = new Command((layer) =>
                    {
                        Window window = new Window
                        {
                            Title = "My User Control Dialog",
                            Content = new LayerEditor()
                        };

                        ((System.Windows.Controls.UserControl)window.Content).DataContext = layer;
                        window.Width = 300;
                        window.Height = 600;
                        window.Closed += Window_Closed;
                        window.ShowDialog();

                    });
                }

                return editSelectedLayer;
            }
        }

        public ICommand AddLayer
        {
            get
            {
                if (addLayer == null)
                {
                    addLayer = new Command((layer) =>
                    {
                        var layerObject = new ManagedSimpleLayer();
                        PropertyChanged(this, new PropertyChangedEventArgs("Layers"));
                    });
                }

                return addLayer;
            }
        }

        public ICommand SetDrawLineCommand
        {
            get
            {
                if (setDrawLineCommand == null)
                {

                    setDrawLineCommand = new Command((vm) =>
                    {
                        var originalDrawLine = DrawLine;
                        ClearParameters();
                        DrawLine = !originalDrawLine;
                    });
                }

                return setDrawLineCommand;
            }

            set { }
        }

        public ICommand SetDrawSquareCommand
        {
            get
            {
                if (setDrawSquareCommand == null)
                {

                    setDrawSquareCommand = new Command((vm) =>
                    {
                        var originalDrawSquare = DrawSquare;
                        ClearParameters();
                        DrawSquare = !originalDrawSquare;
                    });
                }

                return setDrawSquareCommand;
            }

            set { }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new Command((vm) =>
                    {
                        try
                        {
                            SaveFileDialog dialog = new SaveFileDialog();
                            dialog.DefaultExt = ".poc";
                            if (dialog.ShowDialog() == true)
                            {
                                var project = new Project();
                                var scene = new Scene();
                                GetAvailableTiles(project);
                                if (Animations != null)
                                {
                                    foreach (var item in Animations)
                                    {
                                        Animation animation = new Animation();
                                        animation.Resources.AddRange(item.Tiles.Select(i => i.Id));
                                        animation.Frequency = item.Frequency;
                                        animation.Name = item.Name;
                                        project.Animations.Add(animation);
                                    }
                                }
                                
                                project.Scenes.Add(_tileMap.TakeSnapshot());
                                
                                var packFile = Path.ChangeExtension(dialog.FileName, ".pack");
                                _tileMap.PackResources(packFile);
                                byte[] fileBytes = File.ReadAllBytes(packFile);
                                string sb = System.Text.Encoding.ASCII.GetString(fileBytes);
                                

                                project.PackResources = Zip(sb);
                                var json = Newtonsoft.Json.JsonConvert.SerializeObject(project);
                                System.IO.File.WriteAllText(dialog.FileName, json);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    });
                }

                return saveCommand;
            }
        }

        public ICommand OpenFileCommand
        {
            get
            {
                if (openFileCommand == null)
                {
                    openFileCommand = new Command((vm) =>
                    {
                        this.Clean();
                        OpenFileDialog dialog = new OpenFileDialog();
                        dialog.Filter = "POC Files|*.poc";
                        if (dialog.ShowDialog() == true)
                        {
                            BitmapImage bitmapImage = null;
                            var json = System.IO.File.ReadAllText(dialog.FileName);
                            var project = Newtonsoft.Json.JsonConvert.DeserializeObject<Project>(json);
                            bitmapImage = GetSelectableTiles(bitmapImage, project);
                            GetSelectableAnimations(project);                          
                            var packFile = Path.ChangeExtension(dialog.FileName, ".pack");
                            File.WriteAllBytes(packFile, Encoding.ASCII.GetBytes(Unzip(project.PackResources)));
                            _tileMap.UnpackResources(packFile);

                            foreach (var scene in project.Scenes)
                            {                               
                                _tileMap.RestoreSnapshot(scene);
                            }
                        }
                    });
                }

                return openFileCommand;
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        public ICommand ImportImageCommand
        {
            get
            {
                if (importImageCommand == null)
                {
                    importImageCommand = new Command((vm) =>
                    {
                        OpenFileDialog dialog = new OpenFileDialog();
                        dialog.Filter = "Images |*.png";

                        if (dialog.ShowDialog() == true)
                        {
                            this.FilePath = dialog.FileName;
                            using (var img = Image.FromFile(FilePath))
                            {
                                var path = ImportImageToTempFolder(this.Tiles == null ? 1 : this.Tiles.Where(i => i.Splited == false).Count() + 1, img);
                                var newTile = new TileViewModel();
                                this.FilePath = path;
                                newTile.Image = new BitmapImage();
                                newTile.Image.BeginInit();
                                newTile.Image.CacheOption = BitmapCacheOption.None;
                                newTile.Image.UriSource = new Uri(filePath);
                                newTile.Image.EndInit();
                                newTile.Image.Freeze();
                                newTile.Path = this.FilePath;
                                newTile.x = 0;
                                newTile.y = 0;
                                newTile.width = (int)newTile.Image.PixelWidth;
                                newTile.heigth = (int)newTile.Image.PixelHeight;
                                newTile.SpriteControl = new SpriteSheetControl();
                                if (Tiles == null)
                                    Tiles = new ObservableCollection<TileViewModel>();
                                this.Tiles.Add(newTile);
                                PropertyChanged(this, new PropertyChangedEventArgs("Tiles"));
                            }
                        }
                    });
                }

                return importImageCommand;
            }

            set { }
        }

        public ICommand SetDragCommand
        {
            get
            {
                if (setDragCommand == null)
                {

                    setDragCommand = new Command((vm) =>
                    {
                        var originaldrag = Drag;
                        ClearParameters();
                        Drag = !originaldrag;
                    });
                }

                return setDragCommand;
            }

            set { }
        }


        public ICommand SetPaintCommand
        {
            get
            {
                if (setPaintCommand == null)
                {

                    setPaintCommand = new Command((vm) =>
                    {
                        var originalpaint = Paint;
                        ClearParameters();
                        Paint = !originalpaint;
                        _tileMap.IdleBrush();
                    });
                }

                return setPaintCommand;
            }
            set { }
        }

        public ICommand SetEraseCommand
        {
            get
            {
                if (setEraseCommand == null)
                {

                    setEraseCommand = new Command((vm) =>
                    {
                        var originalerase = Erase;
                        ClearParameters();
                        Erase = !originalerase;
                        _tileMap.EraseBrush();
                    });
                }

                return setEraseCommand;
            }
            set { }
        }

        public ICommand SetPickCommand
        {
            get
            {
                if (setPickCommand == null)
                {

                    setPickCommand = new Command((vm) =>
                    {
                        var originalpick = Pick;
                        ClearParameters();
                        Pick = !originalpick;
                        _tileMap.IdleBrush();
                    });
                }

                return setPickCommand;
            }
            set { }
        }

        public ICommand AnimateCommand
        {
            get
            {
                if (animateCommand == null)
                {

                    animateCommand = new Command((vm) =>
                    {
                        Window window = new Window
                        {
                            Title = "Create Animation",
                            Content = new AnimationEditor()
                        };

                        animationViewModel = new AnimationViewModel();
                        animationViewModel.Tiles = this.SelectedTiles;
                        ((System.Windows.Controls.UserControl)window.Content).DataContext = animationViewModel;
                        window.Width = 300;
                        window.Height = 600;
                        window.Closed += SaveAnimation;
                        window.ShowDialog();
                    });
                }

                return animateCommand;
            }
            set { }
        }

        public ICommand AddTextCommand
        {
            get
            {
                if (addTextCommand == null)
                {

                    addTextCommand = new Command((vm) =>
                    {
                        Window window = new Window
                        {
                            Title = "Font Editor",
                            Content = new FontEditor(),
                            Width = 180,
                            Height = 200,
                        };

                        if(this.fontViewModel == null)
                        {
                            this.fontViewModel = new FontViewModel();
                        }

                        ((System.Windows.Controls.UserControl)window.Content).DataContext = this.fontViewModel;
                        window.Width = 300;
                        window.Height = 600;
                        window.Closed += FontEditorClose; ;
                        window.ShowDialog();
                    });
                }

                return addTextCommand;
            }
            set { }
        }

        private void FontEditorClose(object sender, EventArgs e)
        {
            var test = this.fontViewModel;
            if(this.fontViewModel.textControl == null)
                this.fontViewModel.textControl = new TextControl();
            this.fontViewModel.textControl.AddControl(this.fontViewModel.FontSize, this.fontViewModel.Text, this.fontViewModel.FontFamily, this.fontViewModel.FontColor, "MainTileMap");
            this.fontViewModel.Clear();
        }
        #endregion
    }
}
