#include "stdafx.h"
#include "SimpleAnimatedSpriteRenderer.h"

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
	if (_elapsedTime >= period) {
		_elapsedTime -= period;
		++_currentFrame;
		if (_currentFrame >= _anim->GetFrameCount()) {
			if (_loop)
				_currentFrame = 0;
			else {
				--_currentFrame;
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

void SimpleAnimatedSpriteRenderer::Render(float dt){

	//Obtain information from current frame and render. The spritesheet must be previously configured
	int _animIndex = _anim->GetFrame(_currentFrame);
	SimpleSpriteSheetRenderer::SetIndex(_animIndex);
	SimpleSpriteSheetRenderer::Render(dt);
}


