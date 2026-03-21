//-----------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Validator
// Filename    : public.sdk/samples/vst-hosting/validator/source/validator.h
// Created by  : Steinberg, 04/2005
// Description : VST 3 Plug-in Validator class
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "public.sdk/source/vst/hosting/hostclasses.h"
#include "public.sdk/source/vst/hosting/module.h"
#include "public.sdk/source/vst/hosting/pluginterfacesupport.h"
#include "public.sdk/source/vst/testsuite/vsttestsuite.h"
#include "base/source/fstring.h"
#include "pluginterfaces/base/ipluginbase.h"
#include "pluginterfaces/test/itest.h"
#include <iosfwd>

namespace Steinberg {
namespace Vst {

class IComponent;
class IEditController;

class VstModule;
class TestBase;
class TestSuite;

//------------------------------------------------------------------------
/** Main Class of Validator.
 * \defgroup Validator VST3 Validator
 */
class Validator : public FObject, public ITestResult, public IHostApplication
{
public:
//------------------------------------------------------------------------
	Validator (int argc, char* argv[]);
	~Validator () override;

	int run ();

//------------------------------------------------------------------------
	OBJ_METHODS (Validator, FObject)
	REFCOUNT_METHODS (FObject)

	tresult PLUGIN_API queryInterface (const char* _iid, void** obj) override;
//------------------------------------------------------------------------
protected:
	using Module = VST3::Hosting::Module;

	// ITestResult
	void PLUGIN_API addErrorMessage (const tchar* msg) override;
	void PLUGIN_API addMessage (const tchar* msg) override;

	// IHostApplication
	tresult PLUGIN_API getName (String128 name) override;
	tresult PLUGIN_API createInstance (TUID cid, TUID iid, void** obj) override;

	IPtr<TestSuite> createTests (ITestPlugProvider* plugProvider, const ConstString& plugName,
	                             bool extensive);
	void addTest (ITestSuite* testSuite, TestBase* test);
	void runTestSuite (TestSuite* suite, FIDString nameFilter = nullptr);

	struct ModuleTestConfig
	{
		ModuleTestConfig (bool useGlobalInstance, bool useExtensiveTests,
		                  std::string& customTestComponentPath, std::string& testSuiteName,
		                  VST3::Optional<VST3::UID>&& testProcessor)
		: useGlobalInstance (useGlobalInstance)
		, useExtensiveTests (useExtensiveTests)
		, customTestComponentPath (customTestComponentPath)
		, testSuiteName (testSuiteName)
		, testProcessor (std::move (testProcessor))
		{
		}

		bool useGlobalInstance {true};
		bool useExtensiveTests {false};
		std::string customTestComponentPath;
		std::string testSuiteName;
		VST3::Optional<VST3::UID> testProcessor;
	};

	void testModule (Module::Ptr module, const ModuleTestConfig& config);

	int argc;
	char** argv;

	IPtr<PlugInterfaceSupport> mPlugInterfaceSupport;

	int32 numTestsFailed {0};
	int32 numTestsPassed {0};
	bool addErrorWarningTextToOutput {true};

	std::ostream* infoStream {nullptr};
	std::ostream* errorStream {nullptr};
};

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
