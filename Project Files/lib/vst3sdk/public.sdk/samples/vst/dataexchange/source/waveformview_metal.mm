//-----------------------------------------------------------------------------
// Project     : VST SDK
// Flags       : clang-format SMTGSequencer
// Category    : Examples
// Filename    : public.sdk/samples/vst/dataexchange/waveformview_metal.mm
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

#import "shared.h"

#import "nanovg.h"
#import "nanovg_mtl.h"
#import "vstgui/contrib/externalview_metal.h"
#import "waveformview.h"
#import <CoreAudio/CoreAudio.h>
#import <mutex>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {

using namespace VSTGUI;

//------------------------------------------------------------------------
struct MetalLayerView : ExternalView::ExternalNSViewBase<NSView>
{
	CAMetalLayer* metalLayer {nullptr};

	MetalLayerView () : Base ([NSView new])
	{
		metalLayer = [CAMetalLayer new];
		view.layer = metalLayer;
		metalLayer.geometryFlipped = YES;
		metalLayer.opaque = NO;
		metalLayer.contentsGravity = kCAGravityBottomLeft;
		metalLayer.device = MTLCreateSystemDefaultDevice ();
	}

	void setContentScaleFactor (double scaleFactor) override
	{
		metalLayer.contentsScale = scaleFactor;
		auto size = view.bounds.size;
		size.width *= scaleFactor;
		size.height *= scaleFactor;
		metalLayer.drawableSize = size;
	}

	void setViewSize (IntRect frame, IntRect visible) override
	{
		Base::setViewSize (frame, visible);
		auto size = view.bounds.size;
		size.width *= metalLayer.contentsScale;
		size.height *= metalLayer.contentsScale;
		metalLayer.drawableSize = size;
	}
};

//------------------------------------------------------------------------
std::pair<VSTGUI::CExternalView*, NVGcontext*> WaveformViewManager::createNanoVGViewAndContext ()
{
	auto metalView = std::make_shared<MetalLayerView> ();
	auto externalView = new CExternalView ({}, metalView);
	auto context = nvgCreateMTL (metalView->metalLayer, NVG_ANTIALIAS);
	mnvgClearWithColor (context, nvgRGBA (0, 0, 0, 255));
	return {externalView, context};
}

//------------------------------------------------------------------------
void WaveformViewManager::releaseNanoVGContext (NVGcontext* context)
{
	nvgDeleteMTL (context);
}

//------------------------------------------------------------------------
bool WaveformViewManager::preRender (VSTGUI::CExternalView* view, NVGcontext* context)
{
	mnvgClearWithColor (context, nvgRGBA (0, 0, 0, 255));
	return true;
}

//------------------------------------------------------------------------
bool WaveformViewManager::postRender (VSTGUI::CExternalView* view, NVGcontext* context)
{
	return true;
}

//------------------------------------------------------------------------
struct DisplayLinkRenderThread : WaveformViewManager::IRenderThread
{
	static DisplayLinkRenderThread& instance ()
	{
		static DisplayLinkRenderThread thread;
		return thread;
	}

	void start (ThreadFunc&& f) final
	{
		assert (_displayLink == nullptr);
		func = std::move (f);
		auto result = CVDisplayLinkCreateWithActiveCGDisplays (&_displayLink);
		assert (result == kCVReturnSuccess);
		result = CVDisplayLinkSetOutputCallback (_displayLink, displayLinkRender, this);
		assert (result == kCVReturnSuccess);
		result = CVDisplayLinkSetCurrentCGDisplay (_displayLink, CGMainDisplayID ());
		assert (result == kCVReturnSuccess);
		CVDisplayLinkStart (_displayLink);
	}

	void stop () final
	{
		assert (_displayLink != nullptr);
		CVDisplayLinkStop (_displayLink);
		CVDisplayLinkRelease (_displayLink);
		_displayLink = nullptr;
	}

private:
	CVDisplayLinkRef _displayLink {nullptr};
	ThreadFunc func;

	static CVReturn displayLinkRender (CVDisplayLinkRef displayLink, const CVTimeStamp* now,
	                                   const CVTimeStamp* outputTime, CVOptionFlags flagsIn,
	                                   CVOptionFlags* flagsOut, void* displayLinkContext)
	{
		auto This = reinterpret_cast<DisplayLinkRenderThread*> (displayLinkContext);
		This->func ();
		return kCVReturnSuccess;
	}
};

//------------------------------------------------------------------------
auto WaveformViewManager::createRenderThread () -> IRenderThread&
{
	return DisplayLinkRenderThread::instance ();
}

//------------------------------------------------------------------------
} // Vst
} // Steinberg
