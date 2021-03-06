#include "stdafx.h"
#include "SimpleAnimatedSpriteRenderer.h"
#include "SimpleEngine.h"

FACTORY_REGISTER(SimpleObject, SimpleAnimatedSpriteRenderer);

SimpleAnimatedSpriteRenderer::SimpleAnimatedSpriteRenderer() {
	_playing = false;
	_loop = false;
	_currentFrame = 0;
	_elapsedTime = 0.0f;
}

SimpleAnimatedSpriteRenderer::~SimpleAnimatedSpriteRenderer() {


}

void SimpleAnimatedSpriteRenderer::Play() {
	_playing = true;
	_loop = true;
}


void SimpleAnimatedSpriteRenderer::PlayOnce() {
	_playing = true;
	_loop = false;
}

void SimpleAnimatedSpriteRenderer::Stop() {
	_playing = false;
	_currentFrame = 0;
	_elapsedTime = 0;
}

void SimpleAnimatedSpriteRenderer::Pause() {
	_playing = false;
}

void SimpleAnimatedSpriteRenderer::Advance(float dt) {


	if (!_playing)
		return;
	_elapsedTime += dt;
	float period = _anim->GetFrameTime();
	
	//SIMPLE_LOG("T: %.3f DT : %.3f", period, dt);

	while (_elapsedTime >= period) {
		_elapsedTime -= period;
		++_currentFrame;
		if (_currentFrame >= _anim->GetFrameCount()) {
			_currentFrame = 0;
			if (!_loop){
				_playing = false;
			}

		}

	}

	//Not useful for now... but just in case
	SimpleSpriteSheetRenderer::Advance(dt);
}

void SimpleAnimatedSpriteRenderer::SetAnimation(SimpleSpriteAnimation* anim) {
	_anim = anim;
	SimpleSpriteSheetRenderer::SetSpriteSheet(_anim->GetSpriteSheet());
}

void SimpleAnimatedSpriteRenderer::SetAnimation(SimpleID animName) {

	_anim = SimpleEngine::Instance()->GetResourceManager()->GetSpriteAnimation(animName);
	if(_anim != nullptr)
		SimpleSpriteSheetRenderer::SetSpriteSheet(_anim->GetSpriteSheet());

}

void SimpleAnimatedSpriteRenderer::Render(float dt){

	//Obtain information from current frame and render. The spritesheet must be previously configured
	int _animIndex = _anim->GetFrame(_currentFrame);
	SimpleSpriteSheetRenderer::SetIndex(_animIndex);
	SimpleSpriteSheetRenderer::Render(dt);
}



json SimpleAnimatedSpriteRenderer::Serialize() {

	json so = SimpleSpriteSheetRenderer::Serialize();
	json ret{
		{ "animation", _anim->GetAnimationName() },
		{ "currentFrame",_currentFrame},
		{ "elapsedTime",_elapsedTime },
		{ "playing", _playing },
		{ "loop",_loop }
	};
	so["SimpleAnimatedSpriteRenderer"] = ret;

	return so;


}
bool SimpleAnimatedSpriteRenderer::Deserialize(const json &node) {

	SimpleSpriteSheetRenderer::Deserialize(node);

	const json& local = node["SimpleAnimatedSpriteRenderer"];

	SIMPLE_ASSERT(local.find("animation") != local.end());
	std::string animName = local["animation"];
	_anim = SimpleEngine::Instance()->GetResourceManager()->GetSpriteAnimation(animName);
	SIMPLE_ASSERT(_anim != nullptr);

	SIMPLE_ASSERT(local.find("currentFrame") != local.end());
	_currentFrame = local["currentFrame"];

	SIMPLE_ASSERT(local.find("elapsedTime") != local.end());
	_elapsedTime = local["elapsedTime"];

	SIMPLE_ASSERT(local.find("playing") != local.end());
	_playing = local["playing"];

	SIMPLE_ASSERT(local.find("loop") != local.end());
	_loop = local["loop"];

	return true;
}