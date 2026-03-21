#  Data Exchange Tutorial Plug-in

This tutorial shows how to use the *Data Exchange API* via the backwards compatible wrapper class which either uses the API directly if available or uses an alternative IMessage based method to provide the same functionality for hosts not implementing the API.

In this example the audio processor sends the samples it processes to the controller in 1 second big chunks.

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

## Tutorial - How to use the Data Exchange API

In this tutorial you learn how to use the *Data Exchange API* to send data from the realtime audio
process method to the edit controller of your plug-in.

### Sending data from the audio processor

Let's send data from the processor to the controller.

First we need to add the required include so that we can use the wrapper class. Add the following
include before you define your audio processor:

```c++
#include "public.sdk/source/vst/utility/dataexchange.h"
```

To prepare the AudioEffect class you need to overwrite the following methods:

```c++
tresult PLUGIN_API initialize (FUnknown* context) override;
tresult PLUGIN_API connect (Vst::IConnectionPoint* other) override;
tresult PLUGIN_API disconnect (Vst::IConnectionPoint* other) override;
tresult PLUGIN_API setActive (TBool state) override;
tresult PLUGIN_API canProcessSampleSize (int32 symbolicSampleSize) override;
tresult PLUGIN_API process (Vst::ProcessData& data) override;
```

And we also have to add the wrapper class as a member of our audio processor class:

```c++
std::unique_ptr<Vst::DataExchangeHandler> dataExchange;
```

Now we can initialize and configure the dataExchange. We do this in:

```c++
tresult PLUGIN_API DataExchangeProcessor::connect (Vst::IConnectionPoint* other)
{
    auto result = Vst::AudioEffect::connect (other);
    if (result == kResultTrue)
    {
        auto configCallback = [this] (Vst::DataExchangeHandler::Config& config,
                                      const Vst::ProcessSetup& setup) {
            Vst::SpeakerArrangement arr;
            getBusArrangement (Vst::BusDirections::kInput, 0, arr);
            numChannels = static_cast<uint16_t> (Vst::SpeakerArr::getChannelCount (arr));
            auto sampleSize = sizeof (float);

            config.blockSize = setup.sampleRate * numChannels * sampleSize + sizeof (DataBlock);
            config.numBlocks = 2;
            config.alignment = 32;
            config.userContextID = 0;
            return true;
        };

        dataExchange = std::make_unique<Vst::DataExchangeHandler> (this, configCallback);
        dataExchange->onConnect (other, getHostContext ());
    }
    return result;
}
```

The configration is done via the `configCallback`. In this example we configure the queue to have
a block size to store exactly 1 second of audio data of all the channels of the configured speaker 
arrangement of the input bus. We choose two for `numBlocks` because we send one block per second
and in this case two blocks should be enough. If the frequency you need to send the block is higher
you need to increase this value to prevent data drop outs.
The `configCallback` is called when the audio processor is activated.

The next thing we have to do is to call the dataExchange object when the edit controller is disconnected
and release the memory:

```c++
tresult PLUGIN_API DataExchangeProcessor::disconnect (Vst::IConnectionPoint* other)
{
    if (dataExchange)
    {
        dataExchange->onDisconnect (other);
        dataExchange.reset ();
    }
    return AudioEffect::disconnect (other);
}
```

And we also have to call the dataExchange object when the processor's state changes:

```c++
tresult PLUGIN_API DataExchangeProcessor::setActive (TBool state)
{
    if (state)
        dataExchange->onActivate (processSetup);
    else
        dataExchange->onDeactivate ();
    return AudioEffect::setActive (state);
}
```

Now we prepare the data that we want to send to the controller. To make this a little bit easier we
define a struct how this data should look like and move this into its own header "*dataexchange.h*":

```c++
// dataexchange.h

#pragma once

#include "public.sdk/source/vst/utility/dataexchange.h"
#include <cstdint>

namespace Steinberg::Tutorial {

struct DataBlock
{
    uint32_t sampleRate;
    uint16_t sampleSize;
    uint16_t numChannels;
    uint32_t numSamples;
    float samples[0];
};

} // Steinberg::Tutorial
```

So, we want to send the sample rate, size, the number of channels and the number of samples plus 
the actual samples to the controller.

To actually work with this `DataBlock` struct we introduce a little helper function we also add to
this header:

```c++
inline DataBlock* toDataBlock (const Vst::DataExchangeBlock& block)
{
    if (block.blockID != Vst::InvalidDataExchangeBlockID)
        return reinterpret_cast<DataBlock*> (block.data);
    return nullptr;
}
```

One thing is left to do before we can implement the sending of the data and that is that we need a 
member variable of the Vst::DataExchangeBlock struct where we store the actual block we work with
while processing the audio. So we add this to our processor definition:


```c++
class DataExchangeProcessor : public Vst::AudioEffect
{
private:
    Vst::DataExchangeBlock currentExchangeBlock {InvalidDataExchangeBlock};
}
```

Now lets start to implement the processing:

```c++
tresult PLUGIN_API DataExchangeProcessor::process (Vst::ProcessData& processData)
{
    if (processData.numSamples <= 0)
        return kResultTrue;
    return kResultTrue;
}
```

When there are no samples in the `processData` we jump out of the method directly.

Now the first thing we need to do is to acquire a new block from the `dataExchange` object:

```c++
tresult PLUGIN_API DataExchangeProcessor::process (Vst::ProcessData& processData)
{
    // ...
    if (currentExchangeBlock.blockID == Vst::InvalidDataExchangeBlockID)
        acquireNewExchangeBlock ();
    // ...
}
```

We only want to acquire a new block if we have't already. So we check for this first and then we
call the `acquireNewExchangeBlock` method that does this:

```c++
void DataExchangeProcessor::acquireNewExchangeBlock ()
{
    currentExchangeBlock = dataExchange->getCurrentOrNewBlock ();
    if (auto block = toDataBlock (currentExchangeBlock))
    {
        block->sampleRate = static_cast<uint32_t> (processSetup.sampleRate);
        block->numChannels = numChannels;
        block->sampleSize = sizeof (float);
        block->numSamples = 0;
    }
}
```

We ask the `dataExchange` object for a new block with `getCurrentOrNewBlock ()`, check if it is valid 
with a call to the previously defined function `toDataBlock` and fill it with the sample rate, the 
sample size and the number of channels.

Now back to our process function. We now write the samples from the input buffer into our block until
the block is filled with 1 second of audio data:

```c++
tresult PLUGIN_API DataExchangeProcessor::process (Vst::ProcessData& processData)
{
    // ...
    auto input = processData.inputs[0];
    auto output = processData.outputs[0];

    if (auto block = toDataBlock (currentExchangeBlock))
    {
        auto numSamples = static_cast<uint32> (processData.numSamples);
        while (numSamples > 0)
        {
            uint32 numSamplesFreeInBlock = block->sampleRate - block->numSamples;
            uint32 numSamplesToCopy = std::min<uint32> (numSamplesFreeInBlock, numSamples);
            for (auto channel = 0; channel < input.numChannels; ++channel)
            {
                auto blockChannelData = &block->samples[0] + block->numSamples;
                auto inputChannel =
                    input.channelBuffers32[channel] + (processData.numSamples - numSamples);
                memcpy (blockChannelData, inputChannel, numSamplesToCopy * sizeof (float));
            }
            block->numSamples += numSamplesToCopy;
            if (block->numSamples == block->sampleRate)
            {
                dataExchange->sendCurrentBlock ();
                acquireNewExchangeBlock ();
                block = toDataBlock (currentExchangeBlock);
                if (block == nullptr)
                    break;
            }
            numSamples -= numSamplesToCopy;
        }
    }
    // ...
}
```

When it is filled with 1 second of audio data we send the block with `dataExchange->sendCurrentBlock()`
and directly acquire a new block afterwards.

Finally we need to copy back the input audio buffers to the output audio buffers. The whole method
looks like this then:

```c++
tresult PLUGIN_API DataExchangeProcessor::process (Vst::ProcessData& processData)
{
    if (processData.numSamples <= 0)
        return kResultTrue;

    if (currentExchangeBlock.blockID == Vst::InvalidDataExchangeBlockID)
        acquireNewExchangeBlock ();

    auto input = processData.inputs[0];
    auto output = processData.outputs[0];

    if (auto block = toDataBlock (currentExchangeBlock))
    {
        auto numSamples = static_cast<uint32> (processData.numSamples);
        while (numSamples > 0)
        {
            uint32 numSamplesFreeInBlock = block->sampleRate - block->numSamples;
            uint32 numSamplesToCopy = std::min<uint32> (numSamplesFreeInBlock, numSamples);
            for (auto channel = 0; channel < input.numChannels; ++channel)
            {
                auto blockChannelData = &block->samples[0] + block->numSamples;
                auto inputChannel =
                    input.channelBuffers32[channel] + (processData.numSamples - numSamples);
                memcpy (blockChannelData, inputChannel, numSamplesToCopy * sizeof (float));
            }
            block->numSamples += numSamplesToCopy;
            if (block->numSamples == block->sampleRate)
            {
                dataExchange->sendCurrentBlock ();
                acquireNewExchangeBlock ();
                block = toDataBlock (currentExchangeBlock);
                if (block == nullptr)
                    break;
            }
            numSamples -= numSamplesToCopy;
        }
    }
    for (auto channel = 0; channel < input.numChannels; ++channel)
    {
        if (output.channelBuffers32[channel] != input.channelBuffers32[channel])
        {
            memcpy (output.channelBuffers32[channel], input.channelBuffers32[channel],
                    processData.numSamples * sizeof (float));
        }
        output.silenceFlags = input.silenceFlags;
    }

    return kResultOk;
}
```

### Receiving data in the edit controller

We prepare our edit controller by adding the inheritance of Vst::IDataExchangeReceiver and by
providing a few methods and adding a new Vst::DataExchangeReceiverHandler member:

```c++
class DataExchangeController : public Vst::EditControllerEx1,
                               public Vst::IDataExchangeReceiver
{
public:
    // ...
    tresult PLUGIN_API notify (Vst::IMessage* message) override;
    void PLUGIN_API queueOpened (Vst::DataExchangeUserContextID userContextID,
                                 uint32 blockSize,
                                 TBool& dispatchOnBackgroundThread) override;
    void PLUGIN_API queueClosed (Vst::DataExchangeUserContextID userContextID) override;
    void PLUGIN_API onDataExchangeBlocksReceived (Vst::DataExchangeUserContextID userContextID, 
                                                  uint32 numBlocks,
                                                  Vst::DataExchangeBlock* blocks, 
                                                  TBool onBackgroundThread) override;

    DEFINE_INTERFACES
        DEF_INTERFACE (Vst::IDataExchangeReceiver)
    END_DEFINE_INTERFACES (EditController)
    DELEGATE_REFCOUNT (EditController)
private:
    Vst::DataExchangeReceiverHandler dataExchange {this};
}
```

First we need to forward messages to the `DataExchangeReceiverHandler` so that it can process the
data exchange messages when the host does not support the native API:

```c++
tresult PLUGIN_API DataExchangeController::notify (Vst::IMessage* message)
{
    if (dataExchange.onMessage (message))
        return kResultTrue;
    return EditControllerEx1::notify (message);
}
```

And next we can implement the `IDataExchangeReceiver` methods:

```c++
void PLUGIN_API DataExchangeController::queueOpened (Vst::DataExchangeUserContextID userContextID,
                                                     uint32 blockSize,
                                                     TBool& dispatchOnBackgroundThread)
{
    FDebugPrint ("Data Exchange Queue opened.\n");
}

void PLUGIN_API DataExchangeController::queueClosed (Vst::DataExchangeUserContextID userContextID)
{
    FDebugPrint ("Data Exchange Queue closed.\n");
}

void PLUGIN_API DataExchangeController::onDataExchangeBlocksReceived (
    Vst::DataExchangeUserContextID userContextID, uint32 numBlocks, Vst::DataExchangeBlock* blocks,
    TBool onBackgroundThread)
{
    for (auto index = 0u; index < numBlocks; ++index)
    {
        auto dataBlock = toDataBlock (blocks[index]);
        FDebugPrint (
            "Received Data Block: SampleRate: %d, SampleSize: %d, NumChannels: %d, NumSamples: %d\n",
            dataBlock->sampleRate, static_cast<uint32_t> (dataBlock->sampleSize),
            static_cast<uint32_t> (dataBlock->numChannels),
            static_cast<uint32_t> (dataBlock->numSamples));
    }
}
```

The `onDataExchangeBlocksReceived()` method will be called whenever the processor has send a block.
You can now do whatever you want with the data.

If you want the data to be dispatched on a background thread you need to set the
`dispatchOnBackgroundThread` variable to true in the `queueOpened` method. 
