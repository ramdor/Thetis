//------------------------------------------------------------------------
// Copyright(c) 2023 Steinberg Media Technologies.
//------------------------------------------------------------------------

#include "pids.h"
#include "public.sdk/source/vst/vsteditcontroller.h"
#include "base/source/fstreamer.h"

//------------------------------------------------------------------------
namespace Steinberg::Tutorial {

using namespace Steinberg::Vst;

//------------------------------------------------------------------------
class Controller : public EditController
{
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	tresult PLUGIN_API setComponentState (IBStream* state) SMTG_OVERRIDE;
};

//------------------------------------------------------------------------
tresult PLUGIN_API Controller::initialize (FUnknown* context)
{
	tresult result = EditController::initialize (context);
	if (result != kResultOk)
	{
		return result;
	}
	parameters.addParameter (STR ("Gain"), STR ("%"), 0, 1., ParameterInfo::kCanAutomate,
	                         ParameterID::Gain);
	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API Controller::setComponentState (IBStream* state)
{
	if (!state)
		return kInvalidArgument;

	IBStreamer streamer (state, kLittleEndian);

	ParamValue value;
	if (!streamer.readDouble (value))
		return kResultFalse;

	if (auto param = parameters.getParameter (ParameterID::Gain))
		param->setNormalized (value);
	return kResultTrue;
}

//------------------------------------------------------------------------
FUnknown* createControllerInstance (void*)
{
	return static_cast<IEditController*> (new Controller);
}

//------------------------------------------------------------------------
} // Steinberg::Tutorial

