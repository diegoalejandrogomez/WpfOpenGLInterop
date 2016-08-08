#include "stdafx.h"
#include "SimpleLayer.h"

SimpleLayer::SimpleLayer() {
}


const std::vector<SimpleObject*>& SimpleLayer::GetEntities()
{
	return this->_entities;
}

SimpleLayer::~SimpleLayer() {

	for (auto& ent : _entities)
		delete ent;

	_entities.clear();
}

void SimpleLayer::AddEntity(SimpleObject* sObj) {

	_entities.push_back(sObj);
}


void SimpleLayer::RemoveEntity(SimpleObject* sObj) {
	auto it = find(_entities.begin(), _entities.end(), sObj);
	if (it != _entities.end())
		_entities.erase(it);
}

void SimpleLayer::SetZ(float Z) {
	this->Z = Z;
}

float SimpleLayer::GetZ() {
	return this->Z;
}