#include <Windows.h>

#include "host_process.h"

int WINAPI wWinMain(HINSTANCE instance, HINSTANCE, PWSTR command_line, int)
{
	return RunVstAudioHost(instance, command_line);
}
