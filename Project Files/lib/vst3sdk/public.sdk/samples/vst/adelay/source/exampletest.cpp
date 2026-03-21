//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/adelay/source/exampletest.cpp
// Created by  : Steinberg, 10/2010
// Description : 
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "adelaycontroller.h"
#include "adelayprocessor.h"
#include "base/source/fstring.h"
#include "public.sdk/source/main/moduleinit.h"
#include "public.sdk/source/vst/testsuite/vsttestsuite.h"
#include "public.sdk/source/vst/utility/testing.h"
#include "pluginterfaces/base/funknownimpl.h"

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
static ModuleInitializer InitTests ([] () {
	registerTest ("ExampleTest", nullptr, [] (FUnknown* context, ITestResult* testResult) {
		auto plugProvider = U::cast<ITestPlugProvider> (context);
		if (plugProvider)
		{
			auto controller = plugProvider->getController ();
			auto testController = U::cast<IDelayTestController> (controller);
			if (!controller)
			{
				testResult->addErrorMessage (String ("Unknown IEditController"));
				return false;
			}
			bool result = testController->doTest ();
			plugProvider->releasePlugIn (nullptr, controller);

			return (result);
		}
		return false;
	});
});

//------------------------------------------------------------------------
}} // namespaces
