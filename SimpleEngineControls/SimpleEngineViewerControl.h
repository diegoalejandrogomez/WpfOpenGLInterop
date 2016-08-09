#pragma once
using namespace System;
using namespace System::Windows::Forms;
#include "SimpleEngineControls.h"
#include "TileEditorApp.h"

namespace SimpleEngineControls {

	public ref class SimpleEngineViewerControl : public UserControl
	{
	public:
		
		property IntPtr WPFWindowHandle;

		SimpleEngineViewerControl();
		void OnLoad(System::Object ^sender, System::EventArgs ^e);
		void OnSizeChanged(System::Object ^sender, System::EventArgs ^e);
		void OnPaintBackground(PaintEventArgs^ e) override;
		void OnPaint(PaintEventArgs^ e) override;
	
		void Initialize();
	private:
		TileEditorApp * _appLogic;
		
	};


}






