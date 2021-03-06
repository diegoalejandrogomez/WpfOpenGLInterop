#pragma once
#include "SimpleEvent.h"
#include "Events.h"
#include <functional>
#include <unordered_map>
#include <queue>

class SIMPLE_API SimpleDispatcher
{
public:

	static SimpleDispatcher*  Instance()
	{
		static SimpleDispatcher instance;
		return &instance;
	}

	// delete copy and move constructors and assign operators
	SimpleDispatcher(SimpleDispatcher const&) = delete;             // Copy construct
	SimpleDispatcher(SimpleDispatcher&&) = delete;                  // Move construct
	SimpleDispatcher& operator=(SimpleDispatcher const&) = delete;  // Copy assign
	SimpleDispatcher& operator=(SimpleDispatcher &&) = delete;      // Move assign


	SimpleDispatcher() {};

	class EventCallback {

	public:
		EventCallback(void* owner, std::function< void(const SimpleEvent&)>&& callback) {
			_owner = owner;
			_callback = std::move(callback);
		}
		bool operator==(const EventCallback &other) { return _owner == other._owner; };
		bool operator==(const void* other) { return _owner == other; };
		void operator()(const SimpleEvent& evt) const {
			_callback(std::forward<const SimpleEvent&>(evt));
		}
		private:

		void* _owner;
		std::function< void(const SimpleEvent&) > _callback;
		
	};
	
	//register a new listener
	void AddListener(const SimpleEvent::DescriptorType& descriptor, EventCallback&& callback);
	//register a listener to a particular object
	void AddObjectListener(void* object, const SimpleEvent::DescriptorType& descriptor, EventCallback&& callback);
	//remove a listener
	void RemoveListener(const SimpleEvent::DescriptorType& descriptor, void *owner);
	//remove a listener pointing to a particular object
	void RemoveObjectListener(void* object, const SimpleEvent::DescriptorType& descriptor, void *owner);
	//removes the listeners of owner associated to any object
	void RemoveAllObjectsListener(const SimpleEvent::DescriptorType& descriptor, void *owner);
	//Sends an event "asynchronously"
	template <class T, class... Args>
	void SendImmediate(Args&&... args) const
	{
		SimpleEvent *evt = new T{ std::forward<Args>(args)... };
		_DispatchEvent(evt);
		SIMPLE_LOG("Sent Immediate event %s", T::descriptor);
	}

	//Sends an event synchronously
	template <class T, class... Args>
	void Send(Args&&... args) 
	{
		SimpleEvent* event = new T{ std::forward<Args>(args)... };
		_queuedEvents.push(event);
	}

	//Sends the accumulated events at the end of the frame
	void Flush();
private:

	void _DispatchEvent(SimpleEvent *evt) const;
	//Lets assume the registration order is not important
	std::unordered_map< SimpleEvent::DescriptorType, std::vector<EventCallback> > _listeners;
	std::unordered_map<void*, std::unordered_map< SimpleEvent::DescriptorType, std::vector<EventCallback> > > _objectListeners;
	std::queue<SimpleEvent*> _queuedEvents;
};
