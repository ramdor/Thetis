//-----------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Validator
// Filename    : public.sdk/samples/vst-hosting/validator/source/testsuite.h
// Created by  : Steinberg, 05/2019
// Description : VST 3 Plug-in Validator test suite class
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "base/source/fobject.h"
#include "pluginterfaces/test/itest.h"
#include <vector>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
// TestSuite
//------------------------------------------------------------------------
class TestSuite : public ITestSuite, public FObject
{
public:
	TestSuite (FIDString _name) : name (_name) {}

	tresult PLUGIN_API addTest (FIDString _name, ITest* test) SMTG_OVERRIDE
	{
		tests.push_back (IPtr<Test> (NEW Test (_name, test), false));
		return kResultTrue;
	}

	tresult PLUGIN_API addTestSuite (FIDString _name, ITestSuite* testSuite) SMTG_OVERRIDE
	{
		testSuites.push_back (std::make_pair (_name, testSuite));
		return kResultTrue;
	}

	tresult PLUGIN_API setEnvironment (ITest* /*environment*/) SMTG_OVERRIDE
	{
		return kNotImplemented;
	}

	int32 getTestCount () const { return static_cast<int32> (tests.size ()); }

	tresult getTest (int32 index, ITest*& _test, std::string& _name) const
	{
		if (auto test = tests.at (index))
		{
			_test = test->test;
			_name = test->name;
			return kResultTrue;
		}
		return kResultFalse;
	}

	tresult getTestSuite (int32 index, ITestSuite*& testSuite, std::string& _name) const
	{
		if (index < 0 || index >= int32 (testSuites.size ()))
			return kInvalidArgument;
		const auto& ts = testSuites[index];
		_name = ts.first;
		testSuite = ts.second;
		return kResultTrue;
	}

	ITestSuite* getTestSuite (FIDString _name) const
	{
		for (const auto& testSuite : testSuites)
		{
			if (testSuite.first == _name)
				return testSuite.second;
		}
		return nullptr;
	}

	const std::string& getName () const { return name; }
	OBJ_METHODS (TestSuite, FObject)
	REFCOUNT_METHODS (FObject)
	DEF_INTERFACES_1 (ITestSuite, FObject)
protected:
	class Test : public FObject
	{
	public:
		Test (FIDString _name, ITest* _test) : name (_name), test (_test) {}

		std::string name;
		IPtr<ITest> test;
	};
	std::string name;
	std::vector<IPtr<Test>> tests;

	using TestSuitePair = std::pair<std::string, IPtr<ITestSuite>>;
	using TestSuiteVector = std::vector<TestSuitePair>;
	TestSuiteVector testSuites;
};

//------------------------------------------------------------------------
} // Vst
} // Steinberg
