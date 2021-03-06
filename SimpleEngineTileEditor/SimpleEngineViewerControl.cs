﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SimpleEngineTileEditor
{
    public class SimpleEngineViewerControl : UserControl
    {
        #region SimpleEngineImports
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern IntPtr SimpleEngine_Instance();

        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern void SimpleEngine_Initialize();
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern void SimpleEngine_Shutdown();
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern void SimpleEngine_InitRenderer(IntPtr hWnd, UInt32 width, UInt32 height);
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern void SimpleEngine_InitInput(IntPtr hWnd, bool exclusive);
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern void SimpleEngine_SetGameLogic(IntPtr logic);
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern void SimpleEngine_Render(float dt);
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern void SimpleEngine_Advance(float dt);
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern IntPtr SimpleEngine_GetScene();
        #endregion

        #region SimpleRendererImports
        //Simple Renderer Imports
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern void SimpleRenderer_ResizeWindow(Int32 width, Int32 height);
        #endregion

        #region SimpleCameraImports
        //Simple Camera Imports
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern void SimpleCamera2D_ScreenToWorld(ref float x, ref float y);
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern void SimpleCamera2D_Move(float dx, float dy);
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern void SimpleCamera2D_DeltaZoom(float dz);
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern float SimpleCamera2D_GetZoom();
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern float SimpleCamera2D_GetMaxZoom();
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern void SimpleCamera2D_SetZoom(float zoom);
        #endregion

        #region SimpleSceneImports
        //Simple Scene Imports
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern IntPtr SimpleScene_PickFirstPoint(float x, float y);

        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern UInt32 SimpleScene_GetLayerCount();

        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern IntPtr SimpleScene_GetLayerWithIdx(Int32 nLayer);

        #endregion

        #region SimpleLayerImports
        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern IntPtr SimpleLayer_EntitiesBegin(IntPtr layer);

        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern IntPtr SimpleLayer_EntitiesNext(IntPtr layer);

        [DllImport("SimpleEngine_dyn.dll", CallingConvention = CallingConvention.Cdecl )]
        static extern IntPtr SimpleLayer_EntitiesEnd(IntPtr layer);

        #endregion

        #region NativeEditorImports
        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr TileEditorApp_Create();

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void TileEditorApp_Destroy(IntPtr app);

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void TileEditorApp_SetCursorPosition(float x, float y);

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void TileEditorApp_SetCursorIdle();

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void TileEditorApp_SetCursorTile(String sheet, Int32 index);

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void TileEditorApp_NewMap();

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void TileEditorApp_LoadState(String gameState);

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern String TileEditorApp_GetState();

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void TileEditorApp_SetMapSize(int width, int height);

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void TileEditorApp_SetMapWidth(int width);

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void TileEditorApp_SetMapHeight(int height);      

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void TileEditorApp_SetCursorErase();

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void TileEditorApp_Paint();


        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int TileEditorApp_GetMapWidth();

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int TileEditorApp_GetMapHeight();

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern String TileEditorApp_Serialize();

        [DllImport("SimpleEngineNativeTileEditor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern bool TileEditorApp_Deserialize(String node);
        #endregion

        public IntPtr WPFWindowHandle { get; set; }

        public SimpleEngineViewerControl()
        {
            this.Load += OnLoad;
            this.SizeChanged += OnSizeChanged;
        }

        public void OnLoad(Object sender, EventArgs e)
        {

        }

        public void Initialize()
        {
            IntPtr hWnd = this.Handle;
            SimpleEngine_Instance();
            SimpleEngine_InitRenderer(hWnd, (UInt32)Width, (UInt32)Height);
            SimpleEngine_InitInput(WPFWindowHandle, false);
            SimpleEngine_Initialize();

            IntPtr appLogic = TileEditorApp_Create();
            SimpleEngine_SetGameLogic(appLogic);
            OnEngineInitialized(this, EventArgs.Empty);

        }

        public void OnSizeChanged(Object sender, EventArgs e)
        {
            SimpleRenderer_ResizeWindow(Width, Height);
        }

        public void SetMousePosition(float x, float y)
        {
            float ix = x;
            float iy = y;

            SimpleCamera2D_ScreenToWorld(ref ix, ref iy);
            TileEditorApp_SetCursorPosition(ix, iy);
        }

        override protected void OnPaintBackground(PaintEventArgs e)
        {

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            SimpleEngine_Advance(0.0f);
            SimpleEngine_Render(0.0f);
        }

        public ManagedSimpleObject SetItem(float x, float y)
        {

            float ix = x;
            float iy = y;

            SimpleCamera2D_ScreenToWorld(ref ix, ref iy);
            IntPtr res = SimpleScene_PickFirstPoint(ix, iy);

            if (res != IntPtr.Zero)
            {
                ManagedSimpleObject SelectedObject = new ManagedSimpleObject();
                SelectedObject.SetSimpleObject(res);
                return SelectedObject;
            }

            return null;
        }

        public List<ManagedSimpleObject> GetAllTiles()
        {
            List<ManagedSimpleObject> managedSimpleObjects = new List<ManagedSimpleObject>();
            Int32 layerCount = (Int32)SimpleScene_GetLayerCount();

            for (Int32 i = 0; i < layerCount; ++i)
            {
                IntPtr layer = SimpleScene_GetLayerWithIdx(i);

                IntPtr entity = SimpleLayer_EntitiesBegin(layer);
                IntPtr end = SimpleLayer_EntitiesEnd(layer);

                while (entity != end)
                {
                    ManagedSimpleObject simpleObject = new ManagedSimpleObject();
                    simpleObject.SetSimpleObject(entity);
                    managedSimpleObjects.Add(simpleObject);

                    entity = SimpleLayer_EntitiesNext(layer);
                }
            }
            return managedSimpleObjects;
        }

        public void MoveCamera(float dx, float dy)
        {
            float wX = dx; float wY = dy;
            SimpleCamera2D_ScreenToWorld(ref wX, ref wY);
            float wOX = 0; float wOY = 0;
            SimpleCamera2D_ScreenToWorld(ref wOX, ref wOY);

            float deltaX = wX - wOX;
            float deltaY = wY - wOY;

            SimpleCamera2D_Move(deltaX, -deltaY);

        }


        public void DeltaZoom(float dz)
        {
            SimpleCamera2D_DeltaZoom(dz);
        }

        public float GetZoom()
        {
            return SimpleCamera2D_GetZoom();
        }

        public void SetZoom(float z)
        {
            SimpleCamera2D_SetZoom(z);
        }

        public void Restart()
        {
            SimpleEngine_Shutdown();
            Initialize();

        }

        //Custom events
        public event EventHandler OnEngineInitialized;

        public int MaxZoom
        {
            get
            {
                if (SimpleEngine_GetScene() != IntPtr.Zero)
                    return (int)SimpleCamera2D_GetMaxZoom();
                return 100;

            }
        }

        public List<ManagedSimpleLayer> ManagedSimpleLayers
        {

            get
            {
                List<ManagedSimpleLayer> managedSimpleLayers = new List<ManagedSimpleLayer>();

                IntPtr scene = SimpleEngine_GetScene();
                if (scene != IntPtr.Zero)
                {

                    int layerCount = (int)SimpleScene_GetLayerCount();
                    for (int i = 0; i < layerCount; ++i)
                    {
                        IntPtr nativeLayer = SimpleScene_GetLayerWithIdx(i);
                        ManagedSimpleLayer layer = new ManagedSimpleLayer(nativeLayer);
                        managedSimpleLayers.Add(layer);
                    }

                }

                return managedSimpleLayers;
            }
        }
    }

}
