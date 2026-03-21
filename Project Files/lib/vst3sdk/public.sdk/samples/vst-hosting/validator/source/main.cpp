//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Validator
// Filename    : public.sdk/samples/vst-hosting/validator/source/main.cpp
// Created by  : Steinberg, 04/2005
// Description : main entry point
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "validator.h"
#include "public.sdk/source/vst/utility/stringconvert.h"

void* moduleHandle = nullptr;
extern bool InitModule ();
extern bool DeinitModule ();

//------------------------------------------------------------------------
int run (int argc, char* argv[])
{
	InitModule ();

	auto result = Steinberg::Vst::Validator (argc, argv).run ();

	DeinitModule ();

	return result;
}

//------------------------------------------------------------------------
#if SMTG_OS_WINDOWS
//------------------------------------------------------------------------
using Utf8String = std::string;

//------------------------------------------------------------------------
using Utf8Args = std::vector<Utf8String>;
Utf8Args toUtf8Args (int argc, wchar_t* wargv[])
{
	Utf8Args utf8Args;
	for (int i = 0; i < argc; i++)
	{
		auto str = reinterpret_cast<const Steinberg::Vst::TChar*> (wargv[i]);
		utf8Args.push_back (Steinberg::Vst::StringConvert::convert (str));
	}

	return utf8Args;
}

//------------------------------------------------------------------------
using Utf8ArgPtrs = std::vector<char*>;
Utf8ArgPtrs toUtf8ArgPtrs (Utf8Args& utf8Args)
{
	Utf8ArgPtrs utf8ArgPtrs;
	for (auto& el : utf8Args)
	{
		utf8ArgPtrs.push_back (el.data ());
	}

	return utf8ArgPtrs;
}

//------------------------------------------------------------------------
int wmain (int argc, wchar_t* wargv[])
{
	Utf8Args utf8Args = toUtf8Args (argc, wargv);
	Utf8ArgPtrs utf8ArgPtrs = toUtf8ArgPtrs (utf8Args);

	char** argv = &(utf8ArgPtrs.at (0));
	return run (argc, argv);
}

#else

//------------------------------------------------------------------------
int main (int argc, char* argv[])
{
	return run (argc, argv);
}

#endif // SMTG_OS_WINDOWS
