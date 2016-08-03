#pragma once
#include <glm\glm.hpp>

class SimpleCamera2D {
	
public:

	SimpleCamera2D();
	void SetPosition(float x, float y);
	void SetZoom(float zoom);
	void Move(float dx, float dt);
	void SetViewportSize(float w, float h);
		
	glm::mat4 &GetViewProjection() {return _viewProjection;}
	glm::mat4 &GetView() {return _view;}
	glm::mat4 &GetProjection() {return _projection;}

private:
	
	void _UpdateTransform();
	
	//Transform matrices
	glm::mat4 _view;
	glm::mat4 _projection;
	glm::mat4 _viewProjection;
	//Camera properties
	glm::vec3 _position;
	glm::vec2 _size;
	float _zoom;
	float _aspectRatio;
};