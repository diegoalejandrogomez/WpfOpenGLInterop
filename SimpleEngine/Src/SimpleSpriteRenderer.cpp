#include "stdafx.h"
#include "SimpleSpriteRenderer.h"
#include "SimpleEngine.h"
#include <algorithm>
#include <glm\gtx\transform.hpp>

//Register entry as simpleID for factory
FACTORY_REGISTER(SimpleObject, SimpleSpriteRenderer)

SimpleSpriteRenderer::SimpleSpriteRenderer() {

	SimpleRenderer* render = SimpleEngine::Instance()->GetRenderer();
	SimpleResourceManager* resources = SimpleEngine::Instance()->GetResourceManager();
	//All the sprites use the same geometry
	_mesh = render->GetUnitaryQuad();
	_shader = resources->GetProgram("VertexSprite");
	_cam = SimpleEngine::Instance()->GetScene()->GetCamera();

	_rectOffset = glm::vec2(0.0f);
	_rectSize = glm::vec2(1.0f);

	_snapToGrid = false;
	_snapSize = glm::vec2(1.0);
}

void SimpleSpriteRenderer::SetRect(glm::vec2 offset, glm::vec2 size) {
	_rectOffset = offset;
	_rectSize = size;
}

SimpleSpriteRenderer::~SimpleSpriteRenderer() {

	

}

void SimpleSpriteRenderer::SetAsTexture(std::string && name) {
	
	SimpleResourceManager* resources = SimpleEngine::Instance()->GetResourceManager();
	_tex = resources->GetTexture(name);
	if (_tex == nullptr) {
		resources->LoadTexture(name);
		_tex = resources->GetTexture(name);
	}
	SIMPLE_ASSERT(_tex != nullptr);

	_rectOffset = { 0,0 };
	_rectSize = { _tex->GetWidth(), _tex->GetHeight() };
	//Set sprite size using texture resolution
	_aabb.size = _rectSize;
}

void SimpleSpriteRenderer::SetAsTexture(SimpleTexture* t) {
	_tex = t;
	_rectOffset = { 0,0 };
	_rectSize = { _tex->GetWidth(), _tex->GetHeight() };
	//Set sprite size using texture resolution
	_aabb.size = _rectSize;
}



void SimpleSpriteRenderer::SetAsTextureRect(std::string && name, glm::vec2 offset, glm::vec2 size) {
	SimpleResourceManager* resources = SimpleEngine::Instance()->GetResourceManager();
	_tex = resources->GetTexture(name);
	if (_tex == nullptr) {
		resources->LoadTexture(name);
		_tex = resources->GetTexture(name);
	}
	SetRect(offset, size);
}


void SimpleSpriteRenderer::SetAsTextureRect(SimpleTexture* t, glm::vec2 offset, glm::vec2 size) {
	_tex = t;
	SetRect(offset, size);
}

void SimpleSpriteRenderer::Advance(float dt) {



}

void SimpleSpriteRenderer::Render(float dt) {
		
	_shader->Bind();
		
	//Bad way of querying uniforms
	GLuint modelLoc = _shader->GetLocationFor( "modelMatrix");

	glm::vec3 pos = _aabb.Center();
	//Not efficient at all, but easier to read for now
	if (_snapToGrid) {
		pos.x = std::floor(pos.x / _snapSize.x) + 0.5f * _snapSize.x;
		pos.y = std::floor(pos.y / _snapSize.y) + 0.5f * _snapSize.y;
	}
	_aabb.position = pos;

	glm::mat4 model = glm::translate(pos) * glm::rotate(_orientation, glm::vec3(0.0f, 0.0f, 1.0f)) * glm::scale(glm::vec3(_aabb.size.x, _aabb.size.y, 1.0f ));
	

	glUniformMatrix4fv(modelLoc, 1, GL_FALSE, &model[0][0]);

	GLuint viewProjectionLoc = _shader->GetLocationFor("viewProjectionMatrix");
	glUniformMatrix4fv(viewProjectionLoc, 1, GL_FALSE, &_cam->GetViewProjection()[0][0]);

	GLuint uvOffsetLoc = _shader->GetLocationFor("uvOffset");
	glUniform2f(uvOffsetLoc,_rectOffset[0] / (float)_tex->GetWidth(),
							_rectOffset[1] / (float)_tex->GetHeight());


	GLuint uvSizeLoc = _shader->GetLocationFor("uvSize");
	glUniform2f(uvSizeLoc, _rectSize[0] / (float)_tex->GetWidth(),
							_rectSize[1] / (float)_tex->GetHeight());

	GLuint sizeRatioLoc = _shader->GetLocationFor("sizeRatio");
	glUniform2f(sizeRatioLoc,	_tex->GetWidthRatio(),
								_tex->GetHeightRatio());


	GLuint textureLoc = _shader->GetLocationFor("texSampler");
	glUniform1i(textureLoc, 0); //Texture unit 0 is for base images.

	//end uniform query

	_tex->BindTo(0);

	_mesh->Bind();
	_mesh->Draw();
	_mesh->Unbind();

}


json SimpleSpriteRenderer::Serialize() {

	json so = SimpleObject::Serialize();
	json ret{
		{"texture", _tex->GetPath()},
		{"uvOffset", {_rectOffset.x, _rectOffset.y} },
		{"uvSize", {_rectSize.x, _rectSize.y}},
		{"snapToGrid",_snapToGrid},
		{"snapSize", {_snapSize.x, _snapSize.y}}		
	};

	so["SimpleSpriteRenderer"] = ret;
	
	return so;


}
bool SimpleSpriteRenderer::Deserialize(const json &node) {
	
	return this->Deserialize(node, "");
}

bool SimpleSpriteRenderer::Deserialize(const json &node, std::string dir)
{
	SimpleObject::Deserialize(node, dir);

	const json& local = node["SimpleSpriteRenderer"];

	SIMPLE_ASSERT(local.find("texture") != local.end());
	std::string path = local["texture"];

	SIMPLE_ASSERT(local.find("uvOffset") != local.end());
	SIMPLE_ASSERT(local["uvOffset"].is_array());
	_rectOffset.x = local["uvOffset"][0];
	_rectOffset.y = local["uvOffset"][1];

	SIMPLE_ASSERT(local.find("uvSize") != local.end());
	SIMPLE_ASSERT(local["uvSize"].is_array());
	_rectSize.x = local["uvSize"][0];
	_rectSize.y = local["uvSize"][1];

	SIMPLE_ASSERT(local.find("snapToGrid") != local.end());
	_snapToGrid = local["snapToGrid"];

	SIMPLE_ASSERT(local.find("snapSize") != local.end());
	SIMPLE_ASSERT(local["snapSize"].is_array());
	_snapSize.x = local["snapSize"][0];
	_snapSize.y = local["snapSize"][1];

	SetAsTextureRect(std::move(path), _rectOffset, _rectSize);

	return true;
}