#include "stdafx.h"
#include "SpriteSheetControl.h"
#include "SimpleEngine.h"
#include "TileEditorApp.h"

using namespace System::Runtime::InteropServices;

using namespace SimpleEngineControls;

SpriteSheetControl::SpriteSheetControl() {
	_simpleSpriteSheetRenderer = new SimpleSpriteSheetRenderer();
	_managedSimpleObject = gcnew ManagedSimpleObject();
	//_managedSimpleObject->SetSimpleObject(_simpleSpriteRenderer);
}

SpriteSheetControl::~SpriteSheetControl() {
	delete _simpleSpriteSheetRenderer;
}

int SpriteSheetControl::AddControl(System::String^ path, int x, int y, int w, int h)
{
	
	std::string chars =
		(const char*)(Marshal::StringToHGlobalAnsi(path)).ToPointer();

	//Patch ... should be fixed
	 size_t from = chars.find("/temp/");
	 chars.erase(0, from + 6);


	SimpleSpriteSheet* sheet = SimpleEngine::Instance()->GetResourceManager()->GetSpriteSheet(chars);
	if (sheet == nullptr) {
		SimpleEngine::Instance()->GetResourceManager()->CreateSpriteSheet(chars);
		sheet = SimpleEngine::Instance()->GetResourceManager()->GetSpriteSheet(chars);
	}

	int idx = sheet->GetFrameIndex({ x,y }, { w,h });
	if(idx == -1)
		idx = sheet->AddSpriteFrame({ x,y }, { w,h });


	//Set the brush instead of putting the tile in the world
	TileEditorApp* app = dynamic_cast<TileEditorApp*>(SimpleEngine::Instance()->GetGameLogic());
	app->SetCursorTile(chars, idx);
	return idx;
}

ManagedSimpleObject^ SpriteSheetControl::GetManagedSimpleObject() {
	return this->_managedSimpleObject;
}
