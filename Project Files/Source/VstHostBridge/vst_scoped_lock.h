#pragma once

#include <windows.h>

// Scoped RAII lock for a runtime's api_lock CRITICAL_SECTION.
// Works with any runtime type that has a public api_lock member.
template<typename RuntimeT>
struct ScopedRuntimeApiLock
{
	explicit ScopedRuntimeApiLock(RuntimeT* target_runtime)
		: runtime(target_runtime)
	{
		if (runtime)
			EnterCriticalSection(&runtime->api_lock);
	}

	~ScopedRuntimeApiLock()
	{
		if (runtime)
			LeaveCriticalSection(&runtime->api_lock);
	}

	RuntimeT* runtime;
};

// Non-blocking try-lock variant for the audio thread.
template<typename RuntimeT>
struct ScopedTryRuntimeApiLock
{
	explicit ScopedTryRuntimeApiLock(RuntimeT* target_runtime)
		: runtime(target_runtime)
		, locked(FALSE)
	{
		if (runtime)
			locked = TryEnterCriticalSection(&runtime->api_lock);
	}

	~ScopedTryRuntimeApiLock()
	{
		if (locked && runtime)
			LeaveCriticalSection(&runtime->api_lock);
	}

	bool is_locked() const
	{
		return locked != FALSE;
	}

	RuntimeT* runtime;
	BOOL locked;
};
