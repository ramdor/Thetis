//-----------------------------------------------------------------------------
// Project     : VST SDK
// Flags       : clang-format SMTGSequencer
// Category    : Examples
// Filename    : public.sdk/samples/vst/dataexchange/waveformview.h
// Created by  : Steinberg, 06/2023
// Description : VST Data Exchange API Example
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "shared.h"
#include "vstgui/lib/ccolor.h"
#include "vstgui/lib/cexternalview.h"
#include "public.sdk/source/vst/utility/ringbuffer.h"
#include "public.sdk/source/vst/utility/systemtime.h"
#include <atomic>
#include <chrono>
#include <mutex>
#include <thread>

struct NVGcontext;

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
struct PeakValue
{
	float min {0.f};
	float max {0.f};
};

//------------------------------------------------------------------------
struct AudioBufferData
{
	int64 systemTime;
	SampleRate sampleRate;
	std::vector<PeakValue> peak;
};

//------------------------------------------------------------------------
class WaveformViewManager
{
public:
	using DrawCallback = std::function<void ()>;
	using DrawCallbackToken = uint64;

	struct IRenderThread
	{
		using ThreadFunc = std::function<void ()>;

		virtual void start (ThreadFunc&& f) = 0;
		virtual void stop () = 0;

		virtual ~IRenderThread () noexcept = default;
	};

	WaveformViewManager (SystemTime&& systemTime);
	WaveformViewManager (const SystemTime& systemTime);
	~WaveformViewManager () noexcept;

	VSTGUI::CExternalView* createNewView (VSTGUI::CColor leftChannel, VSTGUI::CColor rightChannel);

	bool pushAudioBufferData (AudioBufferData&& data)
	{
		return audioBufferDataQueue.push (std::move (data));
	}

	void renderIfNeeded ();

	double getFramesPerSeconds () const;

//------------------------------------------------------------------------
private:
	using AudioBufferDataQueue = OneReaderOneWriter::RingBuffer<AudioBufferData>;

	AudioBufferDataQueue audioBufferDataQueue {50u};
	SystemTime systemTime;

	struct Impl;
	std::unique_ptr<Impl> impl;

	struct GenericRenderThread : IRenderThread
	{
		GenericRenderThread () = default;

		void start (ThreadFunc&& f) override
		{
			running = true;
			thread = std::thread ([this, func = std::move (f)] () {
				using namespace std::chrono;
				using Clock = high_resolution_clock;

				constexpr microseconds syncInterval (16667);

				while (running)
				{
					auto startTime = Clock::now ();
					func ();
					auto stopTime = Clock::now ();
					auto duration = duration_cast<microseconds> (stopTime - startTime);
					if (duration < syncInterval)
						std::this_thread::sleep_for (syncInterval - duration);
				}
			});
		}

		void stop () override
		{
			if (!thread.joinable ())
				return;
			running = false;
			thread.join ();
		}

	private:
		std::thread thread;
		std::atomic<bool> running {false};
	};
//------------------------------------------------------------------------
	struct RenderThreadManager
	{
		static RenderThreadManager& instance ()
		{
			static RenderThreadManager m;
			return m;
		}

		DrawCallbackToken registerDrawCallback (DrawCallback&& callback)
		{
			auto token = ++tokenIndex;
			callbackMutex.lock ();
			callbacks.emplace_back (token, std::move (callback));
			auto callbacksSize = callbacks.size ();
			callbackMutex.unlock ();
			if (callbacksSize == 1)
				startThread ();
			return token;
		}

		void unregisterDrawCallback (DrawCallbackToken token)
		{
			callbackMutex.lock ();
			auto it = std::find_if (callbacks.begin (), callbacks.end (),
			                        [&] (const auto& el) { return el.first == token; });
			if (it != callbacks.end ())
				callbacks.erase (it);
			auto noCallbacks = callbacks.empty ();
			callbackMutex.unlock ();
			if (noCallbacks)
				stopThread ();
		}

	private:
		RenderThreadManager () : thread (createRenderThread ()) {}
		void startThread ()
		{
			using namespace std::chrono;
			using Clock = high_resolution_clock;

			thread.start ([this] () { renderAll (); });
		}

		void stopThread () { thread.stop (); }

		void renderAll ()
		{
			LockGuard g (callbackMutex);
			for (auto& dc : callbacks)
			{
				dc.second ();
			}
		}

		using Mutex = std::recursive_mutex;
		using LockGuard = std::lock_guard<Mutex>;

		std::vector<std::pair<DrawCallbackToken, DrawCallback>> callbacks;
		Mutex callbackMutex;
		std::atomic<DrawCallbackToken> tokenIndex {0};
		IRenderThread& thread;
	};

//------------------------------------------------------------------------
	static std::pair<VSTGUI::CExternalView*, NVGcontext*> createNanoVGViewAndContext ();
	static void releaseNanoVGContext (NVGcontext* context);
	static bool preRender (VSTGUI::CExternalView* view, NVGcontext* context);
	static bool postRender (VSTGUI::CExternalView* view, NVGcontext* context);
	static IRenderThread& createRenderThread ();
};

//------------------------------------------------------------------------
struct FramesPerSeconds
{
	using Clock = std::chrono::steady_clock;

	void reset ()
	{
		frameCounter = 0;
		startTime = Clock::now ();
	}

	void increaseDrawCount () { ++frameCounter; }

	double get () const
	{
		auto duration =
		    std::chrono::duration_cast<std::chrono::milliseconds> (Clock::now () - startTime)
		        .count ();
		auto fps = (frameCounter * 1000.) / static_cast<double> (duration);
		startTime = Clock::now ();
		frameCounter = 0;
		return fps;
	}

private:
	mutable std::atomic<uint32> frameCounter {0};
	mutable std::chrono::time_point<Clock> startTime {Clock::now ()};
};

//------------------------------------------------------------------------
} // Vst
} // Steinberg
