#include "stdafx.h"
#include "DebugGameLogic.h"
#include "SimpleDebugObject.h"
#include "SimpleEngine.h"
#include "SimpleLayer.h"
#include "SimpleSpriteRenderer.h"

void DebugGameLogic::Init()
{

	SimpleLayer* layer = new SimpleLayer();
	layer->SetZ(SimpleEngine::Instance()->GetScene()->GetLowerZIndex() - 1);
	SimpleEngine::Instance()->GetScene()->AddLayer(layer);
	
	/*SimpleDebugObject* obj = new SimpleDebugObject();
	SimpleEngine::Instance()->GetScene()->AddEntity(obj, layer);*/

	SimpleSpriteRenderer * sprite = new SimpleSpriteRenderer();
	sprite->SetAsTexture("./media/spriteFullPOT.png");
	//sprite->GetTexture()->SetPixelated();
	SimpleEngine::Instance()->GetScene()->AddEntity(sprite, layer);

	
	layer = new SimpleLayer();
	layer->SetZ(SimpleEngine::Instance()->GetScene()->GetLowerZIndex() - 1);
	SimpleEngine::Instance()->GetScene()->AddLayer(layer);
	sprite = new SimpleSpriteRenderer();
	sprite->SetAsTexture("./media/spriteSheet.png");
	SimpleEngine::Instance()->GetScene()->AddEntity(sprite, layer);


	layer = new SimpleLayer();
	layer->SetZ(SimpleEngine::Instance()->GetScene()->GetLowerZIndex() - 1);
	SimpleEngine::Instance()->GetScene()->AddLayer(layer);
	sprite = new SimpleSpriteRenderer();
	sprite->SetAsTexture("./media/spriteFull.png");
	SimpleEngine::Instance()->GetScene()->AddEntity(sprite, layer);

	//Configure input system we are going to use
	//SimpleEngine::Instance()->GetInput()->CreateKeyboard();
	
}

void DebugGameLogic::Advance(float dt)
{
	//auto keyboard = SimpleEngine::Instance()->GetInput()->GetKeyboard();
	//auto camera = SimpleEngine::Instance()->GetScene()->GetCamera();

	//if (keyboard->isKeyDown(OIS::KeyCode::KC_UP))
	//	camera->Move(0.0, 1.0f);
	//if (keyboard->isKeyDown(OIS::KeyCode::KC_DOWN))
	//	camera->Move(0.0, -1.0f);
	//if (keyboard->isKeyDown(OIS::KeyCode::KC_LEFT))
	//	camera->Move(-1.0, 0.0f);
	//if (keyboard->isKeyDown(OIS::KeyCode::KC_RIGHT))
	//	camera->Move(1.0, 0.0f);
	
}

void DebugGameLogic::Shutdown()
{
	//SimpleEngine::Instance()->GetInput()->DestroyKeyboard();
}

bool DebugGameLogic::IsRunning()
{
	return true;
}
