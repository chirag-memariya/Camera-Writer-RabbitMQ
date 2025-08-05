import React, { useState, useEffect } from 'react';
import { Camera, Monitor, HardDrive, Zap, Play, Pause, RotateCcw } from 'lucide-react';

const CameraEventVisualizer = () => {
  const [events, setEvents] = useState([]);
  const [isAutoMode, setIsAutoMode] = useState(false);
  const [autoInterval, setAutoInterval] = useState(null);

  // Camera configuration matching C# code
  const cameraConfig = {
    'CAMERA-001': ['NVR-1', 'Storage-1'],
    'CAMERA-002': ['NVR-2', 'Storage-2']
  };

  const eventTypes = ['MotionDetected', 'AudioAlert', 'ObjectDetection', 'TamperAlert'];
  
  const eventColors = {
    'MotionDetected': 'bg-blue-500',
    'AudioAlert': 'bg-yellow-500',
    'ObjectDetection': 'bg-green-500',
    'TamperAlert': 'bg-red-500'
  };

  // Simulate sending event (matches C# EdgeGatewayAgent logic)
  const sendCameraEvent = (cameraId) => {
    const eventType = eventTypes[Math.floor(Math.random() * eventTypes.length)];
    const timestamp = new Date();
    const interestedParties = cameraConfig[cameraId];
    
    const newEvent = {
      id: `${cameraId}-${timestamp.getTime()}`,
      cameraId,
      eventType,
      timestamp,
      interestedParties: [...interestedParties],
      status: 'publishing'
    };

    setEvents(prev => [newEvent, ...prev.slice(0, 19)]); // Keep last 20 events

    // Simulate message routing delay
    setTimeout(() => {
      setEvents(prev => prev.map(event => 
        event.id === newEvent.id 
          ? { ...event, status: 'delivered' }
          : event
      ));
    }, 800);

    // Auto-clear delivered events after 5 seconds
    setTimeout(() => {
      setEvents(prev => prev.filter(event => event.id !== newEvent.id));
    }, 5000);
  };

  // Auto mode simulation
  useEffect(() => {
    if (isAutoMode) {
      const interval = setInterval(() => {
        const cameras = Object.keys(cameraConfig);
        const randomCamera = cameras[Math.floor(Math.random() * cameras.length)];
        sendCameraEvent(randomCamera);
      }, 2000);
      setAutoInterval(interval);
    } else {
      if (autoInterval) {
        clearInterval(autoInterval);
        setAutoInterval(null);
      }
    }

    return () => {
      if (autoInterval) clearInterval(autoInterval);
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isAutoMode]);

  const clearEvents = () => {
    setEvents([]);
  };

  const toggleAutoMode = () => {
    setIsAutoMode(!isAutoMode);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-4xl font-bold text-white mb-2">
            Camera Event Routing System
          </h1>
          <p className="text-slate-300 text-lg">
            Visualize RabbitMQ Direct Exchange message routing
          </p>
        </div>

        {/* Control Panel */}
        <div className="bg-white/10 backdrop-blur-lg rounded-xl p-6 mb-8 border border-white/20">
          <div className="flex flex-wrap gap-4 items-center justify-center">
            <button
              onClick={() => sendCameraEvent('CAMERA-001')}
              className="bg-gradient-to-r from-blue-500 to-blue-600 hover:from-blue-600 hover:to-blue-700 text-white px-6 py-3 rounded-lg font-medium transition-all duration-200 flex items-center gap-2 shadow-lg hover:shadow-xl transform hover:scale-105"
            >
              <Camera className="w-5 h-5" />
              Trigger CAMERA-001 Event
            </button>
            
            <button
              onClick={() => sendCameraEvent('CAMERA-002')}
              className="bg-gradient-to-r from-green-500 to-green-600 hover:from-green-600 hover:to-green-700 text-white px-6 py-3 rounded-lg font-medium transition-all duration-200 flex items-center gap-2 shadow-lg hover:shadow-xl transform hover:scale-105"
            >
              <Camera className="w-5 h-5" />
              Trigger CAMERA-002 Event
            </button>

            <button
              onClick={toggleAutoMode}
              className={`${isAutoMode 
                ? 'bg-gradient-to-r from-red-500 to-red-600 hover:from-red-600 hover:to-red-700' 
                : 'bg-gradient-to-r from-purple-500 to-purple-600 hover:from-purple-600 hover:to-purple-700'
              } text-white px-6 py-3 rounded-lg font-medium transition-all duration-200 flex items-center gap-2 shadow-lg hover:shadow-xl`}
            >
              {isAutoMode ? <Pause className="w-5 h-5" /> : <Play className="w-5 h-5" />}
              {isAutoMode ? 'Stop Auto Mode' : 'Start Auto Mode'}
            </button>

            <button
              onClick={clearEvents}
              className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white px-6 py-3 rounded-lg font-medium transition-all duration-200 flex items-center gap-2 shadow-lg hover:shadow-xl"
            >
              <RotateCcw className="w-5 h-5" />
              Clear Events
            </button>
          </div>
        </div>

        {/* System Architecture */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
          {/* Camera 1 System */}
          <div className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20">
            <h3 className="text-xl font-semibold text-white mb-4 flex items-center gap-2">
              <Camera className="w-6 h-6 text-blue-400" />
              CAMERA-001 System
            </h3>
            <div className="space-y-3">
              <div className="flex items-center gap-3 p-3 bg-blue-500/20 rounded-lg border border-blue-500/30">
                <Monitor className="w-5 h-5 text-blue-400" />
                <span className="text-white font-medium">NVR-1 Service</span>
                <span className="text-xs bg-blue-500 text-white px-2 py-1 rounded">Routing Key: NVR-1</span>
              </div>
              <div className="flex items-center gap-3 p-3 bg-blue-500/20 rounded-lg border border-blue-500/30">
                <HardDrive className="w-5 h-5 text-blue-400" />
                <span className="text-white font-medium">Storage-1 Service</span>
                <span className="text-xs bg-blue-500 text-white px-2 py-1 rounded">Routing Key: Storage-1</span>
              </div>
            </div>
          </div>

          {/* Camera 2 System */}
          <div className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20">
            <h3 className="text-xl font-semibold text-white mb-4 flex items-center gap-2">
              <Camera className="w-6 h-6 text-green-400" />
              CAMERA-002 System
            </h3>
            <div className="space-y-3">
              <div className="flex items-center gap-3 p-3 bg-green-500/20 rounded-lg border border-green-500/30">
                <Monitor className="w-5 h-5 text-green-400" />
                <span className="text-white font-medium">NVR-2 Service</span>
                <span className="text-xs bg-green-500 text-white px-2 py-1 rounded">Routing Key: NVR-2</span>
              </div>
              <div className="flex items-center gap-3 p-3 bg-green-500/20 rounded-lg border border-green-500/30">
                <HardDrive className="w-5 h-5 text-green-400" />
                <span className="text-white font-medium">Storage-2 Service</span>
                <span className="text-xs bg-green-500 text-white px-2 py-1 rounded">Routing Key: Storage-2</span>
              </div>
            </div>
          </div>
        </div>

        {/* Live Events Feed */}
        <div className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20">
          <h3 className="text-xl font-semibold text-white mb-4 flex items-center gap-2">
            <Zap className="w-6 h-6 text-yellow-400" />
            Live Event Stream
            {events.length > 0 && (
              <span className="text-sm bg-yellow-500 text-black px-2 py-1 rounded-full">
                {events.length}
              </span>
            )}
          </h3>
          
          <div className="space-y-3 max-h-96 overflow-y-auto">
            {events.length === 0 ? (
              <div className="text-center py-8 text-slate-400">
                <Zap className="w-12 h-12 mx-auto mb-3 opacity-50" />
                <p>No events yet. Click a camera button to trigger an event!</p>
              </div>
            ) : (
              events.map((event, index) => (
                <div
                  key={event.id}
                  className={`transform transition-all duration-500 ${
                    index === 0 ? 'scale-105' : 'scale-100'
                  }`}
                >
                  <div className={`p-4 rounded-lg border-2 ${
                    event.status === 'publishing' 
                      ? 'border-yellow-400 bg-yellow-400/20 animate-pulse' 
                      : 'border-green-400 bg-green-400/20'
                  }`}>
                    <div className="flex items-center justify-between mb-2">
                      <div className="flex items-center gap-3">
                        <Camera className={`w-5 h-5 ${
                          event.cameraId === 'CAMERA-001' ? 'text-blue-400' : 'text-green-400'
                        }`} />
                        <span className="text-white font-medium">{event.cameraId}</span>
                        <span className={`px-3 py-1 rounded-full text-white text-sm ${eventColors[event.eventType]}`}>
                          {event.eventType}
                        </span>
                      </div>
                      <div className="flex items-center gap-2">
                        <span className={`px-2 py-1 rounded text-xs ${
                          event.status === 'publishing' 
                            ? 'bg-yellow-500 text-black' 
                            : 'bg-green-500 text-white'
                        }`}>
                          {event.status === 'publishing' ? 'Publishing...' : 'Delivered'}
                        </span>
                        <span className="text-slate-300 text-sm">
                          {event.timestamp.toLocaleTimeString()}
                        </span>
                      </div>
                    </div>
                    
                    <div className="flex items-center gap-2 flex-wrap">
                      <span className="text-slate-300 text-sm">Routed to:</span>
                      {event.interestedParties.map((party, idx) => (
                        <span
                          key={idx}
                          className={`px-2 py-1 rounded text-xs font-medium ${
                            party.includes('NVR') 
                              ? 'bg-purple-500 text-white' 
                              : 'bg-orange-500 text-white'
                          }`}
                        >
                          {party.includes('NVR') ? <Monitor className="w-3 h-3 inline mr-1" /> : <HardDrive className="w-3 h-3 inline mr-1" />}
                          {party}
                        </span>
                      ))}
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Legend */}
        <div className="mt-6 bg-white/5 backdrop-blur-lg rounded-xl p-4 border border-white/10">
          <h4 className="text-white font-medium mb-3">Event Types:</h4>
          <div className="flex flex-wrap gap-3">
            {eventTypes.map(type => (
              <span key={type} className={`px-3 py-1 rounded-full text-white text-sm ${eventColors[type]}`}>
                {type}
              </span>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default CameraEventVisualizer;