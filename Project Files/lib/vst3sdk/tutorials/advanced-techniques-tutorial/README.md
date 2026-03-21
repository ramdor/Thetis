#  Advanced Techniques Tutorial Plug-in

---

## How to build

### macOS

        mkdir build
        cmake -GXcode -Dvst3sdk_SOURCE_DIR="PATH_TO_YOUR_VST_SDK_FOLDER" ../
        cmake --build .

### Windows

        mkdir build
        cmake -Dvst3sdk_SOURCE_DIR="PATH_TO_YOUR_VST_SDK_FOLDER" ..\
        cmake --build .

### Linux

        mkdir build
        cmake -Dvst3sdk_SOURCE_DIR="PATH_TO_YOUR_VST_SDK_FOLDER" ../
        cmake --build .

---

## Tutorial - Advanced Techniques

In this tutorial you will learn:

- How to add nearly sample accurate parameter changes to an audio effect
- Using C++ templates to write one algorithm supporting 32 bit and 64 bit audio processing
- Setting the state of the audio effect in a thread safe manner

---

## Part 1: Sample accurate parameter handling

We will start by looking at this process function:

``` c++
void MyEffect::process (ProcessData& data)
{
    handleParameterChanges (data.inputParameterChanges);

    // get the gain value for this block
    ParamValue gain = gainParameter.getValue ();

    // process audio
    AudioBusBuffers* inputs = data.inputs;
    AudioBusBuffers* outputs = data.outputs;
    for (auto channelIndex = 0; channelIndex < inputs[0].numChannels; ++channelIndex)
    {
        for (auto sampleIndex = 0; sampleIndex < data.numSamples; ++sampleIndex)
        {
            auto sample = inputs[0].channelBuffers32[channelIndex][sampleIndex];
            outputs[0].channelBuffers32[channelIndex][sampleIndex] = sample * gain;
        }
    }
}
```

This is straight and simple, we handle the parameter changes in the function *handleParameterChanges*, which we will see in a moment. Then we get the last gain parameter value and iterate over the input buffers and copy the samples from there to the output buffers and apply the *gain* factor.

If we look at the handleParameterChanges function:

``` c++
void MyEffect::handleParameterChanges (IParameterChanges*changes)
{
    if (!changes)
        return;
    int32 changeCount = changes->getParameterCount ();
    for (auto i = 0; i < changeCount; ++i)
    {
        if (auto queue = changes->getParameterData (i))
        {
            auto paramID = queue->getParameterId ();
            if (paramID == ParameterID::Gain)
            {
                int32 pointCount = queue->getPointCount ();
                if (pointCount > 0)
                {
                    int32 sampleOffset;
                    ParamValue value;
                    if (queue->getPoint (pointCount - 1, sampleOffset, value) == kResultTrue)
                        gainParameter.setValue (value);
                }
            }
        }
    }
}
```

We see that the *Gain* parameter only uses the last point for the gain value.

If we now want to use all points of the *Gain* parameter we can use two utility classes from the SDK.

The first one is the *ProcessDataSlicer* which slices the audio block into smaller peaces.

``` c++
void MyEffect::process (ProcessData& data)
{
    handleParameterChanges (data.inputParameterChanges);

    ProcessDataSlicer slicer (8);
    
    auto doProcessing = [this] (ProcessData& data) {
        // get the gain value for this block
        ParamValue gain = gainParameter.getValue ();

        // process audio
        AudioBusBuffers* inputs = data.inputs;
        AudioBusBuffers* outputs = data.outputs;
        for (auto channelIndex = 0; channelIndex < inputs[0].numChannels; ++channelIndex)
        {
            for (auto sampleIndex = 0; sampleIndex < data.numSamples; ++sampleIndex)
            {
                auto sample = inputs[0].channelBuffers32[channelIndex][sampleIndex];
                outputs[0].channelBuffers32[channelIndex][sampleIndex] = sample * gain;
            }
        }
    }
    
    slicer.process<SymbolicSampleSizes::kSample32> (data, doProcessing);
}
```

As you see we have moved the algorithm part into a lambda *doProcessing* which is passed to *slicer.process*. This lambda is now called multiple times with a maximum of 8 samples per call until the whole buffer is processed. This doesn't give us yet a better parameter resolution, but we can now use the second utility class to handle this.

At first we now look at the type of the *gainParameter* variable as this is our next utility class:

``` c++
SampleAccurate::Parameter gainParameter;
```

We have to change the *handleParameterChanges* function to:

``` c++
void MyEffect::handleParameterChanges (IParameterChanges*inputParameterChanges)
{
    int32 changeCount = inputParameterChanges->getParameterCount ();
    for (auto i = 0; i < changeCount; ++i)
    {
        if (auto queue = changes->getParameterData (i))
        {
            auto paramID = queue->getParameterId ();
            if (paramID == ParameterID::Gain)
            {
                gainParameter.beginChanges (queue);
            }
        }
    }
    
}
```

in order to delegate the handling of the parameter changes to the *gainParameter* object.

Now we just need another small change in the process lambda to use the nearly sample accurate *gain* value. We have to call the *gainParameter* object to *advance* the parameter value:

``` c++
auto doProcessing = [this] (ProcessData& data) {
    // get the gain value for this block
    ParamValue gain = gainParameter.advance (data.numSamples);

    // process audio
    AudioBusBuffers* inputs = data.inputs;
    AudioBusBuffers* outputs = data.outputs;
    for (auto channelIndex = 0; channelIndex < inputs[0].numChannels; ++channelIndex)
    {
        for (auto sampleIndex = 0; sampleIndex < data.numSamples; ++sampleIndex)
        {
            auto sample = inputs[0].channelBuffers32[channelIndex][sampleIndex];
            outputs[0].channelBuffers32[channelIndex][sampleIndex] = sample * gain;
        }
    }
}
```

Finally we have to do some cleanup of the *gainParameter* at the end of the *process* function by calling *gainParameter.endChanges*.

``` c++
void MyEffect::process (ProcessData& data)
{
    handleParameterChanges (data.inputParameterChanges);

    ProcessDataSlicer slicer (8);
    
    auto doProcessing = [this] (ProcessData& data) {
        // get the gain value for this block
        ParamValue gain = gainParameter.advance (data.numSamples);

        // process audio
        AudioBusBuffers* inputs = data.inputs;
        AudioBusBuffers* outputs = data.outputs;
        for (auto channelIndex = 0; channelIndex < inputs[0].numChannels; ++channelIndex)
        {
            for (auto sampleIndex = 0; sampleIndex < data.numSamples; ++sampleIndex)
            {
                auto sample = inputs[0].channelBuffers32[channelIndex][sampleIndex];
                outputs[0].channelBuffers32[channelIndex][sampleIndex] = sample * gain;
            }
        }
    }
    
    slicer.process<SymbolicSampleSizes::kSample32> (data, doProcessing);
    
    gainParameter.endChanges ();
}
```

Now we have nearly sample accurate parameter changes support in this example. Every 8 samples the *gain* parameter will be updated to the correct value.

It's very simple to make this 100% sample accurate, check out the **AGain sample accurate** example in the SDK.

---

## Part 2: Adding 32 and 64 bit audio processing

The example currently only supports 32 bit processing. Now we will add 64 bit processing.

As you may have noticed above the *ProcessDataSlicer* uses a template parameter for its process function. This template parameter *SampleSize* defines the bit depth of the audio buffers in the *ProcessData* structure. This is currently hard-coded to be *SymbolicSampleSizes::kSample32*.

In order to support *SymbolicSampleSizes::kSample64* we only have to make a few changes to the code. First we adopt the algorithm part by introducing a new templated method to our effect:

``` c++
template <SymbolicSampleSizes SampleSize>
void MyEffect::process (ProcessData& data)
{
}
```

We mostly just move the code from the original process method to this one except the code for handling parameter changes:

``` c++
template <SymbolicSampleSizes SampleSize>
void MyEffect::process (ProcessData& data)
{
    ProcessDataSlicer slicer (8);
    
    auto doProcessing = [this] (ProcessData& data) {
        // get the gain value for this block
        ParamValue gain = gainParameter.advance (data.numSamples);

        // process audio
        AudioBusBuffers* inputs = data.inputs;
        AudioBusBuffers* outputs = data.outputs;
        for (auto channelIndex = 0; channelIndex < inputs[0].numChannels; ++channelIndex)
        {
            for (auto sampleIndex = 0; sampleIndex < data.numSamples; ++sampleIndex)
            {
                auto sample = inputs[0].channelBuffers32[channelIndex][sampleIndex];
                outputs[0].channelBuffers32[channelIndex][sampleIndex] = sample * gain;
            }
        }
    }
    
    slicer.process<SampleSize> (data, doProcessing);
}
```

We just change the template parameter *SampleSize* of the process method of the *ProcessDataSlicer* to use the same template parameter as of our own process function.

This will not work correctly yet as we still work with the 32 bit audio buffers in our *doProcessing* lambda. In order to fix this we have to introduce two more templated functions *getChannelBuffers* that will choose the correct audio buffers depending on the *SampleSize* template parameter, which can either be *SymbolicSampleSizes::kSample32* or *SymbolicSampleSizes::kSample64*:

``` c++
template <SymbolicSampleSizes SampleSize,
        typename std::enable_if<SampleSize == SymbolicSampleSizes::kSample32>::type* = nullptr>
inline Sample32** getChannelBuffers (AudioBusBuffers& buffer)
{
    return buffer.channelBuffers32;
}

template <SymbolicSampleSizes SampleSize,
        typename std::enable_if<SampleSize == SymbolicSampleSizes::kSample64>::type* = nullptr>
inline Sample64** getChannelBuffers (AudioBusBuffers& buffer)
{
    return buffer.channelBuffers64;
}
```

Now we can change our *doProcessing* algorithm to use these functions:

``` c++
template <SymbolicSampleSizes SampleSize>
void MyEffect::process (ProcessData& data)
{
    ProcessDataSlicer slicer (8);
    
    auto doProcessing = [this] (ProcessData& data) {
        // get the gain value for this block
        ParamValue gain = gainParameter.advance (data.numSamples);

        // process audio
        AudioBusBuffers* inputs = data.inputs;
        AudioBusBuffers* outputs = data.outputs;
        for (auto channelIndex = 0; channelIndex < inputs[0].numChannels; ++channelIndex)
        {
            auto inputBuffers = getChannelBuffers<SampleSize> (inputs[0])[channelIndex];
            auto outputBuffers = getChannelBuffers<SampleSize> (outputs[0])[channelIndex];
            for (auto sampleIndex = 0; sampleIndex < data.numSamples; ++sampleIndex)
            {
                auto sample = inputBuffers[sampleIndex];
                outputBuffers[sampleIndex] = sample * gain;
            }
        }
    };
    
    slicer.process<SampleSize> (data, doProcessing);
}
```

As a final step we now need to call the templated *process<...>* function from the normal *process* function:

``` c++
void MyEffect::process (ProcessData& data)
{
    handleParameterChanges (data.inputParameterChanges);

    if (processSetup.symbolicSampleSize == SymbolicSampleSizes::kSample32)
        process<SymbolicSampleSizes::kSample32> (data);
    else
        process<SymbolicSampleSizes::kSample64> (data);
    
    gainParameter.endChanges ();
}
```

Depending on the *processSetup.symbolicSampleSize* we either call the 32 bit *process* function or the 64 bit *process* function.

We just have to inform the host that we can process 64 bit:

``` c++
tresult PLUGIN_API MyEffect::canProcessSampleSize (int32symbolicSampleSize)
{
    return (symbolicSampleSize == SymbolicSampleSizes::kSample32 ||
            symbolicSampleSize == SymbolicSampleSizes::kSample64) ?
            kResultTrue :
            kResultFalse;
}
```

Now we have sample accurate parameter changes and 32 and 64 bit audio processing.

---

## Part 3: Thread safe state changes

One common issue in this domain is that the plug-in state coming from a preset or a DAW project is set by the host from a non realtime thread.

If we want to change our internal data model to use this state we have to transfer this state to the realtime thread. This should be done in a realtime thread safe manner otherwise the model may not reflect the correct state as parameter changes dispatched in the realtime thread and the state data set on another thread will end in an undefined state.

For this case we have another utility class: *RTTransferT*

This class expects to have a template parameter *StateModel* describing the state data. We create a simple struct as data model:

``` c++
struct StateModel
{
    double gain;
};

using RTTransfer = RTTransferT<StateModel>;
```

We use *RTTransfer* now as a member for our *MyEffect* class:

``` c++
class MyEffect : ....
{
    RTTransfer stateTransfer;
};
```

If we now get a new *state* from the host, we create a *newStateModel* and write the *stateGain* value into *model->gain* andpass it to the utility class *stateTransfer*:

``` c++
tresult PLUGIN_API MyEffect::setState (IBStream* state)
{
    double stateGain = ... // read this out of the state stream
    
    StateModel model = std::make_unique<StateModel> ();
    model->gain = stateGain;
    
    stateTransfer.transferObject_ui (std::move (model));
    
    return kResultTrue;
}
```

To get the *stateModel* into our realtime thread we have to change the *process* function like this:

``` c++
void MyEffect::process (ProcessData& data)
{
    stateTransfer.accessTransferObject_rt ([this] (const auto& stateModel) {
        gainParameter.setValue (stateModel.gain);
    });
    
    handleParameterChanges (data.inputParameterChanges);

    if (processSetup.symbolicSampleSize == SymbolicSampleSizes::kSample32)
        process<SymbolicSampleSizes::kSample32> (data);
    else
        process<SymbolicSampleSizes::kSample64> (data);
    
    gainParameter.endChanges ();
    return kResultTrue;
}
```

The *accessTransferObject_rt* function will check if there is a new model state and will call the lambda if it is and then we can set our *gainParameter* to the value of *stateModel.gain*.

To free up the memory in the *stateTransfer* object we have to call the *clear_ui* method of it. In this case where we only have one double as state model it is OK to hold onto it until the next state is set or the effect is terminated. So we just add it to the *terminate* method of the plug-in:

``` c++
tresult PLUGIN_API MyEffect::terminate ()
{
    stateTransfer.clear_ui ();
    return AudioEffect::terminate ();
}
```

If the model data uses more memory and you want to get rid of it earlier you have to use a timer or similar to call the clear_ui method a little bit after the setState method was called. But this is not the scope of this tutorial.

If you want to use the utility classes, you will find them in the sdk at:

*public.sdk/source/vst/utility/processdataslicer.h*\
*public.sdk/source/vst/utility/sampleaccurate.h*\
*public.sdk/source/vst/utility/rttransfer.h*

That´s it!
